namespace fxdataloader
{
    partial class FormMain
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
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.tableLayoutPanelControl = new System.Windows.Forms.TableLayoutPanel();
            this.labelSchema = new System.Windows.Forms.Label();
            this.labelLogin = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.textBoxSchema = new System.Windows.Forms.TextBox();
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxLanguage = new System.Windows.Forms.TextBox();
            this.buttonLogon = new System.Windows.Forms.Button();
            this.buttonGetPairs = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStripMain.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelStatus});
            this.statusStripMain.Location = new System.Drawing.Point(0, 448);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(636, 22);
            this.statusStripMain.TabIndex = 0;
            this.statusStripMain.Text = "statusStripMain";
            // 
            // toolStripStatusLabelStatus
            // 
            this.toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
            this.toolStripStatusLabelStatus.Size = new System.Drawing.Size(51, 17);
            this.toolStripStatusLabelStatus.Text = "Unknown";
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 2;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.textBoxLog, 1, 0);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelControl, 0, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 1;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 448F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(636, 448);
            this.tableLayoutPanelMain.TabIndex = 1;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(203, 3);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(430, 442);
            this.textBoxLog.TabIndex = 1;
            // 
            // tableLayoutPanelControl
            // 
            this.tableLayoutPanelControl.ColumnCount = 2;
            this.tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelControl.Controls.Add(this.labelSchema, 0, 0);
            this.tableLayoutPanelControl.Controls.Add(this.labelLogin, 0, 1);
            this.tableLayoutPanelControl.Controls.Add(this.labelPassword, 0, 2);
            this.tableLayoutPanelControl.Controls.Add(this.labelLanguage, 0, 3);
            this.tableLayoutPanelControl.Controls.Add(this.textBoxSchema, 1, 0);
            this.tableLayoutPanelControl.Controls.Add(this.textBoxLogin, 1, 1);
            this.tableLayoutPanelControl.Controls.Add(this.textBoxPassword, 1, 2);
            this.tableLayoutPanelControl.Controls.Add(this.textBoxLanguage, 1, 3);
            this.tableLayoutPanelControl.Controls.Add(this.buttonLogon, 1, 4);
            this.tableLayoutPanelControl.Controls.Add(this.buttonGetPairs, 1, 5);
            this.tableLayoutPanelControl.Controls.Add(this.label2, 1, 7);
            this.tableLayoutPanelControl.Controls.Add(this.button1, 1, 6);
            this.tableLayoutPanelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelControl.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelControl.Name = "tableLayoutPanelControl";
            this.tableLayoutPanelControl.RowCount = 9;
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelControl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelControl.Size = new System.Drawing.Size(194, 442);
            this.tableLayoutPanelControl.TabIndex = 2;
            // 
            // labelSchema
            // 
            this.labelSchema.AutoSize = true;
            this.labelSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSchema.Location = new System.Drawing.Point(3, 0);
            this.labelSchema.Name = "labelSchema";
            this.labelSchema.Size = new System.Drawing.Size(59, 25);
            this.labelSchema.TabIndex = 0;
            this.labelSchema.Text = "Schema:";
            this.labelSchema.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelLogin
            // 
            this.labelLogin.AutoSize = true;
            this.labelLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLogin.Location = new System.Drawing.Point(3, 25);
            this.labelLogin.Name = "labelLogin";
            this.labelLogin.Size = new System.Drawing.Size(59, 25);
            this.labelLogin.TabIndex = 1;
            this.labelLogin.Text = "Login:";
            this.labelLogin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPassword.Location = new System.Drawing.Point(3, 50);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(59, 25);
            this.labelPassword.TabIndex = 2;
            this.labelPassword.Text = "Password:";
            this.labelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLanguage.Location = new System.Drawing.Point(3, 75);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(59, 25);
            this.labelLanguage.TabIndex = 3;
            this.labelLanguage.Text = "Language:";
            this.labelLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxSchema
            // 
            this.textBoxSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSchema.Location = new System.Drawing.Point(68, 3);
            this.textBoxSchema.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.textBoxSchema.Name = "textBoxSchema";
            this.textBoxSchema.Size = new System.Drawing.Size(123, 20);
            this.textBoxSchema.TabIndex = 5;
            this.textBoxSchema.Text = "api.10";
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLogin.Location = new System.Drawing.Point(68, 28);
            this.textBoxLogin.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(123, 20);
            this.textBoxLogin.TabIndex = 6;
            this.textBoxLogin.Text = "demo397";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPassword.Location = new System.Drawing.Point(68, 53);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(123, 20);
            this.textBoxPassword.TabIndex = 7;
            this.textBoxPassword.Text = "1475";
            // 
            // textBoxLanguage
            // 
            this.textBoxLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLanguage.Location = new System.Drawing.Point(68, 78);
            this.textBoxLanguage.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.textBoxLanguage.Name = "textBoxLanguage";
            this.textBoxLanguage.Size = new System.Drawing.Size(123, 20);
            this.textBoxLanguage.TabIndex = 8;
            this.textBoxLanguage.Text = "en_US";
            // 
            // buttonLogon
            // 
            this.buttonLogon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonLogon.Location = new System.Drawing.Point(68, 101);
            this.buttonLogon.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.buttonLogon.Name = "buttonLogon";
            this.buttonLogon.Size = new System.Drawing.Size(123, 23);
            this.buttonLogon.TabIndex = 9;
            this.buttonLogon.Text = "Logon";
            this.buttonLogon.UseVisualStyleBackColor = true;
            this.buttonLogon.Click += new System.EventHandler(this.buttonLogon_Click);
            // 
            // buttonGetPairs
            // 
            this.buttonGetPairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonGetPairs.Location = new System.Drawing.Point(68, 126);
            this.buttonGetPairs.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.buttonGetPairs.Name = "buttonGetPairs";
            this.buttonGetPairs.Size = new System.Drawing.Size(123, 23);
            this.buttonGetPairs.TabIndex = 10;
            this.buttonGetPairs.Text = "Get Pairs";
            this.buttonGetPairs.UseVisualStyleBackColor = true;
            this.buttonGetPairs.Click += new System.EventHandler(this.buttonGetPairs_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(68, 175);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "label2";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(68, 153);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 19);
            this.button1.TabIndex = 13;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 470);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.statusStripMain);
            this.Name = "FormMain";
            this.Text = "Tool for loading fxdata.";
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            this.tableLayoutPanelControl.ResumeLayout(false);
            this.tableLayoutPanelControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelControl;
        private System.Windows.Forms.Label labelSchema;
        private System.Windows.Forms.Label labelLogin;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.TextBox textBoxSchema;
        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxLanguage;
        private System.Windows.Forms.Button buttonLogon;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelStatus;
        private System.Windows.Forms.Button buttonGetPairs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
    }
}

