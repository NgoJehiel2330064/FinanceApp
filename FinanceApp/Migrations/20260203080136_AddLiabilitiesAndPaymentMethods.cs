using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLiabilitiesAndPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceAssetId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceLiabilityId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Liabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    InterestRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    MonthlyPayment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaturityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liabilities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PaymentMethod",
                table: "Transactions",
                column: "PaymentMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceAssetId",
                table: "Transactions",
                column: "SourceAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceLiabilityId",
                table: "Transactions",
                column: "SourceLiabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Liabilities_Type",
                table: "Liabilities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Liabilities_UserId",
                table: "Liabilities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Liabilities");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PaymentMethod",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SourceAssetId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SourceLiabilityId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SourceAssetId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SourceLiabilityId",
                table: "Transactions");
        }
    }
}
