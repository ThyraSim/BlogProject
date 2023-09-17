namespace BlogProject.Models
{
    public class CommunityPost
    {
        public int CommunityId { get; set; }
        public Community Community { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}