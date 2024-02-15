using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileUpload.Migrations
{
    /// <inheritdoc />
    public partial class MorFileUpoadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UploadedFileInfos",
                table: "UploadedFileInfos");

            migrationBuilder.RenameTable(
                name: "UploadedFileInfos",
                newName: "UploadedFileInfo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UploadedFileInfo",
                table: "UploadedFileInfo",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UploadedFileInfo",
                table: "UploadedFileInfo");

            migrationBuilder.RenameTable(
                name: "UploadedFileInfo",
                newName: "UploadedFileInfos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UploadedFileInfos",
                table: "UploadedFileInfos",
                column: "Id");
        }
    }
}
