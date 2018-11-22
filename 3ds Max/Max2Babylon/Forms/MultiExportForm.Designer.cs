﻿namespace Max2Babylon
{
    partial class MultiExportForm
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
            System.Windows.Forms.Label warningLabel;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiExportForm));
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            System.Windows.Forms.Panel panel_buttons;
            this.panel1 = new System.Windows.Forms.Panel();
            this.ExportItemGridView = new System.Windows.Forms.DataGridView();
            this.ColumnExportCheckbox = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnNode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnFilePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_accept = new System.Windows.Forms.Button();
            warningLabel = new System.Windows.Forms.Label();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            panel_buttons = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExportItemGridView)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel_buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ExportItemGridView);
            this.panel1.Controls.Add(tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(764, 341);
            this.panel1.TabIndex = 0;
            // 
            // ExportItemGridView
            // 
            this.ExportItemGridView.AllowUserToResizeColumns = false;
            this.ExportItemGridView.AllowUserToResizeRows = false;
            this.ExportItemGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ExportItemGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.ExportItemGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this.ExportItemGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.ExportItemGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ExportItemGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnExportCheckbox,
            this.ColumnNode,
            this.ColumnFilePath});
            this.ExportItemGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportItemGridView.Location = new System.Drawing.Point(3, 3);
            this.ExportItemGridView.MultiSelect = false;
            this.ExportItemGridView.Name = "ExportItemGridView";
            this.ExportItemGridView.RowHeadersWidth = 27;
            this.ExportItemGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.ExportItemGridView.Size = new System.Drawing.Size(758, 256);
            this.ExportItemGridView.TabIndex = 8;
            this.ExportItemGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ExportItemGridView_CellContentClick);
            this.ExportItemGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ExportItemGridView_CellDoubleClick);
            this.ExportItemGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.ExportItemGridView_CellValidating);
            this.ExportItemGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ExportItemGridView_CellValueChanged);
            this.ExportItemGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportItemGridView_KeyDown);
            // 
            // ColumnExportCheckbox
            // 
            this.ColumnExportCheckbox.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ColumnExportCheckbox.HeaderText = "Export?";
            this.ColumnExportCheckbox.Name = "ColumnExportCheckbox";
            this.ColumnExportCheckbox.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnExportCheckbox.Width = 49;
            // 
            // ColumnNode
            // 
            this.ColumnNode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnNode.HeaderText = "Root node";
            this.ColumnNode.MaxInputLength = 1024;
            this.ColumnNode.Name = "ColumnNode";
            this.ColumnNode.ReadOnly = true;
            this.ColumnNode.Width = 82;
            // 
            // ColumnFilePath
            // 
            this.ColumnFilePath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnFilePath.HeaderText = "File path";
            this.ColumnFilePath.MaxInputLength = 1024;
            this.ColumnFilePath.Name = "ColumnFilePath";
            // 
            // warningLabel
            // 
            warningLabel.AutoEllipsis = true;
            warningLabel.AutoSize = true;
            warningLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            warningLabel.Dock = System.Windows.Forms.DockStyle.Left;
            warningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            warningLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            warningLabel.Location = new System.Drawing.Point(3, 3);
            warningLabel.Margin = new System.Windows.Forms.Padding(3);
            warningLabel.Name = "warningLabel";
            warningLabel.Padding = new System.Windows.Forms.Padding(3);
            warningLabel.Size = new System.Drawing.Size(432, 73);
            warningLabel.TabIndex = 10;
            warningLabel.Text = resources.GetString("warningLabel.Text");
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(panel_buttons, 0, 0);
            tableLayoutPanel1.Controls.Add(warningLabel, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            tableLayoutPanel1.Location = new System.Drawing.Point(3, 259);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(758, 79);
            tableLayoutPanel1.TabIndex = 12;
            // 
            // btn_cancel
            // 
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_cancel.Location = new System.Drawing.Point(123, 3);
            this.btn_cancel.MaximumSize = new System.Drawing.Size(120, 27);
            this.btn_cancel.MinimumSize = new System.Drawing.Size(120, 27);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(120, 27);
            this.btn_cancel.TabIndex = 6;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // btn_accept
            // 
            this.btn_accept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_accept.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_accept.Location = new System.Drawing.Point(3, 3);
            this.btn_accept.MaximumSize = new System.Drawing.Size(120, 27);
            this.btn_accept.MinimumSize = new System.Drawing.Size(120, 27);
            this.btn_accept.Name = "btn_accept";
            this.btn_accept.Size = new System.Drawing.Size(120, 27);
            this.btn_accept.TabIndex = 7;
            this.btn_accept.Text = "Accept";
            this.btn_accept.UseVisualStyleBackColor = true;
            this.btn_accept.Click += new System.EventHandler(this.btn_accept_Click);
            // 
            // panel_buttons
            // 
            panel_buttons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            panel_buttons.AutoSize = true;
            panel_buttons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel_buttons.Controls.Add(this.btn_accept);
            panel_buttons.Controls.Add(this.btn_cancel);
            panel_buttons.Location = new System.Drawing.Point(509, 42);
            panel_buttons.MinimumSize = new System.Drawing.Size(94, 34);
            panel_buttons.Name = "panel_buttons";
            panel_buttons.Padding = new System.Windows.Forms.Padding(3);
            panel_buttons.Size = new System.Drawing.Size(246, 34);
            panel_buttons.TabIndex = 9;
            // 
            // MultiExportForm
            // 
            this.AcceptButton = this.btn_accept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(764, 341);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "MultiExportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multi-File Export";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExportItemGridView)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel_buttons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView ExportItemGridView;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnExportCheckbox;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFilePath;
        private System.Windows.Forms.Button btn_accept;
        private System.Windows.Forms.Button btn_cancel;
    }
}