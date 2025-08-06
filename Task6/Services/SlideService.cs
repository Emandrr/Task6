using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Task6.Contracts;
using Task6.Hubs;
using Task6.Models;

namespace Task6.Services
{
    public class SlideService
    {
        private readonly ApplicationDbContext _db;
        private readonly PresentationService _presentationService;

        public SlideService(ApplicationDbContext db, PresentationService presentationService)
        {
            _db = db;
            _presentationService = presentationService;
        }


        public async Task<Slide> GetSlideContent(int slideId)
        {
            return await _db.Slides
                .Include(s => s.TextElements)
                .FirstOrDefaultAsync(s => s.Id == slideId);
        }

        public async Task<(bool success, string message, TextElement textElement)> AddTextElement(int slideId, string userName, string text, int fontSize, int positionX, int positionY)
        {
            var slide = await _db.Slides
                .Include(s => s.Presentation)
                .FirstOrDefaultAsync(s => s.Id == slideId);

            if (slide == null)
            {
                return (false, "Slide not found", null);
            }

            if (!HasEditPermission(slide.Presentation, userName))
            {
                return (false, "No permission to edit", null);
            }

            var textElement = new TextElement
            {
                Text = text,
                FontSize = fontSize,
                PositionX = positionX,
                PositionY = positionY,
                SlideId = slideId
            };

            _db.TextElements.Add(textElement);
            await _db.SaveChangesAsync();

            return (true, null, textElement);
        }

        public async Task<(bool success, string message, TextElement textElement)> GetTextElement(int textElementId)
        {
            var textElement = await _db.TextElements.FirstOrDefaultAsync(te => te.Id == textElementId);

            if (textElement == null)
            {
                return (false, "Text element not found", null);
            }

            return (true, null, textElement);
        }

        public async Task<(bool success, string message, TextElement textElement)> UpdateTextElement(int id, string userName, string text, int fontSize)
        {
            var textElement = await _db.TextElements
                .Include(te => te.Slide)
                .ThenInclude(s => s.Presentation)
                .FirstOrDefaultAsync(te => te.Id == id);

            if (textElement == null)
            {
                return (false, "Text element not found", null);
            }

            if (!HasEditPermission(textElement.Slide.Presentation, userName))
            {
                return (false, "No permission to edit", null);
            }

            textElement.Text = text;
            textElement.FontSize = fontSize;
            await _db.SaveChangesAsync();

            return (true, null, textElement);
        }

        public async Task<(bool success, string message)> UpdateTextElementPosition(int textElementId, string userName, int positionX, int positionY)
        {
            var textElement = await _db.TextElements
                .Include(te => te.Slide)
                .ThenInclude(s => s.Presentation)
                .FirstOrDefaultAsync(te => te.Id == textElementId);

            if (textElement == null)
            {
                return (false, "Text element not found");
            }

            if (!HasEditPermission(textElement.Slide.Presentation, userName))
            {
                return (false, "No permission to edit");
            }

            textElement.PositionX = positionX;
            textElement.PositionY = positionY;
            await _db.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool success, string message, int? textElementId, int? slideId)> DeleteTextElement(int textElementId, string userName)
        {
            var textElement = await _db.TextElements
                .Include(te => te.Slide)
                .ThenInclude(s => s.Presentation)
                .FirstOrDefaultAsync(te => te.Id == textElementId);

            if (textElement == null)
            {
                return (true, null, null, null);
            }

            if (!HasEditPermission(textElement.Slide.Presentation, userName))
            {
                return (false, "No permission to edit", null, null);
            }

            var slideId = textElement.SlideId;
            _db.TextElements.Remove(textElement);
            await _db.SaveChangesAsync();

            return (true, null, textElementId, slideId);
        }

        private bool HasEditPermission(Presentation presentation, string userName)
        {
            return presentation.CreatorName == userName ||
                   (presentation.EditUserNames != null && presentation.EditUserNames.Contains(userName));
        }

        public async Task<SlideDTO> Add(string presentationId)
        {
            var ans = await _presentationService.GetPresentation(Int32.Parse(presentationId));
            if (ans == null) return null;
            return await AddConfirmed(ans);
        }
        public async Task<SlideDTO> AddConfirmed(Presentation presentation)
        {
            Slide slide = Construct(presentation);
            presentation.Slides.Add(slide);
            await _presentationService.SaveToDatabase(presentation);
            return new SlideDTO() { Id = presentation.Slides.Count, Title = slide.Title };
        }
        public Slide Construct(Presentation presentation)
        {
            Slide slide = new Slide();
            slide.Title = "New slide";
            return slide;
        }
        public async Task Delete(string slideId, string presentationId)
        {
            var ans = await _presentationService.GetPresentation(Int32.Parse(presentationId));
            Slide sl = ans.Slides.Where(p => p.Id == Int32.Parse(slideId)).FirstOrDefault();
            var ath = ans.Slides.Remove(sl);
            await _presentationService.SaveToDatabase(ans);
        }
    }
}