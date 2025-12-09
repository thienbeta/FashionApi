using System.Collections.Generic;

namespace FashionApi.Models.View
{
    public class BatchDeleteResult
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<int> SuccessfulIds { get; set; } = new List<int>();
        public List<int> FailedIds { get; set; } = new List<int>();
        public string Message { get; set; } = string.Empty;
    }
}
