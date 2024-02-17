using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileUpload.Migrations
{
    /// <inheritdoc />
    public partial class TotalSuccesssful : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalFailed",
                table: "TeamaptUploadedFilesInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalSuccessful",
                table: "TeamaptUploadedFilesInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalFailed",
                table: "TeamaptUploadedFilesInfo");

            migrationBuilder.DropColumn(
                name: "TotalSuccessful",
                table: "TeamaptUploadedFilesInfo");
        }
    }
}
