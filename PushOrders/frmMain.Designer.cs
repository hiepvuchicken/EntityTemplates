namespace PushOrders
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
            this.btnPushInfo = new System.Windows.Forms.Button();
            this.btnPushInfos = new System.Windows.Forms.Button();
            this.rtxtResult = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFake = new System.Windows.Forms.Button();
            this.cboCustomer = new System.Windows.Forms.ComboBox();
            this.cboSenderProvince = new System.Windows.Forms.ComboBox();
            this.cboReceiverProvince = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtOrderVol = new System.Windows.Forms.TextBox();
            this.btnSendData = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPushInfo
            // 
            this.btnPushInfo.Location = new System.Drawing.Point(15, 13);
            this.btnPushInfo.Name = "btnPushInfo";
            this.btnPushInfo.Size = new System.Drawing.Size(87, 25);
            this.btnPushInfo.TabIndex = 0;
            this.btnPushInfo.Text = "Push Info";
            this.btnPushInfo.UseVisualStyleBackColor = true;
            // 
            // btnPushInfos
            // 
            this.btnPushInfos.Location = new System.Drawing.Point(110, 13);
            this.btnPushInfos.Name = "btnPushInfos";
            this.btnPushInfos.Size = new System.Drawing.Size(87, 25);
            this.btnPushInfos.TabIndex = 1;
            this.btnPushInfos.Text = "Push Infos";
            this.btnPushInfos.UseVisualStyleBackColor = true;
            // 
            // rtxtResult
            // 
            this.rtxtResult.Location = new System.Drawing.Point(15, 270);
            this.rtxtResult.Name = "rtxtResult";
            this.rtxtResult.Size = new System.Drawing.Size(920, 326);
            this.rtxtResult.TabIndex = 2;
            this.rtxtResult.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Constantia", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(15, 250);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 14);
            this.label1.TabIndex = 3;
            this.label1.Text = "Result :";
            // 
            // btnFake
            // 
            this.btnFake.Location = new System.Drawing.Point(203, 13);
            this.btnFake.Name = "btnFake";
            this.btnFake.Size = new System.Drawing.Size(87, 25);
            this.btnFake.TabIndex = 4;
            this.btnFake.Text = "Fake Data";
            this.btnFake.UseVisualStyleBackColor = true;
            this.btnFake.Click += new System.EventHandler(this.btnFake_Click);
            // 
            // cboCustomer
            // 
            this.cboCustomer.FormattingEnabled = true;
            this.cboCustomer.Location = new System.Drawing.Point(110, 78);
            this.cboCustomer.Name = "cboCustomer";
            this.cboCustomer.Size = new System.Drawing.Size(121, 22);
            this.cboCustomer.TabIndex = 5;
            this.cboCustomer.Leave += new System.EventHandler(this.cboCustomer_Leave);
            // 
            // cboSenderProvince
            // 
            this.cboSenderProvince.FormattingEnabled = true;
            this.cboSenderProvince.Location = new System.Drawing.Point(110, 106);
            this.cboSenderProvince.Name = "cboSenderProvince";
            this.cboSenderProvince.Size = new System.Drawing.Size(121, 22);
            this.cboSenderProvince.TabIndex = 6;
            // 
            // cboReceiverProvince
            // 
            this.cboReceiverProvince.FormattingEnabled = true;
            this.cboReceiverProvince.Location = new System.Drawing.Point(331, 107);
            this.cboReceiverProvince.Name = "cboReceiverProvince";
            this.cboReceiverProvince.Size = new System.Drawing.Size(121, 22);
            this.cboReceiverProvince.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 14);
            this.label2.TabIndex = 8;
            this.label2.Text = "Mã KHL";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 14);
            this.label3.TabIndex = 9;
            this.label3.Text = "Tỉnh phát hành";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(237, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 14);
            this.label4.TabIndex = 10;
            this.label4.Text = "Tỉnh phát";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 139);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 14);
            this.label5.TabIndex = 11;
            this.label5.Text = "Số lượng tin";
            // 
            // txtOrderVol
            // 
            this.txtOrderVol.Location = new System.Drawing.Point(110, 135);
            this.txtOrderVol.Name = "txtOrderVol";
            this.txtOrderVol.Size = new System.Drawing.Size(121, 22);
            this.txtOrderVol.TabIndex = 12;
            // 
            // btnSendData
            // 
            this.btnSendData.Location = new System.Drawing.Point(851, 12);
            this.btnSendData.Name = "btnSendData";
            this.btnSendData.Size = new System.Drawing.Size(87, 25);
            this.btnSendData.TabIndex = 13;
            this.btnSendData.Text = "Send Data";
            this.btnSendData.UseVisualStyleBackColor = true;
            this.btnSendData.Click += new System.EventHandler(this.btnSendData_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 610);
            this.Controls.Add(this.btnSendData);
            this.Controls.Add(this.txtOrderVol);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboReceiverProvince);
            this.Controls.Add(this.cboSenderProvince);
            this.Controls.Add(this.cboCustomer);
            this.Controls.Add(this.btnFake);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rtxtResult);
            this.Controls.Add(this.btnPushInfos);
            this.Controls.Add(this.btnPushInfo);
            this.Font = new System.Drawing.Font("Constantia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmMain";
            this.Text = "Push Orders";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPushInfo;
        private System.Windows.Forms.Button btnPushInfos;
        private System.Windows.Forms.RichTextBox rtxtResult;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnFake;
        private System.Windows.Forms.ComboBox cboCustomer;
        private System.Windows.Forms.ComboBox cboSenderProvince;
        private System.Windows.Forms.ComboBox cboReceiverProvince;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtOrderVol;
        private System.Windows.Forms.Button btnSendData;
    }
}