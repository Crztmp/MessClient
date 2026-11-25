namespace ClientApp01.Models
{
    public class AuthResponse
    {
        public bool Success => !string.IsNullOrEmpty(Token);
        public string Token { get; set; }
        public UserInfo User { get; set; }
        public string Error { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}