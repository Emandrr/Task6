namespace Task6.Models
{
    public class ConnectedUser
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsOnline => true; 
    }
}
