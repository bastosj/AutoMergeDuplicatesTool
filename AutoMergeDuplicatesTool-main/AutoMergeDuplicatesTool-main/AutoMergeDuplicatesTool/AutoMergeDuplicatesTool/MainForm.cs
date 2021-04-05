using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk;
using System.Diagnostics;


namespace AutoMergeDuplicatesTool
{

    public partial class MainForm : Form
    {
        #region Constants

        // Constants to define steps of the process
        // Used to interact with the UI components
        private enum eProcessingSteps
        {
            Retrieving_Entities = 1,
            Retrieving_Rules = 2,
            Processing_List = 3,
            Processing_List_Done = 4,
            Processing_Groups = 5,
            Processing_Groups_Done = 6,
            Fetch_Applied = 7,
            Fetch_Complete = 8,
            Merge_Complete = 9,
            New_Batch = 10
        }

        private const int cDEFAULTNROFCALLS = 5;
        private const int cNROFCALLSNONCRMPROGRESS = 2;
        // Setting the timeout to a higher value than 2 mins to avoid timeouts during the Merge Process
        private const int cMAXDOTNUMBER = 5;

        // Adapt the message for when the Job is complete
        private const string cCompleteMessage = "Auto Merge is complete.";
        private DateTime trialDate = new DateTime(2015, 10, 11);
        #endregion

        #region Variables

        // Timers to measure performance percentages
        Stopwatch swDuplicates = new Stopwatch();
        Stopwatch swMerge = new Stopwatch();

        int intTotalNrOfFetchedRecords = 0;
        int intTotalNrOfGroups = 0;

        // Used to report the nr of batch currently running
        private int intBatchRunning = 1;
        private bool bAreThereMoreGroupsAfterThisOne = false;

        private List<List<Guid>> recordsList; //list of duplicate groups
        private Dictionary<Entity, EntityCollection> dictToMerge; //list of duplicate groups

        // Populated with the change of the selection box that determines the Entity schema name and Id
        internal static string cbValue = string.Empty;
        internal static string cbValueId = string.Empty;

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        /// <summary>
        /// Event responsible to load components when the application is first executed
        /// </summary>
        /// <param name="sender">Default sender</param>
        /// <param name="e">Default event arguments</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            //DateTime currentDate = new DateTime();
            //currentDate = TimeZoneAccess.InvokeTimeZone();

            //if (currentDate != DateTime.MinValue && currentDate > trialDate)
            //{
            //    throw new InvalidCastException("Trial is over. " 
            //        + Environment.NewLine + " Please contact the Dynamics CRM Administrator.");
            //}

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            CRMAccess.OpenConnection();
            // Setting the timeout to a higher value than 2 mins to avoid timeouts during the Merge Process
            //CRMAccess._serviceProxy.Timeout = CRMAccess.tsTimeoutProxy;
            PopulateCBEntities();
        }

