using DemoAPI3.Models;

namespace DemoAPI3
{
    public interface IUserService
    {
        bool SignUp(User user);
        string ExtractUserIdFromToken(string token);
        User AuthenticateUser(string username, string password);
        bool IsFirstSuccessfulLogin(int userID);
        string GenerateJwtToken(User user);
        BalanceResponse GetBalance(BalanceRequest request);
        void AddGiftBalance(int userID);
        void SaveLoginDetails(int userId, string ipAddress, string device, string browser, DateTime loginTime, string jWTToken);
    }
}
