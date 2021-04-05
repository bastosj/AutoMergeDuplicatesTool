using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace AutoMergeDuplicatesTool
{
    /// <summary>
    /// Class responsible for all accesses to the CRM instance.
    /// Contains the connection and the various functions to apply the Duplicate Rules for duplicate records in CRM
    /// Methods are invoked from the UI
    /// </summary>
    public static class CRMAccess
    {
        #region Global Parameters & CRM Connection

        internal static CrmServiceClient _crmServiceClient;
        // Initialization of the service and service proxy instances
        internal static IOrganizationService _orgService;
        // Global list for duplicates
        internal static List<List<Guid>> listDuplicates = new List<List<Guid>>();
        // Max Fetch Count to be retrieved
        internal static int intFetchCount = 5000; //an int but it's working as a "shared" const
        // Max bulk messages for each ExecuteMultipleRequest
        const int cMaxBulkSize = 100;
        internal static TimeSpan tsTimeoutProxy = new TimeSpan(0, 10, 0);

        // Name of the internal job performed by CRM to find duplicates
        internal const string cDDJobMessage = "Auto Merge Duplicates Job: ";

        /// <summary>
        /// Opens the connection to the CRM instance
        /// </summary>
        internal static void OpenConnection()
        {
            try
            {
                // Connect to the CRM web service using a connection string.
                var connectionString = ConfigurationManager.ConnectionStrings["CrmConnection"];
                if (connectionString == null) { throw new InvalidPluginExecutionException("Invalid Connection String. Please contact your Administrator"); }

                _crmServiceClient = new CrmServiceClient(connectionString.ConnectionString);
                _crmServiceClient.OrganizationServiceProxy.Timeout = tsTimeoutProxy;

                // Cast the proxy client to the IOrganizationService interface.
                _orgService = (IOrganizationService)_crmServiceClient.OrganizationServiceProxy != null
                    ? (IOrganizationService)_crmServiceClient.OrganizationServiceProxy : null;

                if (_orgService == null) { throw new InvalidCastException("Could not connect to Dynamics 365. Please contact your Administrator"); }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Connection problems with Dynamics 365: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Closed connection to the CRM instance
        /// </summary>
        internal static void CloseConnection()
        {
            if (_crmServiceClient != null)
            {
                _crmServiceClient.Dispose();
            }
        }

        #endregion

        #region FetchXML - Get Master Ids (from filters)

        /// <summary>
        /// Sets the Master and Slave records grouped on a dictionary according to rules defined in the query.
        /// </summary>
        /// <param name="query">FetchXML query input from the application window</param>
        /// <param name="form">Mainform of the application</param>
        /// <returns> Dictionary contains the master entity as the Key element, and a list of the slave entities on the Value element.</returns>
        internal static Dictionary<Entity, EntityCollection> SetMasterEntities(string query)
        {
            // Dictionary will contain the master Guid and a list of Slave Guids according to the applied rules
            Dictionary<Entity, EntityCollection> dictEntitiesToMerge = new Dictionary<Entity, EntityCollection>();

            // Get list of master entities
            EntityCollection masterEntitiesEC = GetMasterEntities(query);

            //auxiliary list to manage which slaves have been added
            List<Guid> listGuidSlaves = new List<Guid>();
            List<Guid> listAuxGuids = new List<Guid>();
            Guid[] arraySlaves = null;

            // Auxiliary list of slave entities to be aded to the dictionary
            EntityCollection slaveEntitiesEC = new EntityCollection();

            // After retrieving the Guids from the FetchXML rules, check if they are contained within the list of duplicates
            // If positive, the Guid is added as a master record and the remaining are added as slave records

            if (masterEntitiesEC != null && masterEntitiesEC.Entities.Any())
            {
                foreach (Entity ent in masterEntitiesEC.Entities)
                {
                    foreach (List<Guid> subList in listDuplicates)
                    {
                        if (subList.Contains(ent.Id) && !listGuidSlaves.Contains(ent.Id))
                        {
                            subList.Remove(ent.Id); //doesnt need to process this one since it has been paired
                            slaveEntitiesEC = new EntityCollection();

                            //get the entity object for each guid in the slaves list
                            foreach (Guid slaveGuid in subList)
                            {
                                listGuidSlaves.Add(slaveGuid);
                                if (masterEntitiesEC.Entities.Any(o => o.Id == slaveGuid))
                                {
                                    slaveEntitiesEC.Entities.Add(masterEntitiesEC.Entities.Where(o => o.Id == slaveGuid).FirstOrDefault());
                                }

                                // slave id not contained on the master entities collection
                                else
                                {
                                    // handle retrieve multiple with array of guids
                                    // where slaveGuids only added if they are not set as master records

                                    listAuxGuids.Add(slaveGuid);
                                }
                            }

                        }

                        else
                        {
                            continue;
                        }

                        arraySlaves = listAuxGuids.ToArray();

                        if (listAuxGuids.Any())
                        {
                            QueryExpression qExp = new QueryExpression(MainForm.cbValue);
                            qExp.NoLock = true;
                            qExp.ColumnSet = new ColumnSet(true);
                            qExp.Criteria.AddCondition(new ConditionExpression(MainForm.cbValueId, ConditionOperator.In, arraySlaves));
                            slaveEntitiesEC.Entities.AddRange(CRMAccess._orgService.RetrieveMultiple(qExp).Entities);
                        }


                        listAuxGuids.Clear();
                        dictEntitiesToMerge.Add(ent, slaveEntitiesEC);
                    }
                }
            }

            return dictEntitiesToMerge;
        }

        /// <summary>
        /// Retrieves all the entities from the FetchXML query, with the complete ColumnSet
        /// </summary>
        /// <param name="query">FetchXML query from the applicaiton window</param>
        /// <returns>Entity Collection with all the entity records from the FetchXML query</returns>
        internal static EntityCollection GetMasterEntities(string query)
        {
            Guid[] arrayDuplicates = null;
            List<Guid> listAux = new List<Guid>();

            foreach (var list in listDuplicates)
            {
                foreach (var guid in list)
                {
                    listAux.Add(guid);
                }
            }

            arrayDuplicates = listAux.ToArray();

            // We need to convert the FetchXML into a Query Expression to dinamically add an operator In condition
            // This strongly diminishes the number of requests to CRM
            // The In operator contains all Guids within the duplicates list

            QueryExpression queryExpression = ConvertFetchToQueryExp(query);

            if (listDuplicates.Any())
            {
                queryExpression.Criteria.AddCondition(new ConditionExpression(MainForm.cbValueId, ConditionOperator.In, arrayDuplicates));
            }

            queryExpression.NoLock = true;
            //Get all columns in order to be able to find differences and fill the empty fields in the master
            queryExpression.ColumnSet = new ColumnSet(true);

            EntityCollection masterEntitiesEC = new EntityCollection();
            if (queryExpression != null)
            {
                masterEntitiesEC = _orgService.RetrieveMultiple(queryExpression);
            }

            return masterEntitiesEC;
        }

        #endregion

        #region Duplicates

        /// <summary>
        /// Retrieves all the duplicate rules for a given entity name
        /// </summary>
        /// <param name="entity">Schema name of the entity</param>
        /// <returns>Entity Collection containing all the duplicate rules for the given entity</returns>
        internal static EntityCollection RetrieveDuplicateRules(string entity)
        {

            // Get all Published Duplicate Detection Rules
            string fetchDuplicateRules = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' no-lock='true'>
            <entity name='duplicaterule'>
            <all-attributes /> 
    <filter type='and'>
        <condition attribute='baseentityname' operator='eq' value='{0}' />
        <condition attribute='statecode' operator='eq' value='1' />
    </filter>
            </entity>
        </fetch>";

            EntityCollection duplicateRulesER = _orgService.RetrieveMultiple(new FetchExpression(string.Format(fetchDuplicateRules, entity)));

            return duplicateRulesER;
        }

        /// <summary>
        /// Processes the duplicates and groups them based on the base record ids.
        /// This method creates groups for the duplicates so they can then be processed as master and slave records.
        /// </summary>
        /// <param name="progress">Progress handler to measure the progress bar for this operation</param>
        /// <param name="duplicateRecordsEC">Entity Collection that contains all the duplicate records previously queried</param>
        /// <returns>Global List containing a List of Guids for every group of duplicate records</returns>
        internal static List<List<Guid>> ProcessDuplicates(IProgress<int> progress, EntityCollection duplicateRecordsEC)
        {
            // Variables used to calculate an approximate value of the percentage of completion for the duplicate job
            // This percentage is an approximate value since while the groups of master and slave records are being processed 
            // it might not be possible to accurately calculate the percentage
            decimal pct = 0;
            int z = 1;
            decimal count = duplicateRecordsEC.Entities.Count;

            foreach (var r in duplicateRecordsEC.Entities)
            {
                Guid baseRecordId = r.GetAttributeValue<EntityReference>("baserecordid").Id;

                z++;
                // Could pontentially not be an accurate calculation, read above
                pct = CalculatePercentage(z, count);
                progress.Report((int)pct);

                if (listDuplicates.Any(l => l.Any(x => x == baseRecordId)))
                {
                    continue;
                }

                // Here we will get a duplicate of the base record id

                Entity baseEntity = new Entity(MainForm.cbValue);
                baseEntity.Id = baseRecordId;
                var request = new RetrieveDuplicatesRequest
                {
                    BusinessEntity = baseEntity,
                    MatchingEntityName = baseEntity.LogicalName,
                    PagingInfo = new PagingInfo() { PageNumber = 1, Count = 50 }
                };

                var responseDuplicate = (RetrieveDuplicatesResponse)CRMAccess._orgService.Execute(request);
                EntityCollection responseDEC = (EntityCollection)responseDuplicate.Results.FirstOrDefault().Value;
                Guid duplicateRecordId = Guid.Empty;
                duplicateRecordId = responseDEC.Entities.FirstOrDefault().Id;

                // We have the duplicate
                // Add to the DuplicateGroup list

                List<Guid> existingList = listDuplicates.FirstOrDefault(l => l.Any(x => x == duplicateRecordId));

                if (existingList != null)
                {
                    existingList.Add(baseRecordId);
                }

                else
                {
                    List<Guid> listDuplicateGroup = new List<Guid>();
                    listDuplicateGroup.Add(baseRecordId);
                    listDuplicateGroup.Add(duplicateRecordId);
                    listDuplicates.Add(listDuplicateGroup);
                }
            }

            return listDuplicates;
        }

        /// <summary>
        /// Performs a Duplicate Detection Job for the given entity and retrieves all duplicate records
        /// </summary>
        /// <returns>Entity Collection with all the duplicate records for the performed job</returns>
        internal static EntityCollection GetDuplicates(string query)
        {
            #region Retrieve List of Duplicates

            QueryExpression qExpDuplicates = new QueryExpression();
            qExpDuplicates.ColumnSet = new ColumnSet(true);
            qExpDuplicates.EntityName = MainForm.cbValue;

            if (query != null && !string.IsNullOrEmpty(query) && !string.IsNullOrWhiteSpace(query))
            {
                qExpDuplicates = ConvertFetchToQueryExp(query);
            }


            BulkDetectDuplicatesRequest bulkDDRequest = new BulkDetectDuplicatesRequest()
            {
                JobName = cDDJobMessage + MainForm.cbValue,
                Query = qExpDuplicates,

                RecurrencePattern = String.Empty,
                RecurrenceStartTime = DateTime.Now,
                ToRecipients = new Guid[0],
                CCRecipients = new Guid[0],

            };

            BulkDetectDuplicatesResponse response = (BulkDetectDuplicatesResponse)CRMAccess._orgService
        .Execute(bulkDDRequest);

            // Checks success & Waits for the Job to finish

            CRMAccess.WaitForAsyncJobToFinish(response.JobId, 120);

            #endregion

            #region Fetch XML Paging

            string fetchDuplicateRecords = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' no-lock='true' count='{0}'>
                <entity name='duplicaterecord'>
            <attribute name='duplicateid' />
            <attribute name='baserecordid' />
            <filter type='and'>
              <condition attribute='asyncoperationid' operator='eq' value='{1}' />
            </filter>
            </entity>
            </fetch>";

            EntityCollection duplicateRecordsEC = new EntityCollection();
            EntityCollection drECAux = new EntityCollection();

            // Clear aux Entity Collection and Add Range to main
            drECAux = CRMAccess._orgService.RetrieveMultiple(new FetchExpression(string.Format(fetchDuplicateRecords, intFetchCount, response.JobId)));
            duplicateRecordsEC.Entities.AddRange(drECAux.Entities);

            #endregion

            return duplicateRecordsEC;
        }

        #endregion

        #region Merge Data

        /// <summary>
        /// Performs an ExecuteMultipleRequest on a OrganizationRequestCollection containing the Merge Requests for the provided Dictionary
        /// </summary>
        /// <param name="mergeArgs">Merge arguments class containing the schema name for the entity and the dictionary with the Master and Slave records</param>
        /// <param name="progress">Progress handler to measure the progress for this operation</param>
        public static void MergeData(Dictionary<Entity, EntityCollection> dictToMerge, IProgress<int> progress)
        {
            // Variables used to calculate the percentage of completion on the merge requests
            // Calculates the percentage for the number of requests already processes against the total number of requests

            decimal pct = 0;
            decimal countRequests = 0;
            int iNullCount = 0;
            OrganizationRequestCollection requestCollection = new OrganizationRequestCollection();

            foreach (var keyPair in dictToMerge)
            {
                foreach (Entity slaveEnt in keyPair.Value.Entities)
                {
                    // Create the target for the request.
                    EntityReference target = new EntityReference();

                    // Retrieve target and subordinate entities with all columns
                    Entity targetEntity = keyPair.Key != null && keyPair.Key.Id != null ? keyPair.Key : null;
                    Entity subordinate = slaveEnt != null && slaveEnt.Id != null ? slaveEnt : null;

                    // validate if master or slave records are null before proceeding
                    if (targetEntity == null || subordinate == null || targetEntity.Id == null || subordinate.Id == null)
                    {
                        iNullCount++;
                        continue;
                    }

                    // Get the differences between the attributes on the master and slave records
                    Entity updateContent = GetEntityAttributeDifferences(targetEntity, subordinate);

                    // Id is the GUID of the record that is being merged into.
                    // LogicalName is the type of the entity being merged to, as a string
                    target.Id = targetEntity.Id;
                    target.LogicalName = MainForm.cbValue;

                    // Create the request.
                    MergeRequest mergeRequest = new MergeRequest();
                    // SubordinateId is the GUID of the record merging.
                    mergeRequest.SubordinateId = subordinate.Id;
                    mergeRequest.Target = target;
                    mergeRequest.PerformParentingChecks = false;

                    // Set the content to be updated
                    mergeRequest.UpdateContent = updateContent;

                    // Add the requests to a requst collection so they can be processed on Execute Multiple Requests
                    requestCollection.Add(mergeRequest);
                }
            }

            // get the count of records to use in the percentage calculation
            countRequests = requestCollection.Count;

            if (requestCollection.Any())
            {
                OrganizationRequestCollection requestCollectionAux = new OrganizationRequestCollection();
                for (int i = 0; i < requestCollection.Count; i += cMaxBulkSize)
                {

                    requestCollectionAux.AddRange(requestCollection.Skip(i).Take(cMaxBulkSize));

                    ExecuteMultipleRequest requestWithResults = new ExecuteMultipleRequest()
                    {
                        // Assign settings that define execution behavior: continue on error, return responses. 
                        Settings = new ExecuteMultipleSettings()
                        {
                            ContinueOnError = false,
                            ReturnResponses = true
                        },
                        // Create an empty organization request collection.
                        Requests = requestCollectionAux
                    };

                    ExecuteMultipleResponse responseWithResults =
        (ExecuteMultipleResponse)CRMAccess._orgService.Execute(requestWithResults);

                    // Calculate percentage after the operation has concluded
                    pct = CalculatePercentage(i, countRequests);
                    progress.Report((int)pct);
                    requestCollectionAux.Clear();
                }
            }
        }
        #endregion

        #region Auxiliar Functions


        public static QueryExpression ConvertFetchToQueryExp(string query)
        {
            QueryExpression qExpAux = new QueryExpression();
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = query
            };
            var conversionResponse =
                            (FetchXmlToQueryExpressionResponse)_orgService.Execute(conversionRequest);

            qExpAux = conversionResponse.Query;
            return qExpAux;
        }

        /// <summary>
        /// Retrieves the differences in field values between Target and Subordinate entities
        /// </summary>
        /// <param name="target">Target entity to be compared</param>
        /// <param name="subordinate">Subordinate entity to be compared</param>
        /// <param name="schemaName">Schemaname of the entity</param>
        /// <returns>Entity containing the differences between field values</returns>
        public static Entity GetEntityAttributeDifferences(Entity target, Entity subordinate)
        {
            Entity eDifferences = new Entity(MainForm.cbValue);
            try
            {
                if (subordinate != null && target != null)
                {
                    foreach (var att in subordinate.Attributes)
                    {
                        if (!target.Contains(att.Key) && att.Key != "masterid")
                        {
                            if (att.Value is EntityReference)
                            {
                                // Remove the name from the Entity Ref
                                // CRM internally handles this property
                                if (((EntityReference)att.Value).Name != null)
                                {
                                    ((EntityReference)att.Value).Name = null;
                                }
                            }

                            eDifferences.Attributes.Add(att.Key, att.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR LOG: " + Environment.NewLine + ex.Message + Environment.NewLine + ex.GetType().ToString() + Environment.NewLine + ex.StackTrace);
            }

            return eDifferences;
        }

        /// <summary>
        /// Prints logs to a local file
        /// </summary>
        /// <param name="msg">Message to log</param>
        public static void print(String msg)
        {
            string path = @".\Log.log";

            File.AppendAllText(path, msg);

        }

        /// <summary>
        /// Basic percentage calculatin for two values
        /// </summary>
        /// <param name="x">First value</param>
        /// <param name="y">Second value</param>
        /// <returns>Percentage value</returns>
        public static int CalculatePercentage(int x, decimal y)
        {
            return (int)(x / y * 100);
        }


        /// <summary>
        /// Creates an XML file with the provided paging cookie
        /// </summary>
        /// <param name="xml">FetchXML string</param>
        /// <param name="cookie">Paging cookie</param>
        /// <param name="page">Number of the page</param>
        /// <param name="count">Count of records</param>
        /// <returns>Processes XML file containing the paging cookie</returns>
        public static string CreateXml(string xml, string cookie, int page, int count)
        {
            StringReader stringReader = new StringReader(xml);
            XmlTextReader reader = new XmlTextReader(stringReader);

            // Load document
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            return CreateXml(doc, cookie, page, count);
        }

        /// <summary>
        /// Invoked by CreateXml to create the formatted XML fetch query
        /// </summary>
        /// <param name="doc">XML document</param>
        /// <param name="cookie">Paging cookie</param>
        /// <param name="page">Number of the page</param>
        /// <param name="count">Count of records</param>
        /// <returns>Formatted XML string</returns>
        public static string CreateXml(XmlDocument doc, string cookie, int page, int count)
        {
            XmlAttributeCollection attrs = doc.DocumentElement.Attributes;

            if (cookie != null)
            {
                XmlAttribute pagingAttr = doc.CreateAttribute("paging-cookie");
                pagingAttr.Value = cookie;
                attrs.Append(pagingAttr);
            }

            XmlAttribute pageAttr = doc.CreateAttribute("page");
            pageAttr.Value = System.Convert.ToString(page);
            attrs.Append(pageAttr);

            XmlAttribute countAttr = doc.CreateAttribute("count");
            countAttr.Value = System.Convert.ToString(count);
            attrs.Append(countAttr);

            StringBuilder sb = new StringBuilder(1024);
            StringWriter stringWriter = new StringWriter(sb);

            XmlTextWriter writer = new XmlTextWriter(stringWriter);
            doc.WriteTo(writer);
            writer.Close();

            return sb.ToString();
        }

        /// <summary>
        /// Waits for the provided Async Duplicate Detection Job to be complete. 
        /// </summary>
        /// <param name="jobId">Id of the Job</param>
        /// <param name="maxSeconds">Max seconds interval</param>
        internal static void WaitForAsyncJobToFinish(Guid jobId, int maxSeconds)
        {
            for (int i = 0; i < maxSeconds; i++)
            {

                var asyncJob = _orgService.Retrieve("asyncoperation",

                        jobId, new ColumnSet("statecode"));

                if (asyncJob["statecode"] != null && ((OptionSetValue)asyncJob["statecode"]).Value == 3)

                    return;

                System.Threading.Thread.Sleep(1000);

            }
        }

        #endregion
    }
}
