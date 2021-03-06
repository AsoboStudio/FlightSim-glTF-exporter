namespace Max2Babylon
{
    partial class ExporterForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExporterForm));
            this.butExport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtModelPath = new System.Windows.Forms.RichTextBox();
            this.butModelBrowse = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.treeView = new System.Windows.Forms.TreeView();
            this.butCancel = new System.Windows.Forms.Button();
            this.chkManifest = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkWriteTextures = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkASBAnimationRetargeting = new System.Windows.Forms.CheckBox();
            this.grpGeometry = new System.Windows.Forms.GroupBox();
            this.chkKeepInstances = new System.Windows.Forms.CheckBox();
            this.chkOverwriteTextures = new System.Windows.Forms.CheckBox();
            this.chkMergeAOwithMR = new System.Windows.Forms.CheckBox();
            this.chkExportMaterials = new System.Windows.Forms.CheckBox();
            this.chkDracoCompression = new System.Windows.Forms.CheckBox();
            this.chkExportTangents = new System.Windows.Forms.CheckBox();
            this.grpAnimations = new System.Windows.Forms.GroupBox();
            this.cmbExportAnimationType = new System.Windows.Forms.ComboBox();
            this.chkAnimgroupExportNonAnimated = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkExportMorphTangents = new System.Windows.Forms.CheckBox();
            this.chkExportMorphNormals = new System.Windows.Forms.CheckBox();
            this.lblBakeAnimation = new System.Windows.Forms.Label();
            this.cmbBakeAnimationOptions = new System.Windows.Forms.ComboBox();
            this.chkApplyPreprocessToScene = new System.Windows.Forms.CheckBox();
            this.chkMrgContainersAndXref = new System.Windows.Forms.CheckBox();
            this.chkUsePreExportProces = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtQuality = new System.Windows.Forms.TextBox();
            this.txtEnvironmentName = new System.Windows.Forms.RichTextBox();
            this.txtScaleFactor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelQuality = new System.Windows.Forms.Label();
            this.chkFullPBR = new System.Windows.Forms.CheckBox();
            this.btnEnvBrowse = new System.Windows.Forms.Button();
            this.chkNoAutoLight = new System.Windows.Forms.CheckBox();
            this.textureLabel = new System.Windows.Forms.Label();
            this.txtTexturesPath = new System.Windows.Forms.RichTextBox();
            this.btnTxtBrowse = new System.Windows.Forms.Button();
            this.chkKHRMaterialsUnlit = new System.Windows.Forms.CheckBox();
            this.chkKHRTextureTransform = new System.Windows.Forms.CheckBox();
            this.chkKHRLightsPunctual = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboOutputFormat = new System.Windows.Forms.ComboBox();
            this.chkOnlySelected = new System.Windows.Forms.CheckBox();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            this.chkHidden = new System.Windows.Forms.CheckBox();
            this.butExportAndRun = new System.Windows.Forms.Button();
            this.butClose = new System.Windows.Forms.Button();
            this.toolTipDracoCompression = new System.Windows.Forms.ToolTip(this.components);
            this.butMultiExport = new System.Windows.Forms.Button();
            this.envFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.grp_FlightSim = new System.Windows.Forms.GroupBox();
            this.txtDstTextureExt = new System.Windows.Forms.TextBox();
            this.lblCE_to = new System.Windows.Forms.Label();
            this.lblCE_from = new System.Windows.Forms.Label();
            this.txtSrcTextureExt = new System.Windows.Forms.TextBox();
            this.lblConvertExtension = new System.Windows.Forms.Label();
            this.normalMapConventionLbl = new System.Windows.Forms.Label();
            this.cmbNormalMapConvention = new System.Windows.Forms.ComboBox();
            this.chkRemoveNamespace = new System.Windows.Forms.CheckBox();
            this.chk_RemoveLodPrefix = new System.Windows.Forms.CheckBox();
            this.pictureBox_flightsim = new System.Windows.Forms.PictureBox();
            this.logLevelcmb = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.grpGeometry.SuspendLayout();
            this.grpAnimations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.grp_FlightSim.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_flightsim)).BeginInit();
            this.SuspendLayout();
            // 
            // butExport
            // 
            this.butExport.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butExport.Enabled = false;
            this.butExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butExport.Location = new System.Drawing.Point(421, 495);
            this.butExport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butExport.Name = "butExport";
            this.butExport.Size = new System.Drawing.Size(197, 27);
            this.butExport.TabIndex = 100;
            this.butExport.Text = "Export";
            this.butExport.UseVisualStyleBackColor = true;
            this.butExport.Click += new System.EventHandler(this.butExport_Click);
            this.butExport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 78);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Model path:";
            // 
            // txtModelPath
            // 
            this.txtModelPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtModelPath.Location = new System.Drawing.Point(91, 75);
            this.txtModelPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtModelPath.Multiline = false;
            this.txtModelPath.Name = "txtModelPath";
            this.txtModelPath.Size = new System.Drawing.Size(708, 20);
            this.txtModelPath.TabIndex = 2;
            this.txtModelPath.Text = "";
            this.txtModelPath.TextChanged += new System.EventHandler(this.txtFilename_TextChanged);
            this.txtModelPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butModelBrowse
            // 
            this.butModelBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butModelBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butModelBrowse.Location = new System.Drawing.Point(805, 73);
            this.butModelBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butModelBrowse.Name = "butModelBrowse";
            this.butModelBrowse.Size = new System.Drawing.Size(28, 23);
            this.butModelBrowse.TabIndex = 3;
            this.butModelBrowse.Text = "...";
            this.butModelBrowse.UseVisualStyleBackColor = true;
            this.butModelBrowse.Click += new System.EventHandler(this.butModelBrowse_Click);
            this.butModelBrowse.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "babylon";
            this.saveFileDialog.Filter = "Babylon files|*.babylon";
            this.saveFileDialog.RestoreDirectory = true;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 861);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1080, 23);
            this.progressBar.TabIndex = 104;
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.Location = new System.Drawing.Point(12, 532);
            this.treeView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(1252, 319);
            this.treeView.TabIndex = 103;
            this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Enabled = false;
            this.butCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butCancel.Location = new System.Drawing.Point(1098, 861);
            this.butCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 105;
            this.butCancel.Text = "Cancel";
            this.butCancel.UseVisualStyleBackColor = true;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            this.butCancel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkManifest
            // 
            this.chkManifest.AutoSize = true;
            this.chkManifest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkManifest.Location = new System.Drawing.Point(16, 239);
            this.chkManifest.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkManifest.Name = "chkManifest";
            this.chkManifest.Size = new System.Drawing.Size(112, 17);
            this.chkManifest.TabIndex = 14;
            this.chkManifest.Text = "Generate .manifest";
            this.chkManifest.UseVisualStyleBackColor = true;
            this.chkManifest.CheckedChanged += new System.EventHandler(this.chkManifest_CheckedChanged);
            this.chkManifest.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 151);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Options:";
            // 
            // chkWriteTextures
            // 
            this.chkWriteTextures.AutoSize = true;
            this.chkWriteTextures.Checked = true;
            this.chkWriteTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWriteTextures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkWriteTextures.Location = new System.Drawing.Point(7, 27);
            this.chkWriteTextures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkWriteTextures.Name = "chkWriteTextures";
            this.chkWriteTextures.Size = new System.Drawing.Size(92, 17);
            this.chkWriteTextures.TabIndex = 11;
            this.chkWriteTextures.Text = "Write Textures";
            this.chkWriteTextures.UseVisualStyleBackColor = true;
            this.chkWriteTextures.CheckedChanged += new System.EventHandler(this.chkWriteTextures_CheckedChanged);
            this.chkWriteTextures.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkASBAnimationRetargeting);
            this.groupBox1.Controls.Add(this.grpGeometry);
            this.groupBox1.Controls.Add(this.grpAnimations);
            this.groupBox1.Controls.Add(this.lblBakeAnimation);
            this.groupBox1.Controls.Add(this.cmbBakeAnimationOptions);
            this.groupBox1.Controls.Add(this.chkApplyPreprocessToScene);
            this.groupBox1.Controls.Add(this.chkMrgContainersAndXref);
            this.groupBox1.Controls.Add(this.chkUsePreExportProces);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtQuality);
            this.groupBox1.Controls.Add(this.txtEnvironmentName);
            this.groupBox1.Controls.Add(this.txtScaleFactor);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.labelQuality);
            this.groupBox1.Controls.Add(this.chkFullPBR);
            this.groupBox1.Controls.Add(this.btnEnvBrowse);
            this.groupBox1.Controls.Add(this.chkNoAutoLight);
            this.groupBox1.Controls.Add(this.textureLabel);
            this.groupBox1.Controls.Add(this.txtTexturesPath);
            this.groupBox1.Controls.Add(this.btnTxtBrowse);
            this.groupBox1.Controls.Add(this.chkKHRMaterialsUnlit);
            this.groupBox1.Controls.Add(this.chkKHRTextureTransform);
            this.groupBox1.Controls.Add(this.chkKHRLightsPunctual);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboOutputFormat);
            this.groupBox1.Controls.Add(this.chkOnlySelected);
            this.groupBox1.Controls.Add(this.chkAutoSave);
            this.groupBox1.Controls.Add(this.chkHidden);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtModelPath);
            this.groupBox1.Controls.Add(this.chkManifest);
            this.groupBox1.Controls.Add(this.butModelBrowse);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 6);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(892, 479);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // chkASBAnimationRetargeting
            // 
            this.chkASBAnimationRetargeting.AutoSize = true;
            this.chkASBAnimationRetargeting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkASBAnimationRetargeting.Location = new System.Drawing.Point(461, 455);
            this.chkASBAnimationRetargeting.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkASBAnimationRetargeting.Name = "chkASBAnimationRetargeting";
            this.chkASBAnimationRetargeting.Size = new System.Drawing.Size(167, 17);
            this.chkASBAnimationRetargeting.TabIndex = 42;
            this.chkASBAnimationRetargeting.Text = "ASOBO_animation_retargeting";
            this.chkASBAnimationRetargeting.UseVisualStyleBackColor = true;
            this.chkASBAnimationRetargeting.CheckedChanged += new System.EventHandler(this.chkASBAnimationRetargeting_CheckedChanged);
            // 
            // grpGeometry
            // 
            this.grpGeometry.Controls.Add(this.chkKeepInstances);
            this.grpGeometry.Controls.Add(this.chkWriteTextures);
            this.grpGeometry.Controls.Add(this.chkOverwriteTextures);
            this.grpGeometry.Controls.Add(this.chkMergeAOwithMR);
            this.grpGeometry.Controls.Add(this.chkExportMaterials);
            this.grpGeometry.Controls.Add(this.chkDracoCompression);
            this.grpGeometry.Controls.Add(this.chkExportTangents);
            this.grpGeometry.Location = new System.Drawing.Point(185, 173);
            this.grpGeometry.Name = "grpGeometry";
            this.grpGeometry.Size = new System.Drawing.Size(290, 134);
            this.grpGeometry.TabIndex = 41;
            this.grpGeometry.TabStop = false;
            this.grpGeometry.Text = "Geometry";
            // 
            // chkKeepInstances
            // 
            this.chkKeepInstances.AutoSize = true;
            this.chkKeepInstances.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkKeepInstances.Location = new System.Drawing.Point(7, 108);
            this.chkKeepInstances.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkKeepInstances.Name = "chkKeepInstances";
            this.chkKeepInstances.Size = new System.Drawing.Size(97, 17);
            this.chkKeepInstances.TabIndex = 36;
            this.chkKeepInstances.Text = "Keep Instances";
            this.chkKeepInstances.UseVisualStyleBackColor = true;
            this.chkKeepInstances.CheckedChanged += new System.EventHandler(this.chkKeepInstances_CheckedChanged);
            // 
            // chkOverwriteTextures
            // 
            this.chkOverwriteTextures.AutoSize = true;
            this.chkOverwriteTextures.Checked = true;
            this.chkOverwriteTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOverwriteTextures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkOverwriteTextures.Location = new System.Drawing.Point(7, 54);
            this.chkOverwriteTextures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOverwriteTextures.Name = "chkOverwriteTextures";
            this.chkOverwriteTextures.Size = new System.Drawing.Size(112, 17);
            this.chkOverwriteTextures.TabIndex = 19;
            this.chkOverwriteTextures.Text = "Overwrite Textures";
            this.chkOverwriteTextures.UseVisualStyleBackColor = true;
            this.chkOverwriteTextures.CheckedChanged += new System.EventHandler(this.chkOverwriteTextures_CheckedChanged);
            // 
            // chkMergeAOwithMR
            // 
            this.chkMergeAOwithMR.AutoSize = true;
            this.chkMergeAOwithMR.Checked = true;
            this.chkMergeAOwithMR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMergeAOwithMR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkMergeAOwithMR.Location = new System.Drawing.Point(140, 27);
            this.chkMergeAOwithMR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkMergeAOwithMR.Name = "chkMergeAOwithMR";
            this.chkMergeAOwithMR.Size = new System.Drawing.Size(94, 17);
            this.chkMergeAOwithMR.TabIndex = 17;
            this.chkMergeAOwithMR.Text = "Merge AO map";
            this.chkMergeAOwithMR.UseVisualStyleBackColor = true;
            this.chkMergeAOwithMR.CheckedChanged += new System.EventHandler(this.chkMergeAOwithMR_CheckedChanged);
            this.chkMergeAOwithMR.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkExportMaterials
            // 
            this.chkExportMaterials.AutoSize = true;
            this.chkExportMaterials.Checked = true;
            this.chkExportMaterials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportMaterials.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMaterials.Location = new System.Drawing.Point(7, 81);
            this.chkExportMaterials.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkExportMaterials.Name = "chkExportMaterials";
            this.chkExportMaterials.Size = new System.Drawing.Size(98, 17);
            this.chkExportMaterials.TabIndex = 23;
            this.chkExportMaterials.Text = "Export Materials";
            this.chkExportMaterials.UseVisualStyleBackColor = true;
            this.chkExportMaterials.CheckedChanged += new System.EventHandler(this.chkExportMaterials_CheckedChanged);
            // 
            // chkDracoCompression
            // 
            this.chkDracoCompression.AutoSize = true;
            this.chkDracoCompression.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDracoCompression.Location = new System.Drawing.Point(140, 54);
            this.chkDracoCompression.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkDracoCompression.Name = "chkDracoCompression";
            this.chkDracoCompression.Size = new System.Drawing.Size(136, 17);
            this.chkDracoCompression.TabIndex = 18;
            this.chkDracoCompression.Text = "Use Draco compression";
            this.chkDracoCompression.UseVisualStyleBackColor = true;
            this.chkDracoCompression.CheckedChanged += new System.EventHandler(this.chkDracoCompression_CheckedChanged);
            this.chkDracoCompression.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkExportTangents
            // 
            this.chkExportTangents.AutoSize = true;
            this.chkExportTangents.Checked = true;
            this.chkExportTangents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportTangents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportTangents.Location = new System.Drawing.Point(140, 81);
            this.chkExportTangents.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkExportTangents.Name = "chkExportTangents";
            this.chkExportTangents.Size = new System.Drawing.Size(97, 17);
            this.chkExportTangents.TabIndex = 16;
            this.chkExportTangents.Text = "Export tangents";
            this.chkExportTangents.UseVisualStyleBackColor = true;
            this.chkExportTangents.CheckedChanged += new System.EventHandler(this.chkExportTangents_CheckedChanged);
            this.chkExportTangents.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // grpAnimations
            // 
            this.grpAnimations.Controls.Add(this.cmbExportAnimationType);
            this.grpAnimations.Controls.Add(this.chkAnimgroupExportNonAnimated);
            this.grpAnimations.Controls.Add(this.label8);
            this.grpAnimations.Controls.Add(this.chkExportMorphTangents);
            this.grpAnimations.Controls.Add(this.chkExportMorphNormals);
            this.grpAnimations.Location = new System.Drawing.Point(499, 173);
            this.grpAnimations.Name = "grpAnimations";
            this.grpAnimations.Size = new System.Drawing.Size(300, 134);
            this.grpAnimations.TabIndex = 40;
            this.grpAnimations.TabStop = false;
            this.grpAnimations.Text = "Animations";
            // 
            // cmbExportAnimationType
            // 
            this.cmbExportAnimationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExportAnimationType.FormattingEnabled = true;
            this.cmbExportAnimationType.Items.AddRange(new object[] {
            "Export",
            "Not Export",
            "Export ONLY"});
            this.cmbExportAnimationType.Location = new System.Drawing.Point(9, 16);
            this.cmbExportAnimationType.Name = "cmbExportAnimationType";
            this.cmbExportAnimationType.Size = new System.Drawing.Size(121, 21);
            this.cmbExportAnimationType.TabIndex = 34;
            this.cmbExportAnimationType.SelectedIndexChanged += new System.EventHandler(this.cmbExportAnimationType_SelectedIndexChanged);
            // 
            // chkAnimgroupExportNonAnimated
            // 
            this.chkAnimgroupExportNonAnimated.AutoSize = true;
            this.chkAnimgroupExportNonAnimated.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAnimgroupExportNonAnimated.Location = new System.Drawing.Point(12, 69);
            this.chkAnimgroupExportNonAnimated.Name = "chkAnimgroupExportNonAnimated";
            this.chkAnimgroupExportNonAnimated.Size = new System.Drawing.Size(249, 17);
            this.chkAnimgroupExportNonAnimated.TabIndex = 18;
            this.chkAnimgroupExportNonAnimated.Text = "(Animation Group) Export Non-Animated Objects";
            this.chkAnimgroupExportNonAnimated.UseVisualStyleBackColor = true;
            this.chkAnimgroupExportNonAnimated.CheckedChanged += new System.EventHandler(this.chkAnimgroupExportNonAnimated_CheckedChanged);
            this.chkAnimgroupExportNonAnimated.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 89);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(113, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Morph Target Options:";
            // 
            // chkExportMorphTangents
            // 
            this.chkExportMorphTangents.AutoSize = true;
            this.chkExportMorphTangents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMorphTangents.Location = new System.Drawing.Point(18, 105);
            this.chkExportMorphTangents.Name = "chkExportMorphTangents";
            this.chkExportMorphTangents.Size = new System.Drawing.Size(129, 17);
            this.chkExportMorphTangents.TabIndex = 16;
            this.chkExportMorphTangents.Text = "Export morph tangents";
            this.chkExportMorphTangents.UseVisualStyleBackColor = true;
            this.chkExportMorphTangents.CheckedChanged += new System.EventHandler(this.chkExportMorphTangents_CheckedChanged);
            this.chkExportMorphTangents.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkExportMorphNormals
            // 
            this.chkExportMorphNormals.AutoSize = true;
            this.chkExportMorphNormals.Checked = true;
            this.chkExportMorphNormals.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportMorphNormals.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkExportMorphNormals.Location = new System.Drawing.Point(166, 105);
            this.chkExportMorphNormals.Name = "chkExportMorphNormals";
            this.chkExportMorphNormals.Size = new System.Drawing.Size(124, 17);
            this.chkExportMorphNormals.TabIndex = 16;
            this.chkExportMorphNormals.Text = "Export morph normals";
            this.chkExportMorphNormals.UseVisualStyleBackColor = true;
            this.chkExportMorphNormals.CheckedChanged += new System.EventHandler(this.chkExportMorphNormals_CheckedChanged);
            this.chkExportMorphNormals.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // lblBakeAnimation
            // 
            this.lblBakeAnimation.AutoSize = true;
            this.lblBakeAnimation.Enabled = false;
            this.lblBakeAnimation.Location = new System.Drawing.Point(189, 329);
            this.lblBakeAnimation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBakeAnimation.Name = "lblBakeAnimation";
            this.lblBakeAnimation.Size = new System.Drawing.Size(125, 13);
            this.lblBakeAnimation.TabIndex = 40;
            this.lblBakeAnimation.Text = "Bake animations options:";
            // 
            // cmbBakeAnimationOptions
            // 
            this.cmbBakeAnimationOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBakeAnimationOptions.Enabled = false;
            this.cmbBakeAnimationOptions.Items.AddRange(new object[] {
            "Do not bake animations",
            "Bake all animations",
            "Selective bake"});
            this.cmbBakeAnimationOptions.Location = new System.Drawing.Point(322, 326);
            this.cmbBakeAnimationOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbBakeAnimationOptions.Name = "cmbBakeAnimationOptions";
            this.cmbBakeAnimationOptions.Size = new System.Drawing.Size(178, 21);
            this.cmbBakeAnimationOptions.TabIndex = 41;
            this.cmbBakeAnimationOptions.SelectedIndexChanged += new System.EventHandler(this.cmbBakeAnimationOptions_SelectedIndexChanged);
            // 
            // chkApplyPreprocessToScene
            // 
            this.chkApplyPreprocessToScene.AutoSize = true;
            this.chkApplyPreprocessToScene.Enabled = false;
            this.chkApplyPreprocessToScene.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkApplyPreprocessToScene.Location = new System.Drawing.Point(23, 347);
            this.chkApplyPreprocessToScene.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkApplyPreprocessToScene.Name = "chkApplyPreprocessToScene";
            this.chkApplyPreprocessToScene.Size = new System.Drawing.Size(155, 17);
            this.chkApplyPreprocessToScene.TabIndex = 39;
            this.chkApplyPreprocessToScene.Text = "Apply Preprocess To Scene";
            this.chkApplyPreprocessToScene.UseVisualStyleBackColor = true;
            this.chkApplyPreprocessToScene.CheckedChanged += new System.EventHandler(this.chkApplyPreprocessToScene_CheckedChanged);
            // 
            // chkMrgContainersAndXref
            // 
            this.chkMrgContainersAndXref.AutoSize = true;
            this.chkMrgContainersAndXref.Enabled = false;
            this.chkMrgContainersAndXref.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkMrgContainersAndXref.Location = new System.Drawing.Point(23, 327);
            this.chkMrgContainersAndXref.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkMrgContainersAndXref.Name = "chkMrgContainersAndXref";
            this.chkMrgContainersAndXref.Size = new System.Drawing.Size(155, 17);
            this.chkMrgContainersAndXref.TabIndex = 37;
            this.chkMrgContainersAndXref.Text = "Merge Containers And XRef";
            this.chkMrgContainersAndXref.UseVisualStyleBackColor = true;
            this.chkMrgContainersAndXref.CheckedChanged += new System.EventHandler(this.chkMrgContainersAndXref_CheckedChanged);
            // 
            // chkUsePreExportProces
            // 
            this.chkUsePreExportProces.AutoSize = true;
            this.chkUsePreExportProces.Location = new System.Drawing.Point(14, 310);
            this.chkUsePreExportProces.Name = "chkUsePreExportProces";
            this.chkUsePreExportProces.Size = new System.Drawing.Size(138, 17);
            this.chkUsePreExportProces.TabIndex = 36;
            this.chkUsePreExportProces.Text = "Use PreExport Process:";
            this.chkUsePreExportProces.UseVisualStyleBackColor = true;
            this.chkUsePreExportProces.CheckedChanged += new System.EventHandler(this.chkUsePreExportProces_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 420);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "Environment";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 383);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 13);
            this.label7.TabIndex = 33;
            this.label7.Text = "Babylon PBR Options:";
            // 
            // txtQuality
            // 
            this.txtQuality.Location = new System.Drawing.Point(794, 40);
            this.txtQuality.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtQuality.Name = "txtQuality";
            this.txtQuality.Size = new System.Drawing.Size(43, 20);
            this.txtQuality.TabIndex = 9;
            this.txtQuality.Text = "100";
            this.txtQuality.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtQuality.TextChanged += new System.EventHandler(this.txtQuality_TextChanged);
            this.txtQuality.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // txtEnvironmentName
            // 
            this.txtEnvironmentName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEnvironmentName.Location = new System.Drawing.Point(91, 418);
            this.txtEnvironmentName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtEnvironmentName.Multiline = false;
            this.txtEnvironmentName.Name = "txtEnvironmentName";
            this.txtEnvironmentName.Size = new System.Drawing.Size(708, 20);
            this.txtEnvironmentName.TabIndex = 30;
            this.txtEnvironmentName.Text = "";
            this.txtEnvironmentName.TextChanged += new System.EventHandler(this.txtEnvironmentName_TextChanged);
            // 
            // txtScaleFactor
            // 
            this.txtScaleFactor.Location = new System.Drawing.Point(794, 16);
            this.txtScaleFactor.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtScaleFactor.Name = "txtScaleFactor";
            this.txtScaleFactor.Size = new System.Drawing.Size(42, 20);
            this.txtScaleFactor.TabIndex = 7;
            this.txtScaleFactor.Text = "1";
            this.txtScaleFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtScaleFactor.TextChanged += new System.EventHandler(this.txtScaleFactor_TextChanged);
            this.txtScaleFactor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 438);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 29;
            this.label6.Text = "GLTF Options:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(722, 18);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Scale factor:";
            // 
            // labelQuality
            // 
            this.labelQuality.AutoSize = true;
            this.labelQuality.Location = new System.Drawing.Point(710, 42);
            this.labelQuality.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelQuality.Name = "labelQuality";
            this.labelQuality.Size = new System.Drawing.Size(79, 13);
            this.labelQuality.TabIndex = 8;
            this.labelQuality.Text = "Texture quality:";
            // 
            // chkFullPBR
            // 
            this.chkFullPBR.AutoSize = true;
            this.chkFullPBR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkFullPBR.Location = new System.Drawing.Point(177, 399);
            this.chkFullPBR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkFullPBR.Name = "chkFullPBR";
            this.chkFullPBR.Size = new System.Drawing.Size(86, 17);
            this.chkFullPBR.TabIndex = 28;
            this.chkFullPBR.Text = "Use Full PBR";
            this.chkFullPBR.UseVisualStyleBackColor = true;
            this.chkFullPBR.CheckedChanged += new System.EventHandler(this.chkFullPBR_CheckedChanged);
            // 
            // btnEnvBrowse
            // 
            this.btnEnvBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnvBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnvBrowse.Location = new System.Drawing.Point(805, 416);
            this.btnEnvBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnEnvBrowse.Name = "btnEnvBrowse";
            this.btnEnvBrowse.Size = new System.Drawing.Size(28, 23);
            this.btnEnvBrowse.TabIndex = 31;
            this.btnEnvBrowse.Text = "...";
            this.btnEnvBrowse.UseVisualStyleBackColor = true;
            this.btnEnvBrowse.Click += new System.EventHandler(this.btnEnvBrowse_Click);
            // 
            // chkNoAutoLight
            // 
            this.chkNoAutoLight.AutoSize = true;
            this.chkNoAutoLight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkNoAutoLight.Location = new System.Drawing.Point(23, 399);
            this.chkNoAutoLight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkNoAutoLight.Name = "chkNoAutoLight";
            this.chkNoAutoLight.Size = new System.Drawing.Size(113, 17);
            this.chkNoAutoLight.TabIndex = 27;
            this.chkNoAutoLight.Text = "No Automatic Light";
            this.chkNoAutoLight.UseVisualStyleBackColor = true;
            this.chkNoAutoLight.CheckedChanged += new System.EventHandler(this.chkNoAutoLight_CheckedChanged);
            // 
            // textureLabel
            // 
            this.textureLabel.AutoSize = true;
            this.textureLabel.Location = new System.Drawing.Point(11, 110);
            this.textureLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.textureLabel.Name = "textureLabel";
            this.textureLabel.Size = new System.Drawing.Size(76, 13);
            this.textureLabel.TabIndex = 24;
            this.textureLabel.Text = "Textures Path:";
            // 
            // txtTexturesPath
            // 
            this.txtTexturesPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTexturesPath.Location = new System.Drawing.Point(91, 107);
            this.txtTexturesPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTexturesPath.Multiline = false;
            this.txtTexturesPath.Name = "txtTexturesPath";
            this.txtTexturesPath.Size = new System.Drawing.Size(708, 20);
            this.txtTexturesPath.TabIndex = 25;
            this.txtTexturesPath.Text = "";
            this.txtTexturesPath.TextChanged += new System.EventHandler(this.txtTexturesPath_TextChanged);
            // 
            // btnTxtBrowse
            // 
            this.btnTxtBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTxtBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTxtBrowse.Location = new System.Drawing.Point(805, 105);
            this.btnTxtBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTxtBrowse.Name = "btnTxtBrowse";
            this.btnTxtBrowse.Size = new System.Drawing.Size(28, 23);
            this.btnTxtBrowse.TabIndex = 26;
            this.btnTxtBrowse.Text = "...";
            this.btnTxtBrowse.UseVisualStyleBackColor = true;
            this.btnTxtBrowse.Click += new System.EventHandler(this.btnTextureBrowse_Click);
            // 
            // chkKHRMaterialsUnlit
            // 
            this.chkKHRMaterialsUnlit.AutoSize = true;
            this.chkKHRMaterialsUnlit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkKHRMaterialsUnlit.Location = new System.Drawing.Point(323, 455);
            this.chkKHRMaterialsUnlit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkKHRMaterialsUnlit.Name = "chkKHRMaterialsUnlit";
            this.chkKHRMaterialsUnlit.Size = new System.Drawing.Size(118, 17);
            this.chkKHRMaterialsUnlit.TabIndex = 22;
            this.chkKHRMaterialsUnlit.Text = "KHR_materials_unlit";
            this.chkKHRMaterialsUnlit.UseVisualStyleBackColor = true;
            this.chkKHRMaterialsUnlit.CheckedChanged += new System.EventHandler(this.chkKHRMaterialsUnlit_CheckedChanged);
            // 
            // chkKHRTextureTransform
            // 
            this.chkKHRTextureTransform.AutoSize = true;
            this.chkKHRTextureTransform.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkKHRTextureTransform.Location = new System.Drawing.Point(172, 455);
            this.chkKHRTextureTransform.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkKHRTextureTransform.Name = "chkKHRTextureTransform";
            this.chkKHRTextureTransform.Size = new System.Drawing.Size(133, 17);
            this.chkKHRTextureTransform.TabIndex = 21;
            this.chkKHRTextureTransform.Text = "KHR_texture_transform";
            this.chkKHRTextureTransform.UseVisualStyleBackColor = true;
            this.chkKHRTextureTransform.CheckedChanged += new System.EventHandler(this.chkKHRTextureTransform_CheckedChanged);
            // 
            // chkKHRLightsPunctual
            // 
            this.chkKHRLightsPunctual.AutoSize = true;
            this.chkKHRLightsPunctual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkKHRLightsPunctual.Location = new System.Drawing.Point(24, 455);
            this.chkKHRLightsPunctual.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkKHRLightsPunctual.Name = "chkKHRLightsPunctual";
            this.chkKHRLightsPunctual.Size = new System.Drawing.Size(123, 17);
            this.chkKHRLightsPunctual.TabIndex = 20;
            this.chkKHRLightsPunctual.Text = "KHR_lights_punctual";
            this.chkKHRLightsPunctual.UseVisualStyleBackColor = true;
            this.chkKHRLightsPunctual.CheckedChanged += new System.EventHandler(this.chkKHRLightsPunctual_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Output format:";
            // 
            // comboOutputFormat
            // 
            this.comboOutputFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOutputFormat.Items.AddRange(new object[] {
            "babylon",
            "binary babylon",
            "gltf",
            "glb"});
            this.comboOutputFormat.Location = new System.Drawing.Point(91, 16);
            this.comboOutputFormat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboOutputFormat.Name = "comboOutputFormat";
            this.comboOutputFormat.Size = new System.Drawing.Size(121, 21);
            this.comboOutputFormat.TabIndex = 5;
            this.comboOutputFormat.SelectedIndexChanged += new System.EventHandler(this.comboOutputFormat_SelectedIndexChanged);
            this.comboOutputFormat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkOnlySelected
            // 
            this.chkOnlySelected.AutoSize = true;
            this.chkOnlySelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkOnlySelected.Location = new System.Drawing.Point(16, 216);
            this.chkOnlySelected.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOnlySelected.Name = "chkOnlySelected";
            this.chkOnlySelected.Size = new System.Drawing.Size(118, 17);
            this.chkOnlySelected.TabIndex = 13;
            this.chkOnlySelected.Text = "Export only selected";
            this.chkOnlySelected.UseVisualStyleBackColor = true;
            this.chkOnlySelected.CheckedChanged += new System.EventHandler(this.chkOnlySelected_CheckedChanged);
            this.chkOnlySelected.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkAutoSave
            // 
            this.chkAutoSave.AutoSize = true;
            this.chkAutoSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAutoSave.Location = new System.Drawing.Point(16, 170);
            this.chkAutoSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkAutoSave.Name = "chkAutoSave";
            this.chkAutoSave.Size = new System.Drawing.Size(130, 17);
            this.chkAutoSave.TabIndex = 15;
            this.chkAutoSave.Text = "Auto save 3ds Max file";
            this.chkAutoSave.UseVisualStyleBackColor = true;
            this.chkAutoSave.CheckedChanged += new System.EventHandler(this.chkAutoSave_CheckedChanged);
            this.chkAutoSave.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // chkHidden
            // 
            this.chkHidden.AutoSize = true;
            this.chkHidden.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkHidden.Location = new System.Drawing.Point(16, 193);
            this.chkHidden.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkHidden.Name = "chkHidden";
            this.chkHidden.Size = new System.Drawing.Size(125, 17);
            this.chkHidden.TabIndex = 12;
            this.chkHidden.Text = "Export hidden objects";
            this.chkHidden.UseVisualStyleBackColor = true;
            this.chkHidden.CheckedChanged += new System.EventHandler(this.chkHidden_CheckedChanged);
            this.chkHidden.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butExportAndRun
            // 
            this.butExportAndRun.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butExportAndRun.Enabled = false;
            this.butExportAndRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butExportAndRun.Location = new System.Drawing.Point(624, 495);
            this.butExportAndRun.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butExportAndRun.Name = "butExportAndRun";
            this.butExportAndRun.Size = new System.Drawing.Size(197, 27);
            this.butExportAndRun.TabIndex = 102;
            this.butExportAndRun.Text = "Export && Run";
            this.butExportAndRun.UseVisualStyleBackColor = true;
            this.butExportAndRun.Click += new System.EventHandler(this.butExportAndRun_Click);
            this.butExportAndRun.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // butClose
            // 
            this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butClose.Location = new System.Drawing.Point(1184, 861);
            this.butClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(80, 23);
            this.butClose.TabIndex = 106;
            this.butClose.Text = "Close";
            this.butClose.UseVisualStyleBackColor = true;
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            this.butClose.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            // 
            // toolTipDracoCompression
            // 
            this.toolTipDracoCompression.ShowAlways = true;
            // 
            // butMultiExport
            // 
            this.butMultiExport.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butMultiExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butMultiExport.Location = new System.Drawing.Point(827, 495);
            this.butMultiExport.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.butMultiExport.Name = "butMultiExport";
            this.butMultiExport.Size = new System.Drawing.Size(199, 27);
            this.butMultiExport.TabIndex = 109;
            this.butMultiExport.Text = "Multi-File Export | Shift-click to edit";
            this.butMultiExport.UseVisualStyleBackColor = true;
            this.butMultiExport.Click += new System.EventHandler(this.butMultiExport_Click);
            // 
            // envFileDialog
            // 
            this.envFileDialog.DefaultExt = "dds";
            this.envFileDialog.Filter = "dds files|*.dds";
            this.envFileDialog.Title = "Select Environment";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox2.InitialImage")));
            this.pictureBox2.Location = new System.Drawing.Point(916, 11);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(348, 159);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 9;
            this.pictureBox2.TabStop = false;
            // 
            // grp_FlightSim
            // 
            this.grp_FlightSim.Controls.Add(this.txtDstTextureExt);
            this.grp_FlightSim.Controls.Add(this.lblCE_to);
            this.grp_FlightSim.Controls.Add(this.lblCE_from);
            this.grp_FlightSim.Controls.Add(this.txtSrcTextureExt);
            this.grp_FlightSim.Controls.Add(this.lblConvertExtension);
            this.grp_FlightSim.Controls.Add(this.normalMapConventionLbl);
            this.grp_FlightSim.Controls.Add(this.cmbNormalMapConvention);
            this.grp_FlightSim.Controls.Add(this.chkRemoveNamespace);
            this.grp_FlightSim.Controls.Add(this.chk_RemoveLodPrefix);
            this.grp_FlightSim.Location = new System.Drawing.Point(916, 341);
            this.grp_FlightSim.Name = "grp_FlightSim";
            this.grp_FlightSim.Size = new System.Drawing.Size(344, 144);
            this.grp_FlightSim.TabIndex = 111;
            this.grp_FlightSim.TabStop = false;
            this.grp_FlightSim.Text = "FlightSim";
            // 
            // txtDstTextureExt
            // 
            this.txtDstTextureExt.Location = new System.Drawing.Point(162, 111);
            this.txtDstTextureExt.Name = "txtDstTextureExt";
            this.txtDstTextureExt.Size = new System.Drawing.Size(100, 20);
            this.txtDstTextureExt.TabIndex = 41;
            this.txtDstTextureExt.TextChanged += new System.EventHandler(this.txtDstTextureExt_TextChanged);
            // 
            // lblCE_to
            // 
            this.lblCE_to.AutoSize = true;
            this.lblCE_to.Location = new System.Drawing.Point(136, 114);
            this.lblCE_to.Name = "lblCE_to";
            this.lblCE_to.Size = new System.Drawing.Size(20, 13);
            this.lblCE_to.TabIndex = 40;
            this.lblCE_to.Text = "To";
            // 
            // lblCE_from
            // 
            this.lblCE_from.AutoSize = true;
            this.lblCE_from.Location = new System.Drawing.Point(9, 114);
            this.lblCE_from.Name = "lblCE_from";
            this.lblCE_from.Size = new System.Drawing.Size(30, 13);
            this.lblCE_from.TabIndex = 39;
            this.lblCE_from.Text = "From";
            // 
            // txtSrcTextureExt
            // 
            this.txtSrcTextureExt.Location = new System.Drawing.Point(45, 111);
            this.txtSrcTextureExt.Name = "txtSrcTextureExt";
            this.txtSrcTextureExt.Size = new System.Drawing.Size(85, 20);
            this.txtSrcTextureExt.TabIndex = 38;
            this.txtSrcTextureExt.TextChanged += new System.EventHandler(this.txtSrcTextureExt_TextChanged);
            // 
            // lblConvertExtension
            // 
            this.lblConvertExtension.AutoSize = true;
            this.lblConvertExtension.Location = new System.Drawing.Point(9, 91);
            this.lblConvertExtension.Name = "lblConvertExtension";
            this.lblConvertExtension.Size = new System.Drawing.Size(130, 13);
            this.lblConvertExtension.TabIndex = 37;
            this.lblConvertExtension.Text = "Convert texture extension:";
            // 
            // normalMapConventionLbl
            // 
            this.normalMapConventionLbl.AutoSize = true;
            this.normalMapConventionLbl.Location = new System.Drawing.Point(9, 67);
            this.normalMapConventionLbl.Name = "normalMapConventionLbl";
            this.normalMapConventionLbl.Size = new System.Drawing.Size(121, 13);
            this.normalMapConventionLbl.TabIndex = 36;
            this.normalMapConventionLbl.Text = "Normal Map Convention";
            // 
            // cmbNormalMapConvention
            // 
            this.cmbNormalMapConvention.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNormalMapConvention.FormattingEnabled = true;
            this.cmbNormalMapConvention.Items.AddRange(new object[] {
            "DirectX",
            "OpenGL"});
            this.cmbNormalMapConvention.Location = new System.Drawing.Point(136, 64);
            this.cmbNormalMapConvention.Name = "cmbNormalMapConvention";
            this.cmbNormalMapConvention.Size = new System.Drawing.Size(99, 21);
            this.cmbNormalMapConvention.TabIndex = 35;
            this.cmbNormalMapConvention.SelectedIndexChanged += new System.EventHandler(this.cmbNormalMapConvention_SelectedIndexChanged);
            // 
            // chkRemoveNamespace
            // 
            this.chkRemoveNamespace.AutoSize = true;
            this.chkRemoveNamespace.Location = new System.Drawing.Point(11, 43);
            this.chkRemoveNamespace.Name = "chkRemoveNamespace";
            this.chkRemoveNamespace.Size = new System.Drawing.Size(126, 17);
            this.chkRemoveNamespace.TabIndex = 1;
            this.chkRemoveNamespace.Text = "Remove Namespace";
            this.chkRemoveNamespace.UseVisualStyleBackColor = true;
            this.chkRemoveNamespace.CheckedChanged += new System.EventHandler(this.chkRemoveNamespace_CheckedChanged);
            // 
            // chk_RemoveLodPrefix
            // 
            this.chk_RemoveLodPrefix.AutoSize = true;
            this.chk_RemoveLodPrefix.Location = new System.Drawing.Point(11, 19);
            this.chk_RemoveLodPrefix.Name = "chk_RemoveLodPrefix";
            this.chk_RemoveLodPrefix.Size = new System.Drawing.Size(119, 17);
            this.chk_RemoveLodPrefix.TabIndex = 0;
            this.chk_RemoveLodPrefix.Text = "Remove LOD prefix";
            this.chk_RemoveLodPrefix.UseVisualStyleBackColor = true;
            this.chk_RemoveLodPrefix.CheckedChanged += new System.EventHandler(this.chk_RemoveLodPrefix_CheckedChanged);
            // 
            // pictureBox_flightsim
            // 
            this.pictureBox_flightsim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_flightsim.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_flightsim.Image")));
            this.pictureBox_flightsim.InitialImage = global::Max2Babylon.Properties.Resources.FlightSimExporter;
            this.pictureBox_flightsim.Location = new System.Drawing.Point(915, 176);
            this.pictureBox_flightsim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox_flightsim.Name = "pictureBox_flightsim";
            this.pictureBox_flightsim.Size = new System.Drawing.Size(348, 157);
            this.pictureBox_flightsim.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_flightsim.TabIndex = 112;
            this.pictureBox_flightsim.TabStop = false;
            // 
            // logLevelcmb
            // 
            this.logLevelcmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.logLevelcmb.FormattingEnabled = true;
            this.logLevelcmb.Items.AddRange(new object[] {
            "ERROR",
            "WARNING",
            "MESSAGE",
            "VERBOSE"});
            this.logLevelcmb.Location = new System.Drawing.Point(66, 495);
            this.logLevelcmb.Name = "logLevelcmb";
            this.logLevelcmb.Size = new System.Drawing.Size(103, 21);
            this.logLevelcmb.TabIndex = 35;
            this.logLevelcmb.SelectedIndexChanged += new System.EventHandler(this.logLevelcmb_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 498);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 13);
            this.label9.TabIndex = 43;
            this.label9.Text = "Log level:";
            // 
            // ExporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1276, 898);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.logLevelcmb);
            this.Controls.Add(this.pictureBox_flightsim);
            this.Controls.Add(this.grp_FlightSim);
            this.Controls.Add(this.butMultiExport);
            this.Controls.Add(this.butExportAndRun);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.butClose);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.butExport);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(846, 388);
            this.Name = "ExporterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Babylon.js - Export scene to babylon or glTF format";
            this.Activated += new System.EventHandler(this.ExporterForm_Activated);
            this.Deactivate += new System.EventHandler(this.ExporterForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExporterForm_FormClosed);
            this.Load += new System.EventHandler(this.ExporterForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExporterForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpGeometry.ResumeLayout(false);
            this.grpGeometry.PerformLayout();
            this.grpAnimations.ResumeLayout(false);
            this.grpAnimations.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.grp_FlightSim.ResumeLayout(false);
            this.grp_FlightSim.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_flightsim)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox txtModelPath;
        private System.Windows.Forms.Button butModelBrowse;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Button butCancel;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.CheckBox chkManifest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkHidden;
        private System.Windows.Forms.CheckBox chkAutoSave;
        private System.Windows.Forms.Button butExportAndRun;
        private System.Windows.Forms.CheckBox chkOnlySelected;
        private System.Windows.Forms.Button butClose;
        private System.Windows.Forms.ComboBox comboOutputFormat;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtScaleFactor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkExportTangents;
        private System.Windows.Forms.Label labelQuality;
        private System.Windows.Forms.TextBox txtQuality;
        private System.Windows.Forms.CheckBox chkMergeAOwithMR;
        private System.Windows.Forms.CheckBox chkDracoCompression;
        private System.Windows.Forms.ToolTip toolTipDracoCompression;
        private System.Windows.Forms.CheckBox chkOverwriteTextures;
        private System.Windows.Forms.Button butMultiExport;
        private System.Windows.Forms.CheckBox chkKHRLightsPunctual;
        private System.Windows.Forms.CheckBox chkKHRTextureTransform;
        private System.Windows.Forms.CheckBox chkKHRMaterialsUnlit;
        private System.Windows.Forms.CheckBox chkExportMaterials;
        private System.Windows.Forms.Label textureLabel;
        private System.Windows.Forms.RichTextBox txtTexturesPath;
        private System.Windows.Forms.Button btnTxtBrowse;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RichTextBox txtEnvironmentName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkFullPBR;
        private System.Windows.Forms.Button btnEnvBrowse;
        private System.Windows.Forms.CheckBox chkNoAutoLight;
        private System.Windows.Forms.CheckBox chkWriteTextures;
        private System.Windows.Forms.OpenFileDialog envFileDialog;
        private System.Windows.Forms.CheckBox chkAnimgroupExportNonAnimated;
        private System.Windows.Forms.CheckBox chkExportMorphTangents;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkExportMorphNormals;
        private System.Windows.Forms.CheckBox chkUsePreExportProces;
        private System.Windows.Forms.CheckBox chkMrgContainersAndXref;
        private System.Windows.Forms.GroupBox grpGeometry;
        private System.Windows.Forms.GroupBox grpAnimations;
        private System.Windows.Forms.ComboBox cmbExportAnimationType;
        private System.Windows.Forms.CheckBox chkApplyPreprocessToScene;
        private System.Windows.Forms.Label lblBakeAnimation;
        private System.Windows.Forms.ComboBox cmbBakeAnimationOptions;
        private System.Windows.Forms.CheckBox chkASBAnimationRetargeting;
        private System.Windows.Forms.GroupBox grp_FlightSim;
        private System.Windows.Forms.CheckBox chkRemoveNamespace;
        private System.Windows.Forms.CheckBox chk_RemoveLodPrefix;
        private System.Windows.Forms.PictureBox pictureBox_flightsim;
        private System.Windows.Forms.Label normalMapConventionLbl;
        private System.Windows.Forms.ComboBox cmbNormalMapConvention;
        private System.Windows.Forms.TextBox txtDstTextureExt;
        private System.Windows.Forms.Label lblCE_to;
        private System.Windows.Forms.Label lblCE_from;
        private System.Windows.Forms.TextBox txtSrcTextureExt;
        private System.Windows.Forms.Label lblConvertExtension;
        private System.Windows.Forms.ComboBox logLevelcmb;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkKeepInstances;
    }
}
