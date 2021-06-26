using AutoMapper;
using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTest.Models
{
    public class PrintItem
    {
        public string STT { get; set; }
        public string BC { get; set; }
        public string FullName { get; set; }
        public string Ngay { get; set; }
        public string ItemCode { get; set; }
        public string Receiverinfo { get; set; }
        public string receiverPhone { get; set; }
        public string SendingContent { get; set; }
        public string totalFreight { get; set; }
        public string DVGTGT { get; set; }
        public string CODAmount { get; set; }

        public PrintItem(service_process_LoadItemList_printf_dev_Result entity, bool encodeHtml = false)
        {
            if (entity != null)
            {
                Mapper.CreateMap<service_process_LoadItemList_printf_dev_Result, PrintItem>();
                Mapper.Map(entity, this);

            }
        }
    }
}