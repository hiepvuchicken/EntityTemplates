using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSerialize.BO
{
    public class SortingList
    {
        public List<SortingDetails> sortingDetails { get; set; }
    }

    public class SortingDetails
    {
        public string POSCode { get; set; }
        public string POSName { get; set; }
        public string POSFileWav { get; set; }
        public string SortingCode { get; set; }
    }
}