        /// <summary>
        /// Dispose of the Connection when the application is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CRMAccess.CloseConnection();
        }

        /// <summary>
        /// Event responsible to handle the selection of different items on the entity combobox
        /// </summary>
        /// <param name="sender">Default sender</param>
        /// <param name="e">Default event arguments</param>
        private void cbEntity_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (!cb.Focused)
            {
                return;
            }

            if (cbEntity == null) { return; }
            cbValue = cbEntity.SelectedValue.ToString();
            cbValueId = cbValue + "id";
            if (String.IsNullOrEmpty(cbValue)) { return; }
            GetDuplicateDetectionRules();
            ManipulateUI(eProcessingSteps.Retrieving_Rules);
        }

        /// <summary>
        /// Event responsible to handle the click of Apply Rules button.
        /// Triggers the GetDuplicates and ProcessDuplicates functions on background threads
        /// Triggers UI behaviours accordingly to the process phase
        /// Reports time elapsed for the operations to the UI
        /// Reports progress of the operations to the UI
        /// </summary>
        /// <param name="sender">Default sender</param>
        /// <param name="e">Default event arguments</param>
        private async void btApplyRules_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Query duplicate records for " + cbValue + "?", "Query Duplicates", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (cbEntity.SelectedItem != null && cbEntity.SelectedIndex != -1) //if no entity is selected
                {
                    ManipulateUI(eProcessingSteps.Processing_List);

                    EntityCollection duplicateRecordsEC = await GetDuplicates(pbRetrievingList, lblRetrievingListValue);

                    if (duplicateRecordsEC.Entities.Count == 0)  // Return to initial state if no duplicates are found
                    {
                        MessageBox.Show("No duplicates were found for entity: " + cbEntity.SelectedValue);
                        ManipulateUI(eProcessingSteps.Retrieving_Rules);
                        return;
                    }

                    ManipulateUI(eProcessingSteps.Processing_List_Done);

                    if (duplicateRecordsEC.Entities.Count >= CRMAccess.intFetchCount)
                    {
                        MessageBox.Show("There are over " + CRMAccess.intFetchCount + " duplicate records."
                            + Environment.NewLine + "Merge operation will be performed by batches of " + CRMAccess.intFetchCount + " records each");
                    }

                    recordsList = await ProcessDuplicates(pbProcessing, lblProcessingValue, duplicateRecordsEC);

                    ManipulateUI(eProcessingSteps.Processing_Groups_Done);
                }
            }
        }

        /// <summary>
        /// Event responsible to handle the click of Merge Data button.
        /// Triggers the SetMasterEntities and MergeData functions on background threads
        /// Reports time elapsed for the operations in the UI
        /// Reports progress of the operations to the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btMerge_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFetchXMLMaster.Text) && !string.IsNullOrWhiteSpace(txtFetchXMLMaster.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Merge Data using the selected rules?", "Merge Data", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    // Enable required components for this phase
                    ManipulateUI(eProcessingSteps.Fetch_Applied);

                    // Get Dictionary with Master and Slave entities     
                    Dictionary<Entity, EntityCollection> dictToMerge = await SetMasterEntities();

                    // Enable count of processed records
                    ManipulateUI(eProcessingSteps.Fetch_Complete);

                    // Merge Data for this batch
                    await MergeData(pbMerge, lblMergeProgress, dictToMerge);

                    ManipulateUI(eProcessingSteps.Merge_Complete);

                    // Continue processing further batches
                    if (bAreThereMoreGroupsAfterThisOne)
                    {
                        ContinueProcessing();
                    }
                    else  // if there are no more batches
                    {
                        MessageBox.Show(cCompleteMessage);
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("Empty Fetch XML", "Please fill in the FetchXML to determine the Master records.");
            }
        }

        #endregion

        #region Populate UI

        /// <summary>
        /// Automatically populates the combo selection box with the entities available for merge operations in CRM
        /// </summary>
        private void PopulateCBEntities()
        {
            var dataSource = new List<EntityNames>();
            dataSource.Add(new EntityNames { Name = "Account", LogicalName = "account" });
            dataSource.Add(new EntityNames { Name = "Contact", LogicalName = "contact" });
            dataSource.Add(new EntityNames { Name = "Lead", LogicalName = "lead" });

            cbEntity.DataSource = dataSource;
            cbEntity.DisplayMember = "Name";
            cbEntity.ValueMember = "LogicalName";

            cbEntity.SelectedItem = null;
        }

        /// <summary>
        /// Automatically adds checkbox controls for each of the duplicate detection rules published in CRM
        /// </summary>
        /// <param name="list"></param>
        private void FillFlowPanel(EntityCollection list)
        {
            CheckBox box;
            int x = 1;
            flowPanelRules.Controls.Clear();
            foreach (var cursor in list.Entities)
            {
                box = new CheckBox();
                box.Tag = x;
                box.Text = cursor.Attributes["name"].ToString();
                box.AutoSize = true;
                box.Checked = true;
                box.Enabled = false;
                flowPanelRules.Controls.Add(box);
                x++;
            }
        }

        #endregion

        #region Manipulate Controls

        /// <summary>
        /// Handles the UI to display components for when the Apply Button is clicked
        /// </summary>
        private void DisplayNewBatchRules()
        {
            lblRetrievingListValue.SafeInvoke(l => l.Visible = true);
            lblProcessingValue.SafeInvoke(l => l.Visible = true);
        }

        /// <summary>
        /// Handles different components in the UI based on the step provided
        /// </summary>
        /// <param name="step">Step value provided for each of the process phases</param>
        private void ManipulateUI(eProcessingSteps step)
        {
            if (step == eProcessingSteps.Retrieving_Rules)
            {
                btApplyRules.SafeInvoke(b => b.Visible = true);
                lblStep2.SafeInvoke(l => l.Visible = true);
                flowPanelRules.SafeInvoke(f => f.Visible = true);
                cbEntity.SafeInvoke(c => c.Enabled = true);
                btApplyRules.SafeInvoke(b => b.Enabled = true);
            }
            else if (step == eProcessingSteps.Processing_List)
            {
                pbRetrievingList.SafeInvoke(p => p.Visible = true);
                lblRetrieving.SafeInvoke(l => l.Visible = true);
                lblRetrievingListValue.SafeInvoke(l => l.Visible = true);
                btApplyRules.SafeInvoke(b => b.Enabled = false);
                cbEntity.SafeInvoke(c => c.Enabled = false);
            }
            else if (step == eProcessingSteps.Processing_List_Done)
            {
                SetProgress(pbRetrievingList, lblRetrievingListValue, 100);
                lblProcessing.SafeInvoke(l => l.Visible = true);
                lblProcessingValue.SafeInvoke(l => l.Visible = true);
                pbProcessing.SafeInvoke(p => p.Visible = true);
            }

            else if (step == eProcessingSteps.Processing_Groups_Done)
            {
                lblStep3.SafeInvoke(l => l.Visible = true);
                lblRecordsFound.SafeInvoke(l => l.Visible = true);
                lblNumberOfRecords.SafeInvoke(l => l.Visible = true);
                lblFetchXML.SafeInvoke(l => l.Visible = true);
                lblFetchXML2.SafeInvoke(l => l.Visible = true);
                txtFetchXMLMaster.SafeInvoke(t => t.Visible = true);
                btMerge.SafeInvoke(b => b.Visible = true);
                lblElapsedRules.SafeInvoke(l => l.Visible = true);
                lblETRulesValue.SafeInvoke(l => l.Visible = true);

                SetProgress(pbProcessing, lblProcessingValue, 100);
                lblProcessingValue.SafeInvoke(l => l.Visible = true);
                lblETRulesValue.SafeInvoke(l => l.Text = GetTimeElapsed(swDuplicates));
            }
            else if (step == eProcessingSteps.Fetch_Applied)
            {
                lblBatchValue.Text = intBatchRunning.ToString();
                lblMergeProgress.SafeInvoke(l => l.Visible = true);
                lblElapsedMerge.SafeInvoke(l => l.Visible = true);
                pbMerge.SafeInvoke(p => p.Visible = true);
                lblBatchTitle.SafeInvoke(l => l.Visible = true);
                lblBatchValue.SafeInvoke(l => l.Visible = true);
                btMerge.SafeInvoke(b => b.Enabled = false);
            }
            else if (step == eProcessingSteps.Fetch_Complete)
            {
                SetCountStats(recordsList);
            }
            else if (step == eProcessingSteps.Merge_Complete)
            {
                SetProgress(pbMerge, lblMergeProgress, 100);
                lblETMerge.SafeInvoke(l => l.Visible = true);
                lblETMerge.SafeInvoke(l => l.Text = GetTimeElapsed(swMerge));
            }
            else if (step == eProcessingSteps.New_Batch)
            {
                ClearUI();
                lblBatchValue.SafeInvoke(l => l.Text = intBatchRunning.ToString());
                lblBatchValue.SafeInvoke(l => l.Refresh());
            }
        }

        #endregion

        #region Batch Recurrence

        /// <summary>
        /// Enables continue processing for batches where the number of duplicare records is greater than 5000
        /// Creates recurrence between the processes to automate the merge of records for the various batches
        /// </summary>
        private async void ContinueProcessing()
        {
            if (bAreThereMoreGroupsAfterThisOne)
            {
                // Query another batch of duplicates
                // Awaits completion of the Task before proceeding                
                // While there are records in a new batch, create recurrence
                while (await NewBatchProcess())
                {
                    // Get Dictionary with Master and Slave entities     
                    Dictionary<Entity, EntityCollection> dictToMerge = await SetMasterEntities();
                    // Enable count of processed records
                    ManipulateUI(eProcessingSteps.Fetch_Complete);
                    // Get Dictionary with Master and Slave entities                   
                    await MergeData(pbMerge, lblMergeProgress, dictToMerge);
                    ManipulateUI(eProcessingSteps.Merge_Complete);

                    if (!bAreThereMoreGroupsAfterThisOne)
                    {
                        MessageBox.Show(cCompleteMessage);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles whether or not there are further batches to be processed
        /// </summary>
        /// <returns>Task to run this operation on a background Thread</returns>
        private async Task<bool> NewBatchProcess()
        {
            CRMAccess.listDuplicates = new List<List<Guid>>(); //cleans the global list
            // Set Nr of Batch running
            intBatchRunning++;
            ManipulateUI(eProcessingSteps.New_Batch);
            EntityCollection duplicateRecordsEC = await GetDuplicates(pbRetrievingList, lblRetrievingListValue);
            ManipulateUI(eProcessingSteps.Processing_List_Done);
            recordsList = await ProcessDuplicates(pbProcessing, lblProcessingValue, duplicateRecordsEC);
            ManipulateUI(eProcessingSteps.Processing_Groups_Done);
            lblETRulesValue.SafeInvoke(l => l.Text = GetTimeElapsed(swDuplicates));
            return duplicateRecordsEC.Entities.Count > 0;
        }

        #endregion

        #region Auxiliary Functions

        /// <summary>
        /// Sets the progress for a given progress bar and label
        /// </summary>
        /// <param name="pb">Progress Bar control</param>
        /// <param name="lb">Label control</param>
        /// <param name="value">Value to be set</param>
        private void SetProgress(ProgressBar pb, System.Windows.Forms.Label lb, int value)
        {
            lb.SafeInvoke(l => l.Text = value + "%");
            lb.SafeInvoke(l => l.Refresh());
            pb.SafeInvoke(p => p.Value = value);
        }

        /// <summary>
        /// Gets time elapsed for a given Stopwatch and formats it into a Timespan HH:MM:SS
        /// </summary>
        /// <param name="sw">Stopwatch</param>
        /// <returns></returns>
        private string GetTimeElapsed(Stopwatch sw)
        {
            TimeSpan ts = new TimeSpan();
            ts = sw.Elapsed;
            return String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

        }

        /// <summary>
        /// Clears the UI progress bars and label values
        /// </summary>
        private void ClearUI()
        {
            SetProgress(pbRetrievingList, lblRetrievingListValue, 50);
            SetProgress(pbProcessing, lblProcessingValue, 0);
            SetProgress(pbMerge, lblMergeProgress, 0);
        }

        /// <summary>
        /// Initiates the progress bar handler
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="lb"></param>
        private IProgress<int> InitProgressHandler(ProgressBar pb, System.Windows.Forms.Label lb)
        {
            var progressHandler = new Progress<int>(value =>
            {
                if (value >= 0 && value <= 100)
                {
                    SetProgress(pb, lb, value);
                }
            });
            return progressHandler as IProgress<int>;
        }

        /// <summary>
        /// Invokes the Retrieve function and fills in the published duplicate detection rules
        /// </summary>
        /// <param name="entity">Entity to be queried</param>
        private void GetDuplicateDetectionRules()
        {
            EntityCollection duplicateRulesER = CRMAccess.RetrieveDuplicateRules(cbValue);
            FillFlowPanel(duplicateRulesER);
        }

        /// <summary>
        /// Automates calculation of timers for the statistics
        /// </summary>
        /// <param name="recordsList">List of duplicates</param>
        public void SetCountStats(List<List<Guid>> recordsList)
        {
            intTotalNrOfFetchedRecords += (dictToMerge.Keys.Count + dictToMerge.SelectMany(x => x.Value.Entities).Count());
            intTotalNrOfGroups += dictToMerge.Count;
            lblNumberOfRecords.Text = intTotalNrOfFetchedRecords + "    Groups: " + intTotalNrOfGroups;
        }

        #endregion

        #region CRM Access Interaction

        /// <summary>
        /// Async method that invokes the CRMAccess method GetDuplicates
        /// Runs on a background Thread
        /// </summary>
        /// <param name="pb">Progress bar for this operation</param>
        /// <param name="lb">Progress label for this operation</param>
        /// <returns></returns>
        private async Task<EntityCollection> GetDuplicates(ProgressBar pb, System.Windows.Forms.Label lb)
        {
            var progress = InitProgressHandler(pb, lb);
            swDuplicates.Start(); // Start the the timer for the duplicate process - It will be stopped once all duplicates are retrieved in the ProcessDuplicates function
            SetProgress(pb, lb, 50);
            return await Task.Run(() =>
            {
                EntityCollection ents = CRMAccess.GetDuplicates(string.Empty);
                bAreThereMoreGroupsAfterThisOne = ents.Entities.Count >= CRMAccess.intFetchCount;
                return ents;
            });
        }

        /// <summary>
        /// Async method that invokes the CRMAccess method ProcessDuplicates
        /// Runs on a background Thread
        /// </summary>
        /// <param name="pb">Progress bar for this operation</param>
        /// <param name="lb">Progress label for this operation</param>
        /// <param name="duplicateRecordsEC">Entity Collection containing all the duplicate records for a given batch</param>
        /// <returns>Global list of duplicate records</returns>
        private async Task<List<List<Guid>>> ProcessDuplicates(ProgressBar pb, System.Windows.Forms.Label lb, EntityCollection duplicateRecordsEC)
        {
            // Initiates the progress handler for this operation
            var progress = InitProgressHandler(pb, lb);
            return recordsList = await Task.Run(() =>
            {
                var list = CRMAccess.ProcessDuplicates(progress, duplicateRecordsEC);
                swDuplicates.Stop(); //stops the timer
                return list;
            });
        }

        /// <summary>
        /// Async method that invokes the CRMAccess method SetMasterEntities
        /// Runs on a background Thread
        /// </summary>
        /// <returns>Returns a dictionary containing the master records and the slave records</returns>
        private async Task<Dictionary<Entity, EntityCollection>> SetMasterEntities()
        {
            swMerge.Start(); // Start the the timer for the duplicate process - It will be stopped once all duplicates are retrieved in the ProcessDuplicates function
            return dictToMerge = await Task.Run(() =>
                {
                    return CRMAccess.SetMasterEntities(txtFetchXMLMaster.Text);
                });
        }

        /// <summary>
        /// Async method that invokes the CRMAccess method MergeData
        /// Runs on a background Thread
        /// </summary>
        /// <param name="pb">Progress bar for this operation</param>
        /// <param name="lb">Progress label for this operation</param>
        /// <param name="dictToMerge">Dictionary with the master and slave records</param>
        /// <returns></returns>
        private async Task MergeData(ProgressBar pb, System.Windows.Forms.Label lb, Dictionary<Entity, EntityCollection> dictToMerge)
        {
            // Initiates the progress handler for this operation
            var progress = InitProgressHandler(pb, lb);
            await Task.Run(() =>
                {
                    CRMAccess.MergeData(dictToMerge, progress);
                    swMerge.Stop(); //stops the timer
                });
        }

        #endregion

    }
}
