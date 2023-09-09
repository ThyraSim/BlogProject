namespace BlogProject.Models
{
    public class PostScore
    {
        public string UserId { get; set; }
        public int PostId { get; set; }
        public int Vote { get; set; }
    }
}