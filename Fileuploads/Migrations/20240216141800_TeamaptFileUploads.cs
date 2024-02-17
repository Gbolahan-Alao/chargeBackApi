using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileUpload.Migrations
{
    /// <inheritdoc />
    public partial class TeamaptFileUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedFileInfo");

            migrationBuilder.CreateTable(
                name: "FairmoneyUploadedFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaskedPan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rrn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerminalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountToBeCredited = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FairmoneyUploadedFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PalmpayUploadedFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaskedPan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rrn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerminalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountToBeCredited = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalmpayUploadedFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamaptUploadedFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaskedPan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rrn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerminalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountToBeCredited = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamaptUploadedFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamaptUploadedFilesInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamaptUploadedFilesInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFileInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFileInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FairmoneyUploadedFile");

            migrationBuilder.DropTable(
                name: "PalmpayUploadedFile");

            migrationBuilder.DropTable(
                name: "TeamaptUploadedFile");

            migrationBuilder.DropTable(
                name: "TeamaptUploadedFilesInfo");

            migrationBuilder.DropTable(
                name: "UploadedFileInfos");

            migrationBuilder.CreateTable(
                name: "UploadedFileInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFileInfo", x => x.Id);
                });
        }
    }
}
