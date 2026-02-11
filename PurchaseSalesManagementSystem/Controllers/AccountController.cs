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

        public AccountController(IWebHostEnvironment env)
        {
            // プロジェクト直下のパスを Repository に渡す
            _repo = new Repository_Login(env);
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
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "ID or PASSWORD wrong!" });
        }
    }
}
