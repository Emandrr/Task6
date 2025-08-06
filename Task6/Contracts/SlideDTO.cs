using Task6.Models;

namespace Task6.Contracts
{
    public class SlideDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<TextElementDTO> TextElements { get; set; } = new List<TextElementDTO>();
    }
}
