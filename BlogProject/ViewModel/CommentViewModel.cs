using BlogProject.Models;

namespace BlogProject.ViewModel
{
    public class CommentViewModel
    {
        public Comment Comment { get; set; }
        public int CurrentUserVote { get; set; }
    }
}
