namespace AutoMergeDuplicatesTool
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbEntity = new System.Windows.Forms.ComboBox();
            this.btApplyRules = new System.Windows.Forms.Button();
            this.lblRecordsFound = new System.Windows.Forms.Label();
            this.txtFetchXMLMaster = new System.Windows.Forms.TextBox();
            this.lblFetchXML = new System.Windows.Forms.Label();
            this.lblStep1 = new System.Windows.Forms.Label();
            this.lblStep2 = new System.Windows.Forms.Label();
            this.flowPanelRules = new System.Windows.Forms.FlowLayoutPanel();
            this.lblRetrieving = new System.Windows.Forms.Label();
            this.lblRetrievingListValue = new System.Windows.Forms.Label();
            this.lblStep3 = new System.Windows.Forms.Label();
            this.lblProcessing = new System.Windows.Forms.Label();
            this.lblProcessingValue = new System.Windows.Forms.Label();
            this.lblNumberOfRecords = new System.Windows.Forms.Label();
            this.btMerge = new System.Windows.Forms.Button();
            this.lblElapsedRules = new System.Windows.Forms.Label();
            this.lblETRulesValue = new System.Windows.Forms.Label();
            this.lblETMerge = new System.Windows.Forms.Label();
            this.pbMerge = new System.Windows.Forms.ProgressBar();
            this.lblMergeProgress = new System.Windows.Forms.Label();
            this.pbProcessing = new System.Windows.Forms.ProgressBar();
            this.lblBatchTitle = new System.Windows.Forms.Label();
            this.lblBatchValue = new System.Windows.Forms.Label();
            this.pbRetrievingList = new System.Windows.Forms.ProgressBar();
            this.lblFetchXML2 = new System.Windows.Forms.Label();
            this.lblElapsedMerge = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbEntity
            // 
            this.cbEntity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEntity.FormattingEnabled = true;
            this.cbEntity.Location = new System.Drawing.Point(14, 41);
            this.cbEntity.Name = "cbEntity";
            this.cbEntity.Size = new System.Drawing.Size(121, 21);
            this.cbEntity.TabIndex = 0;
            this.cbEntity.SelectedIndexChanged += new System.EventHandler(this.cbEntity_SelectedIndexChanged);
            // 
            // btApplyRules
            // 
            this.btApplyRules.Location = new System.Drawing.Point(10, 376);
            this.btApplyRules.Name = "btApplyRules";
            this.btApplyRules.Size = new System.Drawing.Size(75, 23);
            this.btApplyRules.TabIndex = 1;
            this.btApplyRules.Text = "Apply Rules";
            this.btApplyRules.UseVisualStyleBackColor = true;
            this.btApplyRules.Visible = false;
            this.btApplyRules.Click += new System.EventHandler(this.btApplyRules_Click);
            // 
            // lblRecordsFound
            // 
            this.lblRecordsFound.AutoSize = true;
            this.lblRecordsFound.Location = new System.Drawing.Point(387, 413);
            this.lblRecordsFound.Name = "lblRecordsFound";
            this.lblRecordsFound.Size = new System.Drawing.Size(86, 13);
            this.lblRecordsFound.TabIndex = 2;
            this.lblRecordsFound.Text = "Records Found: ";
            this.lblRecordsFound.Visible = false;
            // 
            // txtFetchXMLMaster
            // 
            this.txtFetchXMLMaster.Location = new System.Drawing.Point(390, 41);
            this.txtFetchXMLMaster.Multiline = true;
            this.txtFetchXMLMaster.Name = "txtFetchXMLMaster";
            this.txtFetchXMLMaster.Size = new System.Drawing.Size(465, 260);
            this.txtFetchXMLMaster.TabIndex = 3;
            this.txtFetchXMLMaster.Visible = false;
            // 
            // lblFetchXML
            // 
            this.lblFetchXML.AutoSize = true;
            this.lblFetchXML.Location = new System.Drawing.Point(320, 41);
            this.lblFetchXML.Name = "lblFetchXML";
            this.lblFetchXML.Size = new System.Drawing.Size(59, 13);
            this.lblFetchXML.TabIndex = 4;
            this.lblFetchXML.Text = "Fetch XML";
            this.lblFetchXML.Visible = false;
            // 
            // lblStep1
            // 
            this.lblStep1.AutoSize = true;
            this.lblStep1.Location = new System.Drawing.Point(12, 13);
            this.lblStep1.Name = "lblStep1";
            this.lblStep1.Size = new System.Drawing.Size(123, 13);
            this.lblStep1.TabIndex = 5;
            this.lblStep1.Text = "Step 1: Choose an entity";
            // 
            // lblStep2
            // 
            this.lblStep2.AutoSize = true;
            this.lblStep2.Location = new System.Drawing.Point(12, 95);
            this.lblStep2.Name = "lblStep2";
            this.lblStep2.Size = new System.Drawing.Size(177, 13);
            this.lblStep2.TabIndex = 7;
            this.lblStep2.Text = "Step 2: Published rules for this entity";
            this.lblStep2.Visible = false;
            // 
            // flowPanelRules
            // 
            this.flowPanelRules.AutoScroll = true;
            this.flowPanelRules.Location = new System.Drawing.Point(10, 124);
            this.flowPanelRules.Name = "flowPanelRules";
            this.flowPanelRules.Size = new System.Drawing.Size(291, 245);
            this.flowPanelRules.TabIndex = 8;
            this.flowPanelRules.Visible = false;
            // 
            // lblRetrieving
            // 
            this.lblRetrieving.AutoSize = true;
            this.lblRetrieving.Location = new System.Drawing.Point(103, 381);
            this.lblRetrieving.Name = "lblRetrieving";
            this.lblRetrieving.Size = new System.Drawing.Size(74, 13);
            this.lblRetrieving.TabIndex = 9;
            this.lblRetrieving.Text = "Retrieving List";
            this.lblRetrieving.Visible = false;
            // 
            // lblRetrievingListValue
            // 
            this.lblRetrievingListValue.AutoSize = true;
            this.lblRetrievingListValue.Location = new System.Drawing.Point(290, 380);
            this.lblRetrievingListValue.Name = "lblRetrievingListValue";
            this.lblRetrievingListValue.Size = new System.Drawing.Size(21, 13);
            this.lblRetrievingListValue.TabIndex = 10;
            this.lblRetrievingListValue.Text = "0%";
            this.lblRetrievingListValue.Visible = false;
            // 
            // lblStep3
            // 
            this.lblStep3.AutoSize = true;
            this.lblStep3.Location = new System.Drawing.Point(325, 10);
            this.lblStep3.Name = "lblStep3";
            this.lblStep3.Size = new System.Drawing.Size(186, 13);
            this.lblStep3.TabIndex = 11;
            this.lblStep3.Text = "Step 3: Define how Masters are found";
            this.lblStep3.Visible = false;
            // 
            // lblProcessing
            // 
            this.lblProcessing.AutoSize = true;
            this.lblProcessing.Location = new System.Drawing.Point(103, 405);
            this.lblProcessing.Name = "lblProcessing";
            this.lblProcessing.Size = new System.Drawing.Size(59, 13);
            this.lblProcessing.TabIndex = 12;
            this.lblProcessing.Text = "Processing";
            this.lblProcessing.Visible = false;
            // 
            // lblProcessingValue
            // 
            this.lblProcessingValue.AutoSize = true;
            this.lblProcessingValue.Location = new System.Drawing.Point(290, 406);
            this.lblProcessingValue.Name = "lblProcessingValue";
            this.lblProcessingValue.Size = new System.Drawing.Size(21, 13);
            this.lblProcessingValue.TabIndex = 13;
            this.lblProcessingValue.Text = "0%";
            this.lblProcessingValue.Visible = false;
            // 
            // lblNumberOfRecords
            // 
            this.lblNumberOfRecords.AutoSize = true;
            this.lblNumberOfRecords.Location = new System.Drawing.Point(479, 413);
            this.lblNumberOfRecords.Name = "lblNumberOfRecords";
            this.lblNumberOfRecords.Size = new System.Drawing.Size(13, 13);
            this.lblNumberOfRecords.TabIndex = 14;
            this.lblNumberOfRecords.Text = "..";
            this.lblNumberOfRecords.Visible = false;
            // 
            // btMerge
            // 
            this.btMerge.Location = new System.Drawing.Point(390, 307);
            this.btMerge.Name = "btMerge";
            this.btMerge.Size = new System.Drawing.Size(75, 23);
            this.btMerge.TabIndex = 16;
            this.btMerge.Text = "Merge Data";
            this.btMerge.UseVisualStyleBackColor = true;
            this.btMerge.Visible = false;
            this.btMerge.Click += new System.EventHandler(this.btMerge_Click);
            // 
            // lblElapsedRules
            // 
            this.lblElapsedRules.AutoSize = true;
            this.lblElapsedRules.Location = new System.Drawing.Point(387, 380);
            this.lblElapsedRules.Name = "lblElapsedRules";
            this.lblElapsedRules.Size = new System.Drawing.Size(144, 13);
            this.lblElapsedRules.TabIndex = 17;
            this.lblElapsedRules.Text = "Elapsed time to Apply Rules: ";
            this.lblElapsedRules.Visible = false;
            // 
            // lblETRulesValue
            // 
            this.lblETRulesValue.AutoSize = true;
            this.lblETRulesValue.Location = new System.Drawing.Point(537, 381);
            this.lblETRulesValue.Name = "lblETRulesValue";
            this.lblETRulesValue.Size = new System.Drawing.Size(13, 13);
            this.lblETRulesValue.TabIndex = 19;
            this.lblETRulesValue.Text = "0";
            this.lblETRulesValue.Visible = false;
            // 
            // lblETMerge
            // 
            this.lblETMerge.AutoSize = true;
            this.lblETMerge.Location = new System.Drawing.Point(537, 397);
            this.lblETMerge.Name = "lblETMerge";
            this.lblETMerge.Size = new System.Drawing.Size(13, 13);
            this.lblETMerge.TabIndex = 20;
            this.lblETMerge.Text = "0";
            this.lblETMerge.Visible = false;
            // 
            // pbMerge
            // 
            this.pbMerge.Location = new System.Drawing.Point(390, 346);
            this.pbMerge.Name = "pbMerge";
            this.pbMerge.Size = new System.Drawing.Size(465, 23);
            this.pbMerge.TabIndex = 21;
            this.pbMerge.Visible = false;
            // 
            // lblMergeProgress
            // 
            this.lblMergeProgress.AutoSize = true;
            this.lblMergeProgress.Location = new System.Drawing.Point(861, 351);
            this.lblMergeProgress.Name = "lblMergeProgress";
            this.lblMergeProgress.Size = new System.Drawing.Size(21, 13);
            this.lblMergeProgress.TabIndex = 22;
            this.lblMergeProgress.Text = "0%";
            this.lblMergeProgress.Visible = false;
            // 
            // pbProcessing
            // 
            this.pbProcessing.Location = new System.Drawing.Point(183, 402);
            this.pbProcessing.Name = "pbProcessing";
            this.pbProcessing.Size = new System.Drawing.Size(100, 23);
            this.pbProcessing.TabIndex = 23;
            this.pbProcessing.Visible = false;
            // 
            // lblBatchTitle
            // 
            this.lblBatchTitle.AutoSize = true;
            this.lblBatchTitle.Location = new System.Drawing.Point(574, 312);
            this.lblBatchTitle.Name = "lblBatchTitle";
            this.lblBatchTitle.Size = new System.Drawing.Size(81, 13);
            this.lblBatchTitle.TabIndex = 24;
            this.lblBatchTitle.Text = "Batch Running:";
            this.lblBatchTitle.Visible = false;
            // 
            // lblBatchValue
            // 
            this.lblBatchValue.AutoSize = true;
            this.lblBatchValue.Location = new System.Drawing.Point(661, 312);
            this.lblBatchValue.Name = "lblBatchValue";
            this.lblBatchValue.Size = new System.Drawing.Size(13, 13);
            this.lblBatchValue.TabIndex = 25;
            this.lblBatchValue.Text = "0";
            this.lblBatchValue.Visible = false;
            // 
            // pbRetrievingList
            // 
            this.pbRetrievingList.Location = new System.Drawing.Point(183, 376);
            this.pbRetrievingList.Name = "pbRetrievingList";
            this.pbRetrievingList.Size = new System.Drawing.Size(100, 23);
            this.pbRetrievingList.TabIndex = 26;
            this.pbRetrievingList.Visible = false;
            // 
            // lblFetchXML2
            // 
            this.lblFetchXML2.AutoSize = true;
            this.lblFetchXML2.Location = new System.Drawing.Point(320, 61);
            this.lblFetchXML2.Name = "lblFetchXML2";
            this.lblFetchXML2.Size = new System.Drawing.Size(69, 13);
            this.lblFetchXML2.TabIndex = 27;
            this.lblFetchXML2.Text = "Master Rules";
            this.lblFetchXML2.Visible = false;
            // 
            // lblElapsedMerge
            // 
            this.lblElapsedMerge.AutoSize = true;
            this.lblElapsedMerge.Location = new System.Drawing.Point(387, 397);
            this.lblElapsedMerge.Name = "lblElapsedMerge";
            this.lblElapsedMerge.Size = new System.Drawing.Size(144, 13);
            this.lblElapsedMerge.TabIndex = 18;
            this.lblElapsedMerge.Text = "Elapsed time to Merge Data: ";
            this.lblElapsedMerge.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 437);
            this.Controls.Add(this.lblFetchXML2);
            this.Controls.Add(this.pbRetrievingList);
            this.Controls.Add(this.lblBatchValue);
            this.Controls.Add(this.lblBatchTitle);
            this.Controls.Add(this.pbProcessing);
            this.Controls.Add(this.lblMergeProgress);
            this.Controls.Add(this.pbMerge);
            this.Controls.Add(this.lblETMerge);
            this.Controls.Add(this.lblETRulesValue);
            this.Controls.Add(this.lblElapsedMerge);
            this.Controls.Add(this.lblElapsedRules);
            this.Controls.Add(this.btMerge);
            this.Controls.Add(this.lblNumberOfRecords);
            this.Controls.Add(this.lblProcessingValue);
            this.Controls.Add(this.lblProcessing);
            this.Controls.Add(this.lblStep3);
            this.Controls.Add(this.lblRetrievingListValue);
            this.Controls.Add(this.lblRetrieving);
            this.Controls.Add(this.flowPanelRules);
            this.Controls.Add(this.lblStep2);
            this.Controls.Add(this.lblStep1);
            this.Controls.Add(this.lblFetchXML);
            this.Controls.Add(this.txtFetchXMLMaster);
            this.Controls.Add(this.lblRecordsFound);
            this.Controls.Add(this.btApplyRules);
            this.Controls.Add(this.cbEntity);
            this.Name = "MainForm";
            this.Text = "Auto Merge Duplicates Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbEntity;
        private System.Windows.Forms.Button btApplyRules;
        private System.Windows.Forms.Label lblRecordsFound;
        private System.Windows.Forms.TextBox txtFetchXMLMaster;
        private System.Windows.Forms.Label lblFetchXML;
        private System.Windows.Forms.Label lblStep1;
        private System.Windows.Forms.Label lblStep2;
        private System.Windows.Forms.FlowLayoutPanel flowPanelRules;
        private System.Windows.Forms.Label lblRetrieving;
        private System.Windows.Forms.Label lblRetrievingListValue;
        private System.Windows.Forms.Label lblStep3;
        private System.Windows.Forms.Label lblProcessing;
        private System.Windows.Forms.Label lblProcessingValue;
        private System.Windows.Forms.Label lblNumberOfRecords;
        private System.Windows.Forms.Button btMerge;
        private System.Windows.Forms.Label lblElapsedRules;
        private System.Windows.Forms.Label lblETRulesValue;
        private System.Windows.Forms.Label lblETMerge;
        private System.Windows.Forms.ProgressBar pbMerge;
        private System.Windows.Forms.Label lblMergeProgress;
        private System.Windows.Forms.ProgressBar pbProcessing;
        private System.Windows.Forms.Label lblBatchTitle;
        private System.Windows.Forms.Label lblBatchValue;
        private System.Windows.Forms.ProgressBar pbRetrievingList;
        private System.Windows.Forms.Label lblFetchXML2;
        private System.Windows.Forms.Label lblElapsedMerge;
    }
}