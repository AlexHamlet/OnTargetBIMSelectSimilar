using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectSimilar
{
    [Serializable]
    public class EntitlementResponse
    {
        public string UserId { get; set; }
        public string AppId { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }

    [Serializable]
    public class EntitlementCache
    {
        public DateTime? Time { get; set; }
        public bool PrevEntitled { get; set; }
    }
}
