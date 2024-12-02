using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class TransactionStatus
    {
        public Guid TransactionId { get; set; }
        public bool Status { get; set; }
        public string StatusDescription { get; set; }
    }
}
