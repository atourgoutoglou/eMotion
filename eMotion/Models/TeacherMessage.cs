using System.ComponentModel.DataAnnotations;

namespace eMotion.Models
{
    public class TeacherMessage
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = "";

        [Required]
        public string TeacherId { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}