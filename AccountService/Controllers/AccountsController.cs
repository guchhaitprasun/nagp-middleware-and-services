using AccountService.Data;
using AccountService.Data.Entities;
using AccountService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System;

namespace AccountService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreateAccount([FromBody] AccountRegistration account)
        {
            var result = CreateNewAccountInDatabase(account);
            if (int.Parse(result.AccountNumber) > 0)
            {
                return Ok(result);
            }

            return BadRequest();

        }

        [HttpPost]
        [Route("addfunds")]
        public IActionResult AddMoney([FromBody] Addfunds funds)
        {
            try
            {
                //Fetch Account holder
                var holder = _context.Accounts.Where(o => o.AccountNumber == funds.AccountNumber).FirstOrDefault();

                if (holder != null)
                {
                    holder.Balance += funds.Amount;

                    _context.Accounts.Update(holder);
                    _context.SaveChanges();

                    // Create New Transaction entry
                    _context.Transactions.Add(new TransactionModel
                    {
                        ReferenceNumber = Guid.NewGuid(),
                        FromAccountId = holder.AccountNumber, 
                        ToAccountId = holder.AccountNumber, 
                        TransactionType = "Credit",
                        Remark = "Fund Added to the Account",
                        Amount = funds.Amount,
                        DateTime = DateTime.Now,
                        TransactionForAccountId = holder.AccountNumber,
                    });
                    _context.SaveChanges();

                    return Ok("Funds Added Successfully");

                }

                return BadRequest("Account Does Not exists");
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

        }

        [HttpPost]
        [Route("sendmoney")]
        public IActionResult SendMoney([FromBody] Transaction transaction)
        {
            var result = ProcessFundTransfer(transaction);
            return Ok(result);
        }

        [HttpGet]
        [Route("{accountNumber}/statement")]
        public IActionResult GetAccountStatement(string accountNumber)
        {
            try
            {
                var transactions = _context
                                    .Transactions
                                    .Where(o => o.TransactionForAccountId == int.Parse(accountNumber))
                                    .Select(o => new
                                    {
                                        AccountNumber = accountNumber,
                                        TransactionType = o.TransactionType, 
                                        Amount = o.Amount,
                                        Remark = o.Remark,
                                        TransactionDate = o.DateTime, 
                                    })
                                    .OrderBy(o => o.TransactionDate)
                                    .ToList();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{accountNumber}/statement/pdf")]
        public IActionResult GetPdfStatement(string accountNumber)
        {
            return Ok("Account Statement pdf");
        }

        #region Private Region
        private Account CreateNewAccountInDatabase(AccountRegistration account)
        {
            Account createdAccount = new Account();
            try
            {
                AccountModel model = new AccountModel
                {
                    AccountType = account.AccountType.ToLower() == "current" ? "Current" : "Savings",
                    AccountHolderName = account.FullName,
                    Balance = 0
                };

                _context.Accounts.Add(model);
                _context.SaveChanges();

                createdAccount = new Account
                {
                    AccountHolderName = model.AccountHolderName,
                    Balance = model.Balance,
                    AccountType = account.AccountType,
                    AccountNumber = model.AccountNumber.ToString()
                };

            }
            catch (Exception)
            {

            }

            return createdAccount;
        }

        private TransactionStatus ProcessFundTransfer(Transaction transaction)
        {
            TransactionStatus status = new TransactionStatus();
            try
            {
                //Fetch Account Information of the receipient
                AccountModel? Sender = _context.Accounts.Where(o => o.AccountNumber == transaction.FromAccount).FirstOrDefault();
                AccountModel? Receiver = _context.Accounts.Where(o => o.AccountNumber == transaction.ToAccount).FirstOrDefault();


                //check if sender has enough balance
                if (Sender != null && Receiver != null)
                {
                    if (Sender.Balance >= transaction.Amount)
                    {
                        Sender.Balance -= transaction.Amount;
                        _context.Accounts.Update(Sender);

                        // Create New Transaction entry for debit
                        var debitTrnsaction = new TransactionModel
                        {
                            ReferenceNumber = Guid.NewGuid(),
                            TransactionType = "Debit",
                            Remark = $"Rs.{transaction.Amount} is debited from account number {Sender.AccountNumber}, available balance: Rs.{Sender.Balance - transaction.Amount}, Remark: {transaction.Note}",
                            Amount = transaction.Amount,
                            DateTime = DateTime.Now,
                            FromAccountId = Sender.AccountNumber, 
                            ToAccountId = Receiver.AccountNumber,
                            TransactionForAccountId = Sender.AccountNumber,
                        };
                        _context.Transactions.Add(debitTrnsaction);

                        Receiver.Balance += transaction.Amount;
                        _context.Accounts.Update(Receiver);

                        //Create New Transaction entry for credit
                        var creditTransaction = new TransactionModel
                        {
                            ReferenceNumber = debitTrnsaction.ReferenceNumber,
                            TransactionType = "Credit", 
                            Amount = transaction.Amount,
                            Remark = $"Rs.{transaction.Amount} Received from {Sender.AccountHolderName}, Account Number: {Sender.AccountNumber}", 
                            DateTime = DateTime.Now,
                            FromAccountId = Sender.AccountNumber,
                            ToAccountId = Receiver.AccountNumber,
                            TransactionForAccountId= Receiver.AccountNumber,
                        };
                        _context.Transactions.Add(creditTransaction);


                        _context.SaveChanges();

                        status = new TransactionStatus
                        {
                            Status = true,
                            TransactionId = creditTransaction.ReferenceNumber,
                            StatusDescription = "Transaction Completed Successfully"
                        };

                    }
                    else
                    {
                        status = new TransactionStatus
                        {
                            Status = false,
                            TransactionId = Guid.Empty,
                            StatusDescription = "Insufficient Balance"
                        };
                    }
                }
                else
                {
                    status = new TransactionStatus
                    {
                        Status = false,
                        TransactionId = Guid.Empty,
                        StatusDescription = Sender == null ? "Invalid Sender" : "Invalid Receiver",
                    };
                }


            }
            catch (Exception ex)
            {
                status = new TransactionStatus
                {
                    Status = false,
                    StatusDescription = ex.Message,
                    TransactionId = Guid.Empty,
                };
            }

            return status;
        }
        #endregion
    }
}
