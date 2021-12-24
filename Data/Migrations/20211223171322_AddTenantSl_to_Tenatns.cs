using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.WebApp.Data.Migrations
{
    public partial class AddTenantSl_to_Tenatns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantSl",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantSl",
                table: "Tenants");
        }
    }
}
