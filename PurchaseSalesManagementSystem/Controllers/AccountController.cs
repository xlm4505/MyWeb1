using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;
using System.Text.Json;

namespace PurchaseSalesManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly Repository_Login _repo;
        private readonly IWebHostEnvironment _env;


        public AccountController(IWebHostEnvironment env)
        {
            // プロジェクト直下のパスを Repository に渡す
            _repo = new Repository_Login(env);
            _env = env;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login([FromBody] JsonElement data)
        {
            string userId = data.GetProperty("userId").GetString();
            string password = data.GetProperty("password").GetString();

            if (_repo.Authenticate(userId, password))
            {
                HttpContext.Session.SetString("LoginUser", userId);

                // ★ Environment.txt を読み込む
                var envPath = Path.Combine(_env.ContentRootPath, "Environment.txt");
                var envValue = System.IO.File.Exists(envPath)
                    ? System.IO.File.ReadAllText(envPath).Trim()
                    : "";

                // ★ セッションに保存
                HttpContext.Session.SetString("Environment", envValue);

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "ID or PASSWORD wrong!" });
        }
    }
}
