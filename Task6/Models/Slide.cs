namespace Task6.Models
{
    public class Slide
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Presentation Presentation { get; set; }
        public int PresentationId { get; set; }
        public List<TextElement> TextElements { get; set; } = new List<TextElement>();
    }
}
