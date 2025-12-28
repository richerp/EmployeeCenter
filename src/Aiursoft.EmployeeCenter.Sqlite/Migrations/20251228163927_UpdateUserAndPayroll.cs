using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.EmployeeCenter.Sqlite.Migrations
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
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Payrolls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Payrolls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FullAttendance",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HousingFundCompany",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HousingFundPersonal",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "JobSalary",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LateEarly",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaternityCompany",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalCompany",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalPersonal",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherAllowances",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Overtime",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PensionCompany",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PensionPersonal",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PerformanceBonus",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PersonalIncomeTax",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SickLeave",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SpecialAdditionalDeduction",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentCompany",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnemploymentPersonal",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkInjuryCompany",
                table: "Payrolls",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "JobLevel",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 30,
                nullable: true);
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
