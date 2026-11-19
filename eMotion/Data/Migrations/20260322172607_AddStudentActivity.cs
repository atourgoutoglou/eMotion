using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eMotion.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChapterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    WrongAnswers = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    TheoryTimeSeconds = table.Column<int>(type: "int", nullable: false),
                    QuizTimeSeconds = table.Column<int>(type: "int", nullable: false),
                    AnswerChanges = table.Column<int>(type: "int", nullable: false),
                    MaterialClicks = table.Column<int>(type: "int", nullable: false),
                    IdleTimeSeconds = table.Column<int>(type: "int", nullable: false),
                    EmotionState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentActivities", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentActivities");
        }
    }
}
