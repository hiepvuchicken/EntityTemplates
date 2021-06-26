using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BCCPAPI
{
    public partial class Form1 : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Form1));
        public Form1()
        {
            InitializeComponent();
        }

        public void SendMailTrip()
        {
            try
            {


            }
            catch (Exception ex)
            {
                log.Error($"ex.Message: {ex.Message} /n ex.InnerException: {ex.InnerException}");
                throw ex;
            }
        }

        public void SendItem()
        {
            try
            {
                //AddItemRequest client = new AddItemRequest();
                //ItemDataSet dsItem = new ItemDataSet();
            }
            catch (Exception ex)
            {
                log.Error($"ex.Message: {ex.Message} /n ex.InnerException: {ex.InnerException}");
                throw ex;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetMailTrip();
        }

        private void GetMailTrip()
        {
            try
            {
                //MailTripDataSet dsMailTrip = new MailTripDataSet();
                //GetMailTripRequest client = new GetMailTripRequest();
                //ExchangeUat.Exchange
                //ExchangeServiceLive.ServiceClient clients = new ExchangeServiceLive.ServiceClient("httpExchangeService");
                gw.ServiceClient client = new gw.ServiceClient();
                var s = client.GetMailTrip("100000", "100910", "1","C", "20171027", "366", "OE", "OE");

            }
            catch (Exception ex)
            {
                log.Error($"ex.Message: {ex.Message} /n ex.InnerException: {ex.InnerException}");
                throw ex;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //ExS_uat.ServiceClient client = new ExS_uat.ServiceClient("httpExchangeService");
                //var s = client.GetMailTrip("600100", "100916", "3", "E", "20180906", "2247", "OE", "OE");
            }
            catch (Exception ex)
            {
                log.Error($"ex.Message: {ex.Message} /n ex.InnerException: {ex.InnerException}");
                throw ex;
            }
            
        }
    }
}
