using System.ComponentModel.DataAnnotations;

namespace AccountService.Data.Entities
{
    public class AccountModel
    {
        [Key]
        public int AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string AccountHolderName { get; set; }
        public decimal Balance { get; set; }
    }
}
