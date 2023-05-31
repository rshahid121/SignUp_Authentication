using DemoAPI3.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;

namespace DemoAPI3
{
    public class UserService: IUserService
    {
        private string _connectionString = "";
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("Percept");
        }

        public bool SignUp(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "INSERT INTO Users (Username, Password, FirstName, LastName, Device, IPAddress) " +
                             "VALUES (@Username, @Password, @FirstName, @LastName, @Device, @IPAddress)";

                connection.Execute(sql, user);
            }

            return true;

        }
        public string ExtractUserIdFromToken(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);

            // Extract the desired claim from the token
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            return userId;
        }

        public User AuthenticateUser(string username, string password)
        {
            // Authenticate the user using the provided credentials
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";
                var parameters = new { Username = username, Password = password };

                return connection.QueryFirstOrDefault<User>(query, parameters, commandType: CommandType.Text);
            }
        }

        public void SaveLoginDetails(int userId, string ipAddress,string device, string browser, DateTime loginTime, string jWTToken)
        {
            //string connectionString = _configuration.GetConnectionString(_connectionString);

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO LoginDetails (UserId, IPAddress, Device, Browser, LoginTime, JWTToken) VALUES (@UserId, @IPAddress, @Device, @Browser, @LoginTime, @JWTToken)";
                var parameters = new { UserId = userId, IPAddress = ipAddress, Device = device, Browser = browser, LoginTime = loginTime, JWTToken = jWTToken };

                connection.Execute(query, parameters);
            }
        }
        public bool IsFirstSuccessfulLogin(int userID)
        {
            // Check if it's the user's first successful login
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Replace "YourTable" with the appropriate table name and column names in your database
                var query = "SELECT FirstSuccessfulLogin FROM Users WHERE UserID = @UserID";
                var parameters = new { UserID = userID };

                return connection.QuerySingleOrDefault<bool?>(query, parameters, commandType: CommandType.Text) ?? false;
            }
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("JwtSettings:JwtSecret")!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserId", user.UserID.ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


        public void AddGiftBalance(int userID)
        {
            // Add the gift balance to the user's account
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Replace "YourTable" with the appropriate table name and column names in your database
                var query = "UPDATE Users SET Balance = isnull(Balance,0) + @GiftBalance, FirstSuccessfulLogin = 0  WHERE UserID = @userID";
                var parameters = new { UserID = userID, GiftBalance = 5.0m };

                connection.Execute(query, parameters, commandType: CommandType.Text);
            }
        }

        public BalanceResponse GetBalance(BalanceRequest request)
        {
            string userId = ExtractUserIdFromToken(request.Token);            

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query the database to get the user's balance
                string query = "SELECT isnull(Balance,0) as Balance FROM Users WHERE UserId = @UserId";
                decimal balance = connection.ExecuteScalar<decimal>(query, new { UserId = userId });

                return new BalanceResponse { Balance = balance };
            }

        }
    }
}
