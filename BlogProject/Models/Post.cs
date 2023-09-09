using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class Post
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
        public List<Comment> Comments { get; set; }
        [Required]
        public string Titre { get; set; }
        [Required]
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Score { get; set; }
    }
}
