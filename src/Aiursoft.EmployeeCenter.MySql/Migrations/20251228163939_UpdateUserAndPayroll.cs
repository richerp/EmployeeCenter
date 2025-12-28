using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.EmployeeCenter.MySql.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAndPayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdministrativeFines",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Payrolls",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Payrolls",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FullAttendance",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HousingFundCompany",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HousingFundPersonal",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "JobSalary",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LateEarly",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaternityCompany",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalCompany",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalPersonal",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherAllowances",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Overtime",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PensionCompany",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PensionPersonal",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PerformanceBonus",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PersonalIncomeTax",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SickLeave",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SpecialAdditionalDeduction",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentCompany",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentPersonal",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkInjuryCompany",
                table: "Payrolls",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "AspNetUsers",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "AspNetUsers",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "AspNetUsers",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "JobLevel",
                table: "AspNetUsers",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdministrativeFines",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "FullAttendance",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "HousingFundCompany",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "HousingFundPersonal",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "JobSalary",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "LateEarly",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "MaternityCompany",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "MedicalCompany",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "MedicalPersonal",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "OtherAllowances",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Overtime",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PensionCompany",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PensionPersonal",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PerformanceBonus",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PersonalIncomeTax",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "SickLeave",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "SpecialAdditionalDeduction",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "UnemploymentCompany",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "UnemploymentPersonal",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "WorkInjuryCompany",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "JobLevel",
                table: "AspNetUsers");
        }
    }
}
