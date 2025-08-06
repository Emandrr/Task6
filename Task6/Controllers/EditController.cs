using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Task6.Services;
using Task6.Contracts;
using Task6.Models;
namespace Task6.Controllers
{
    public class EditController : Controller
    {
        private PresentationService _presentationService;
        private SlideService _slideService;

        public EditController(PresentationService presentationService, SlideService slideService)
        {
            _presentationService = presentationService;
            _slideService = slideService;

        }
        public async Task<IActionResult> Index(int id)
        {
            var ans = await _presentationService.GetPresentation(id);
            return View(await _presentationService.GetPresentation(id));
        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string presentationId, string userName, string newRole, string Role)
        {
            bool res = await _presentationService.ChangePresentatiomUserRole(presentationId, userName, newRole, Role);
            if (!res) return Json(new { success = false, message = "Invalid result" });
            return Json(new { success = true, message = "User role updated successfully" });

        }
        [HttpGet("Edit/Presentation/{presentationId}")]
        public async Task<IActionResult> Presentation(int presentationId)
        {
            var ans = await _presentationService.GetPresentation(presentationId);
            return RedirectToAction("Index", "Presentation", new {id = presentationId });
        }
        [HttpPost]
        public async Task<IActionResult> AddSlide(string presentationId)
        {
            SlideDTO ans = await _slideService.Add(presentationId);
            if(ans==null) return Json(new { success = false, message = "Invalid result" });
            else return Json(new { success = true, slide = ans });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSlide(string slideId,string presentationId)
        {
            await _slideService.Delete(slideId,presentationId);
            
            return Json(new { success = true});
        }

        [HttpGet]
        public async Task<IActionResult> GetSlideContent(int slideId)
        {
            try
            {
                var slide = await _slideService.GetSlideContent(slideId);

                if (slide == null)
                {
                    return Json(new { success = false, message = "Slide not found" });
                }

                return Json(new
                {
                    success = true,
                    slide = new
                    {
                        id = slide.Id,
                        title = slide.Title,
                        textElements = slide.TextElements.Select(te => new
                        {
                            id = te.Id,
                            text = te.Text,
                            fontSize = te.FontSize,
                            positionX = te.PositionX,
                            positionY = te.PositionY
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTextElement(int slideId, string text, int fontSize, int positionX, int positionY)
        {
            try
            {
                var userName = Request.Headers["X-User-Name"].FirstOrDefault();
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var (success, message, textElement) = await _slideService.AddTextElement(slideId, userName, text, fontSize, positionX, positionY);

                if (!success)
                {
                    return Json(new { success = false, message });
                }

                return Json(new
                {
                    success = true,
                    textElement = new
                    {
                        id = textElement.Id,
                        text = textElement.Text,
                        fontSize = textElement.FontSize,
                        positionX = textElement.PositionX,
                        positionY = textElement.PositionY,
                        slideId = textElement.SlideId
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTextElement(int textElementId)
        {
            try
            {
                var (success, message, textElement) = await _slideService.GetTextElement(textElementId);

                if (!success)
                {
                    return Json(new { success = false, message });
                }

                return Json(new
                {
                    success = true,
                    textElement = new
                    {
                        id = textElement.Id,
                        text = textElement.Text,
                        fontSize = textElement.FontSize,
                        positionX = textElement.PositionX,
                        positionY = textElement.PositionY
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTextElement(int id, string text, int fontSize)
        {
            try
            {
                var userName = Request.Headers["X-User-Name"].FirstOrDefault();
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var (success, message, textElement) = await _slideService.UpdateTextElement(id, userName, text, fontSize);

                if (!success)
                {
                    return Json(new { success = false, message });
                }

                return Json(new
                {
                    success = true,
                    textElement = new
                    {
                        id = textElement.Id,
                        text = textElement.Text,
                        fontSize = textElement.FontSize,
                        positionX = textElement.PositionX,
                        positionY = textElement.PositionY,
                        slideId = textElement.SlideId
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTextElementPosition(int textElementId, int positionX, int positionY)
        {
            try
            {
                var userName = Request.Headers["X-User-Name"].FirstOrDefault();
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var (success, message) = await _slideService.UpdateTextElementPosition(textElementId, userName, positionX, positionY);

                if (!success)
                {
                    return Json(new { success = false, message });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTextElement(int textElementId, int slideId)
        {
            try
            {
                var userName = Request.Headers["X-User-Name"].FirstOrDefault();
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var (success, message, deletedTextElementId, deletedSlideId) = await _slideService.DeleteTextElement(textElementId, userName);

                if (!success)
                {
                    return Json(new { success = false, message });
                }

                return Json(new { success = true, textElement = new { id = deletedTextElementId, slideId = deletedSlideId } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
