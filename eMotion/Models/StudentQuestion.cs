using System;
using System.ComponentModel.DataAnnotations;

namespace eMotion.Models
{
    public class StudentQuestion
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }

        [Required]
        public string StudentName { get; set; }

        [Required(ErrorMessage = "Επίλεξε κεφάλαιο.")]
        public string Chapter { get; set; }

        [Required(ErrorMessage = "Γράψε την απορία σου.")]
        [StringLength(1000)]
        public string Message { get; set; }

        public string? TeacherReply { get; set; }

        public bool IsAnswered { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? AnsweredAt { get; set; }
    }
}