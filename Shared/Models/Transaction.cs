using System.ComponentModel.DataAnnotations;

namespace AccountService.Models
{
    public class Transaction
    {
        public int FromAccount { get; set; }
        public int ToAccount { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }
}
