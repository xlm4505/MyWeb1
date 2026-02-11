using System.IO;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_Login
    {
        private readonly string _authFilePath;

        public Repository_Login(IWebHostEnvironment env)
        {
            _authFilePath = Path.Combine(env.ContentRootPath, "user.txt");
        }

        public bool Authenticate(string userId, string password)
        {
            if (!File.Exists(_authFilePath))
                return false;

            var lines = File.ReadAllLines(_authFilePath);

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length == 2 && parts[0] == userId && parts[1] == password)
                    return true;
            }

            return false;
        }
    }
}
