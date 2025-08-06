namespace Task6.Contracts
{
    public class TextElementDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int FontSize { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int SlideId { get; set; }
    }
}
