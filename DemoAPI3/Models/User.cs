namespace DemoAPI3.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Device { get; set; }
        public string IPAddress { get; set; }
        public bool FirstSuccessfulLogin { get; set; }
        public decimal Balance { get; set; }
    }
}
