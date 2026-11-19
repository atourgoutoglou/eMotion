using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMotion.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityTypeToStudentActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivityType",
                table: "StudentActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityType",
                table: "StudentActivities");
        }
    }
}
