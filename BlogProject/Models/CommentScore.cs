namespace BlogProject.Models
{
    public class CommentScore
    {
        public string UserId { get; set; }
        public int CommentId { get; set; }
        public int Vote { get; set; }
    }
}