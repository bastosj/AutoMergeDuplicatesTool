# Auto Merge Duplicates Tool

Auto Merge Duplicates Tool enables you to automatically run your duplicate detection rules against your database and immediately merge the identified records based on a given criteria (e.g.: keep most recent accounts as the "master record"). This is a visual interface (Windows Forms) for you to select duplicate detection rules to apply, run duplicate detection validations against your data to identify duplicate rows, and then determine which ones to keep as "master" and which ones to deactivate. 

The solution is built leveraging the platform's API and OOTB behavior, meaning that when merging, the tool will trigger OOTB relationship behavior to automatically assign/share/associate records with the appropriate parent records.

## Development 
This tool has been developed to extend native functionality within Dynamics 365. While the duplicate detection system in place has a lot of features, it does not provide capability to automatically merge duplicate records found using the duplicate detection engine. This tool leverages that engine and provides an interface to automate the duplicate detection features and enable you to merge records automatically. This solution includes capability to handle large volumes of data (over the standard 5.000 record limit) by leveraging the API to do the merging through batch multiple requests.

## Setup
There are a few steps to essentially get this solution to work:
1. Configure App.config with a valid connection string to your Dataverse environment
2. Create and publish your duplicate detection rules for a given Table prior to running the tool. NB: Set always "exclude inactive matching records" to true
3. Run the application on a machine that can reach your target environment



## Dependencies
  - (At least) .NET Framework 4.6.2 is installed prior to running the application
  - Authentication is configured (through app.config) for a given Dataverse environment


## User Guide

For a quick glimpse of the application working, please see Quick Guide below. For detailed instructions and a general overview on application configuration, see below chapters.

### Quick Guide

This is a guide if you want to quickly test the application features

1. Configure the application with the credentials and CRM URL.
2. Run the application.
3. Select a table to look for its duplicates.
4. Confirm that the intended duplicate detection rules are published.
5. Click **Apply Rules** (and then wait for both process bars to be complete);
    - In case of more than 5.000 duplicate records are found, press OK in the warning message
6. Check the **Records Found** and **Groups** processed for the duplicate records
7. Create a FetchXML expression that would define the rules for the master records and paste it in the FetchXML textbox.
    - This FetchXML can be generated from the Advanced Find in CRM.
8. **Click Merge Data**. A confirmation dialog is displayed enquiring the confirmation of the master rules. Click Yes once confirmed.
9. Check the progress of the merge requests in the progress bar.
    - Note that a Batch Running label is displayed with the number of the batch that is currently running. A new duplicate record batch is processed for every 5.000 records found.
10. Once the duplicate detection job is complete, a dialog is displayed informing of its completion. Click **OK**.
11. Check the displayed statistics for information on elapsed time for the operations.
12. Confirm the data has been correctly merged on CRM for the master records, whilst the slave records have been disabled.

### ------- Detailed Guide -------
### Choose a Table

When the user starts the application, it prompts a window displaying a selection box and a table should be picked from the list. This is the table type that will be used to query the duplicate records.
 
![image](https://user-images.githubusercontent.com/16020111/113573769-b24e8480-9612-11eb-90c7-244792fb3bc3.png)

### Published Rules for this Table

The above selection will trigger a section where the published rules for the given table are displayed, and a button to apply those rules to retrieve all the duplicate records from Dataverse.

![image](https://user-images.githubusercontent.com/16020111/113573809-c4c8be00-9612-11eb-8218-e4970b64ae71.png)
![image](https://user-images.githubusercontent.com/16020111/113573821-ca260880-9612-11eb-8b37-ba3cd7076c79.png)


Once the **Apply Rules** button is clicked, if more than 5.000 duplicate records are found in CRM for the published duplicate detection rules, it will prompt the user with a warning stating this process will be performed in batches of 5.000 duplicate records each (see next chapter Define how masters are found);


### Define how master records are determined

The user should fill in a **FetchXML expression** that defines how the master records are determined in Dataverse.
For each group of duplicate records found, the tool uses this FetchXML expression to determine the master record and the ones that would be deactivated.

When the **Merge Data** button is clicked, data will be queried and groups of records created, for each group of duplicate records found. Note that for the pasted FetchXML expression, the columns are ignored by the application as it queries all columns for the table, to be able to find differences between the master and slave records. 

Fields that are null or empty in the master records, but contain data in slave records, will be mapped from the slave records into the master records.

A list containing the newly defined master records and the respective slave records is then processed by the application and proceeds with multiple merge requests for the data.

In case more than 5.000 records were found in the initial query, new batches of duplicate detection jobs will be performed for every other 5.000 records found. This process is automated in the application and does not require user input. If after each batch of 5.000 records, one of its records still has duplicates in other batches, the application will identify them as soon as its duplicates are caught.

![image](https://user-images.githubusercontent.com/16020111/113574463-0f970580-9614-11eb-8c88-2e5dbad07562.png)



