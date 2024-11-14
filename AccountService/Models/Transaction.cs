namespace AccountService.Models
{
    public class Transaction
    {
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public string TransactionType { get; set; } // Debit/Credit
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
    }
}
