using System.ComponentModel.DataAnnotations;

namespace lab3app.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string UserPassword { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
