using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitWork;

    public CompanyController(IUnitOfWork unit)
    {
        _unitWork = unit;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Upsert(int? id)
    {
        Company company = new Company();

        if ((id ?? 0) != 0)
        {
            //Update Company

            company = _unitWork.Company.GetFirstOrDefault(l => l.Id == id);
        }

        return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(Company obj)
    {
        if (ModelState.IsValid)
        {
            if (obj.Id == 0)
            {
                _unitWork.Company.Add(obj);
                TempData["success"] = "Company created successfully.";
            }
            else
            {
                _unitWork.Company.Update(obj);
                TempData["success"] = "Company updated successfully.";
            }

            _unitWork.Save();

            return RedirectToAction("Index");
        }

        return View(obj);
    }

    #region API Calls

    [HttpGet]
    public IActionResult GetAll()
    {
        var companyList = _unitWork.Company.GetAll();

        return Json(new { data = companyList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitWork.Company.GetFirstOrDefault(l => l.Id == id);

        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting." });
        }

        _unitWork.Company.Remove(obj);
        _unitWork.Save();

        return Json(new { success = true, message = "Deleted successfully." });
    }

    #endregion
}
