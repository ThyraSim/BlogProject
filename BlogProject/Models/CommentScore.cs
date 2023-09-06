namespace BlogProject.Models
{
    public class CommentScore
    {
        public int UserId { get; set; }
        public int CommentId { get; set; }
        public int Vote { get; set; }
    }
}