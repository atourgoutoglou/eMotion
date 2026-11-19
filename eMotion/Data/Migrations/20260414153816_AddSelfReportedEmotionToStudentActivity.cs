using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMotion.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSelfReportedEmotionToStudentActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelfReportedEmotion",
                table: "StudentActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelfReportedEmotion",
                table: "StudentActivities");
        }
    }
}
