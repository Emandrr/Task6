using System.ComponentModel.DataAnnotations;

namespace Task6.Models
{
    public class Presentation
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        [Required]
        public string CreatorName { get; set; } = string.Empty;
        public List<string> EditUserNames { get; set; } = new List<string>();
        public List<Slide> Slides { get; set; } = new List<Slide>();
    }
}
