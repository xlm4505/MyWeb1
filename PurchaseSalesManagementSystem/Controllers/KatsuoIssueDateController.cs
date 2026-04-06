using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;

public class KatsuoIssueDateController : Controller
{
    private readonly Repository_KatsuoIssueDate _repo;

    public KatsuoIssueDateController(Repository_KatsuoIssueDate repo)
    {
        _repo = repo;
    }

    public IActionResult KatsuoIssueDate()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetData(string? userName)
    {
        var data = _repo.GetKatsuoIssueDateData(userName);
        return Json(data);
    }
}
