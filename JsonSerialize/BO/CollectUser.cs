using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSerialize.BO
{
    public class CollectUser
    {
        public string POSCode { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Mobile { get; set; }
        public string Description { get; set; }
        public string PosmanCode { get; set; }
    }

    public class RequestSyncCollectUser
    {
        public IList<CollectUser> collectUsers { get; set; }
        public string token { get; set; }
    }

    public class ResponseSyncUser
    {
        public bool status { get; set; }
        public string errorContent { get; set; }
       public int totalSuccess { get; set; }
        public int totalFail { get; set; }
        public IList<SyncUser> successUsers { get; set; }
        public IList<SyncUser> failUsers { get; set; }
    }

    public class SyncUser
    {
        public string errorCode { get; set; }
        public string errorContent { get; set; }
        public string POSCode { get; set; }
        public string PosmanCode { get; set; }
    }

    
}
