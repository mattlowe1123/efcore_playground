using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MattsShop.Context.Migrations
{
    public partial class largefield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReallyLargeField",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReallyLargeField",
                table: "Customers");
        }
    }
}
