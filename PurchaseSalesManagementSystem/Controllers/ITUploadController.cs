using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Data;
using System.Net;
using System.Net.Sockets;

public class ITUploadController : Controller
{

    private readonly Repository_ITUpload _repo;
    public ITUploadController(Repository_ITUpload repo)
    {
        _repo = repo;
    }

    public IActionResult ITUpload()
    {
        return View();
    }

    [HttpPost]
    public IActionResult UploadExcel(IFormFile excelFile)
    {
        try
        {
            // IPアドレスを取得（クライアントIP取得）
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "";

            var userName = HttpContext.Session.GetString("LoginUser") ?? "";

            List<string> msg;
            int insertCount = 0;
            (insertCount, msg) = _repo.Upload(excelFile, userName, ipAddress);

            return Json(new
            {
                errorList = msg,
                successCount = insertCount
            });

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
