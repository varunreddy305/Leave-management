using Microsoft.EntityFrameworkCore.Migrations;

namespace Leave_management.Data.Migrations
{
    public partial class AddedNewField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "LeaveRequests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "LeaveRequests");
        }
    }
}
