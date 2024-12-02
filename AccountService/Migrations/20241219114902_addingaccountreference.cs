using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Migrations
{
    /// <inheritdoc />
    public partial class addingaccountreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionForAccountId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionForAccountId",
                table: "Transactions",
                column: "TransactionForAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_TransactionForAccountId",
                table: "Transactions",
                column: "TransactionForAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountNumber",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_TransactionForAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionForAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionForAccountId",
                table: "Transactions");
        }
    }
}
