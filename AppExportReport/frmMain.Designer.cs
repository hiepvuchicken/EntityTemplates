namespace AppExportReport
{
    partial class frmMain
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
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtContractNumber = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtReceiver = new System.Windows.Forms.TextBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.lblNotification = new System.Windows.Forms.Label();
            this.txtPathSave = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnPathSave = new System.Windows.Forms.Button();
            this.txtMonth = new System.Windows.Forms.TextBox();
            this.btnSavePDF = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.prgBpercent = new System.Windows.Forms.ProgressBar();
            this.lblPercent = new System.Windows.Forms.Label();
            this.btnConfig = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(109, 19);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(644, 26);
            this.txtFilePath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "File dữ liệu";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(760, 19);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(110, 26);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "Chọn file";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "Tháng";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 19);
            this.label3.TabIndex = 5;
            this.label3.Text = "Mã HĐ";
            // 
            // txtContractNumber
            // 
            this.txtContractNumber.Location = new System.Drawing.Point(109, 84);
            this.txtContractNumber.Name = "txtContractNumber";
            this.txtContractNumber.Size = new System.Drawing.Size(218, 26);
            this.txtContractNumber.TabIndex = 3;
            this.txtContractNumber.Text = "(HĐ 44/2020/HĐKT/TCT-FIS)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(333, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 19);
            this.label4.TabIndex = 7;
            this.label4.Text = "Người tiếp nhận";
            // 
            // txtReceiver
            // 
            this.txtReceiver.Location = new System.Drawing.Point(456, 52);
            this.txtReceiver.Name = "txtReceiver";
            this.txtReceiver.Size = new System.Drawing.Size(297, 26);
            this.txtReceiver.TabIndex = 2;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(786, 211);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(110, 26);
            this.btnExport.TabIndex = 6;
            this.btnExport.Text = "Kết xuất Word";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // lblNotification
            // 
            this.lblNotification.AutoSize = true;
            this.lblNotification.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNotification.ForeColor = System.Drawing.Color.Red;
            this.lblNotification.Location = new System.Drawing.Point(12, 9);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(94, 21);
            this.lblNotification.TabIndex = 11;
            this.lblNotification.Text = "Thông báo";
            // 
            // txtPathSave
            // 
            this.txtPathSave.Location = new System.Drawing.Point(109, 116);
            this.txtPathSave.Name = "txtPathSave";
            this.txtPathSave.Size = new System.Drawing.Size(644, 26);
            this.txtPathSave.TabIndex = 4;
            this.txtPathSave.Text = "D:\\WORKING\\report\\";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(9, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(94, 19);
            this.label5.TabIndex = 13;
            this.label5.Text = "Thư mục lưu";
            // 
            // btnPathSave
            // 
            this.btnPathSave.Location = new System.Drawing.Point(760, 116);
            this.btnPathSave.Name = "btnPathSave";
            this.btnPathSave.Size = new System.Drawing.Size(110, 26);
            this.btnPathSave.TabIndex = 14;
            this.btnPathSave.Text = "Chọn thư mục";
            this.btnPathSave.UseVisualStyleBackColor = true;
            this.btnPathSave.Click += new System.EventHandler(this.btnPathSave_Click);
            // 
            // txtMonth
            // 
            this.txtMonth.Location = new System.Drawing.Point(109, 52);
            this.txtMonth.Name = "txtMonth";
            this.txtMonth.Size = new System.Drawing.Size(218, 26);
            this.txtMonth.TabIndex = 15;
            this.txtMonth.Text = "0";
            // 
            // btnSavePDF
            // 
            this.btnSavePDF.Location = new System.Drawing.Point(670, 211);
            this.btnSavePDF.Name = "btnSavePDF";
            this.btnSavePDF.Size = new System.Drawing.Size(110, 26);
            this.btnSavePDF.TabIndex = 16;
            this.btnSavePDF.Text = "Kết xuất PDF";
            this.btnSavePDF.UseVisualStyleBackColor = true;
            this.btnSavePDF.Click += new System.EventHandler(this.btnSavePDF_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtFilePath);
            this.groupBox1.Controls.Add(this.txtMonth);
            this.groupBox1.Controls.Add(this.btnBrowse);
            this.groupBox1.Controls.Add(this.btnPathSave);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtPathSave);
            this.groupBox1.Controls.Add(this.txtContractNumber);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtReceiver);
            this.groupBox1.Location = new System.Drawing.Point(12, 46);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(884, 159);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông tin nhập";
            // 
            // prgBpercent
            // 
            this.prgBpercent.Location = new System.Drawing.Point(436, 211);
            this.prgBpercent.Name = "prgBpercent";
            this.prgBpercent.Size = new System.Drawing.Size(228, 26);
            this.prgBpercent.TabIndex = 18;
            // 
            // lblPercent
            // 
            this.lblPercent.AutoSize = true;
            this.lblPercent.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPercent.Location = new System.Drawing.Point(518, 216);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(64, 15);
            this.lblPercent.TabIndex = 16;
            this.lblPercent.Text = "lblPercent";
            this.lblPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnConfig
            // 
            this.btnConfig.Location = new System.Drawing.Point(12, 212);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(110, 28);
            this.btnConfig.TabIndex = 16;
            this.btnConfig.Text = "Cấu hình";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Visible = false;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 252);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.lblPercent);
            this.Controls.Add(this.prgBpercent);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSavePDF);
            this.Controls.Add(this.lblNotification);
            this.Controls.Add(this.btnExport);
            this.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Kết xuất báo cáo";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtContractNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtReceiver;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label lblNotification;
        private System.Windows.Forms.TextBox txtPathSave;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnPathSave;
        private System.Windows.Forms.TextBox txtMonth;
        private System.Windows.Forms.Button btnSavePDF;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ProgressBar prgBpercent;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.Button btnConfig;
    }
}

