using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargeBackAuthApi.Migrations
{
    /// <inheritdoc />
    public partial class MerchantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "AspNetUsers");
        }
    }
}
