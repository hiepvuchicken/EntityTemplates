using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushOrders.ObjectsBussiness
{
    public class InfomationModel
    {
        public int RowIndex { get; set; }

        /// <summary>
        /// Id đơn hàng
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// PosIdthugom
        /// </summary>
        public Nullable<int> PosIdCollect { get; set; }

        /// <summary>
        /// Ma tinh gui
        /// </summary>
        public Nullable<int> SenderProvince { get; set; }

        /// <summary>
        /// Ma huyen gui
        /// </summary>
        public Nullable<int> SenderDistrict { get; set; }

        // Khách hàng import chính là CustomerCode
        /// <summary>
        /// Ma khach hang
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Số đơn hàng
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Mã bưu gửi
        /// </summary>
        public string ItemCode { get; set; }
        public Nullable<decimal> TrongLuong { get; set; }
        public Nullable<decimal> ChieuDai { get; set; }
        public Nullable<decimal> ChieuRong { get; set; }
        public Nullable<decimal> ChieuCao { get; set; }

        /// <summary>
        /// Địa chỉ người gửi
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Tên người gửi
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Điện thoại người gửi
        /// </summary>
        public string SenderTel { get; set; }

        /// <summary>
        /// Nội dung gửi
        /// </summary>
        public string SenderDesc { get; set; }

        /// <summary>
        /// Email người gửi
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Số tiền COD
        /// </summary>
        public Nullable<decimal> CODAmount { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Mã tỉnh nhận
        /// </summary>
        public Nullable<int> ReceiverProvince { get; set; }

        /// <summary>
        /// Mã Huyện nhận
        /// </summary>
        public Nullable<int> ReceiverDistrict { get; set; }

        /// <summary>
        /// Tên người nhận
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// Email người nhận
        /// </summary>
        public string ReceiverEmail { get; set; }

        /// <summary>
        /// Địa chỉ người nhận 
        /// </summary>
        public string ReceiverAddress { get; set; }

        /// <summary>
        /// Điện thoại nguời nhận
        /// </summary>
        public string ReceiverTel { get; set; }

        /// <summary>
        /// ExtendData
        /// </summary>
        public string ExtendData { get; set; }

        /// <summary>
        /// Nếu đơn hàng của khách hàng dropoff : giá trị FlagConfig = 1
        /// Nếu đơn hàng của khách hàng pickup: giá trị FlagConfig = 2
        /// Trường hợp khác gán: giá trị FlagConfig = null
        /// </summary>
        public int? FlagConfig { get; set; }

        public string SenderDistrictName { get; set; }

        public string ReceiverDistrictName { get; set; }

        public Nullable<decimal> CODofSender { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
        public Nullable<DateTime> CollectDate { get; set; }
    }
}
