using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSerialize.BO
{
    public class ObjectMessage
    {
        public string TXDATE { get; set; }
        public string TXNUM { get; set; }
        public string TXTIME { get; set; }
        public string TLID { get; set; }
        public string BRID { get; set; }
        public string MSGTYPE { get; set; }
        public string OBJNAME { get; set; }
        public string ACTFLAG { get; set; }
        public string CMDINQUIRY { get; set; }
        public string CLAUSE { get; set; }

    }
}
