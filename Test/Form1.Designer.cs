namespace Test
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnPostOrder = new System.Windows.Forms.Button();
            this.txtDataSend = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtReceive = new System.Windows.Forms.TextBox();
            this.btnPostListOrder = new System.Windows.Forms.Button();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.lblvalueF2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnPostOrder
            // 
            this.btnPostOrder.Location = new System.Drawing.Point(12, 13);
            this.btnPostOrder.Name = "btnPostOrder";
            this.btnPostOrder.Size = new System.Drawing.Size(75, 23);
            this.btnPostOrder.TabIndex = 0;
            this.btnPostOrder.Text = "Post Order";
            this.btnPostOrder.UseVisualStyleBackColor = true;
            this.btnPostOrder.Click += new System.EventHandler(this.btnPostOrder_Click);
            // 
            // txtDataSend
            // 
            this.txtDataSend.Location = new System.Drawing.Point(12, 65);
            this.txtDataSend.Multiline = true;
            this.txtDataSend.Name = "txtDataSend";
            this.txtDataSend.Size = new System.Drawing.Size(775, 149);
            this.txtDataSend.TabIndex = 1;
            this.txtDataSend.Text = resources.GetString("txtDataSend.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Data Send";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 217);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Receive";
            // 
            // txtReceive
            // 
            this.txtReceive.Location = new System.Drawing.Point(12, 233);
            this.txtReceive.Multiline = true;
            this.txtReceive.Name = "txtReceive";
            this.txtReceive.Size = new System.Drawing.Size(775, 104);
            this.txtReceive.TabIndex = 3;
            // 
            // btnPostListOrder
            // 
            this.btnPostListOrder.Location = new System.Drawing.Point(93, 13);
            this.btnPostListOrder.Name = "btnPostListOrder";
            this.btnPostListOrder.Size = new System.Drawing.Size(75, 23);
            this.btnPostListOrder.TabIndex = 5;
            this.btnPostListOrder.Text = "Post Orders";
            this.btnPostListOrder.UseVisualStyleBackColor = true;
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(278, 15);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(509, 20);
            this.txtUrl.TabIndex = 6;
            this.txtUrl.Text = "http://localhost:22504/serviceApi/v2/";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(252, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Url";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(234, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Token";
            // 
            // txtToken
            // 
            this.txtToken.Location = new System.Drawing.Point(278, 39);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(509, 20);
            this.txtToken.TabIndex = 9;
            this.txtToken.Text = "D8A91F1E-7E1B-43B2-B4C4-EE8D37192D9E";
            // 
            // lblvalueF2
            // 
            this.lblvalueF2.AutoSize = true;
            this.lblvalueF2.Location = new System.Drawing.Point(13, 344);
            this.lblvalueF2.Name = "lblvalueF2";
            this.lblvalueF2.Size = new System.Drawing.Size(35, 13);
            this.lblvalueF2.TabIndex = 10;
            this.lblvalueF2.Text = "label5";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 385);
            this.Controls.Add(this.lblvalueF2);
            this.Controls.Add(this.txtToken);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.btnPostListOrder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtReceive);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDataSend);
            this.Controls.Add(this.btnPostOrder);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPostOrder;
        private System.Windows.Forms.TextBox txtDataSend;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtReceive;
        private System.Windows.Forms.Button btnPostListOrder;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Label lblvalueF2;
    }
}