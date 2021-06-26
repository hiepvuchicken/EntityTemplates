using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using NAD.Common.BarcodeLib;

namespace CommonLib
{
    public class Barcode
    {
        public static byte[] ImageBarcodeBC07(string BC07Code)
        {
            NAD.Common.BarcodeLib.Barcode barcode = new NAD.Common.BarcodeLib.Barcode();
            return ImageToByte(barcode.Encode(TYPE.CODE128, BC07Code, 262, 42));
        }

        /// <summary>
        /// DUCNV: Hàm lấy mã vạch của bc34
        /// </summary>
        /// <param name="BC34Code">mã túi thư</param>
        /// <returns>ảnh mã vạch</returns>
        public static byte[] ImageBarcodeBC34(string BC34Code)
        {
            try
            {
                NAD.Common.BarcodeLib.Barcode barcode = new NAD.Common.BarcodeLib.Barcode();
                return ImageToByte(barcode.Encode(TYPE.CODE128, BC34Code, 255, 31));
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ImageItemBarcode(string strItemBatchCode)
        {
            NAD.Common.BarcodeLib.Barcode barcode = new NAD.Common.BarcodeLib.Barcode();
            return ImageToByte(barcode.Encode(TYPE.CODE128, strItemBatchCode, 255, 31));
        }

        public static byte[] ImageBarcodeBD13(string bd13Barcode)
        {
            NAD.Common.BarcodeLib.Barcode barcode = new NAD.Common.BarcodeLib.Barcode();
            return ImageToByte(barcode.Encode(TYPE.CODE128, bd13Barcode, 500, 31));
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
