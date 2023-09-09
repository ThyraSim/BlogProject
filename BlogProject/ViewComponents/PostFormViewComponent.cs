namespace BlogProject.ViewComponents
{
    using Microsoft.AspNetCore.Mvc;
    using BlogProject.Models;

    public class PostFormViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View(new Post());
        }
    }

}
