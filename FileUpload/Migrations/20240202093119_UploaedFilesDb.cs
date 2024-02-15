using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileUpload.Migrations
{
    /// <inheritdoc />
    public partial class UploaedFilesDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalFailed",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "TotalSuccessful",
                table: "UploadedFiles");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "UploadedFiles",
                newName: "TerminalId");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "UploadedFiles",
                newName: "TransactionDate");

            migrationBuilder.AddColumn<string>(
                name: "AccountToBeCredited",
                table: "UploadedFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "UploadedFiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MaskedPan",
                table: "UploadedFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rrn",
                table: "UploadedFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Stan",
                table: "UploadedFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountToBeCredited",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "MaskedPan",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "Rrn",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "Stan",
                table: "UploadedFiles");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "UploadedFiles",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "TerminalId",
                table: "UploadedFiles",
                newName: "FileName");

            migrationBuilder.AddColumn<int>(
                name: "TotalFailed",
                table: "UploadedFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalSuccessful",
                table: "UploadedFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
