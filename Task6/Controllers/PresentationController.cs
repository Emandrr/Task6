using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Task6.Models;
using Task6.Services;
namespace Task6.Controllers
{
    public class PresentationController : Controller
    {
        private PresentationService _presentationService;

        public PresentationController(PresentationService presentationService)
        {
            _presentationService = presentationService;
        }

        public async Task<IActionResult> Index(int id)
        {
            
            var ans = await _presentationService.GetPresentation(id);

            var slides = ans.Slides;
            if (slides == null || !slides.Any())
            {
                return NotFound("Презентация не найдена или не содержит слайдов");
            }

            return View(slides);
        }
    }
}
