using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft;
using RestSharp;


namespace Test
{
    public partial class Form1 : Form
    {
        private string _textOtherForm;
        public string textOtherForm {
            get { return _textOtherForm; }
            set { _textOtherForm = value; }
        }
        public Form1()
        {
            InitializeComponent();

            Form2 form2 = new Form2();
            form2.ShowDialog();
            _textOtherForm = form2.text;
        }

        private void btnPostOrder_Click(object sender, EventArgs e)
        {
            PostOrder();
           
        }

        private void PostOrder()
        {
            try
            {

                RestClient client = new RestClient(txtUrl.Text.ToString());
                var request = new RestRequest($"/serviceApi/v2/ReceiveOrder?token={txtToken.Text.ToString()}", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(txtDataSend.Text.ToString());

                var result = client.Execute(request);
                if (result != null && result.Content != null && result.Content.Length > 0)
                {
                    txtReceive.Text = result.Content;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }


}


