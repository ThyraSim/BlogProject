namespace BlogProject.Models
{
    public class Community
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();
    }
}
