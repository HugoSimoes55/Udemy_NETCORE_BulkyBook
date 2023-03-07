using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class CoverTypeController : Controller
{
    private readonly IUnitOfWork _unitWork;

    public CoverTypeController(IUnitOfWork unit)
    {
        _unitWork = unit;
    }

    public IActionResult Index()
    {
        List<CoverType> objList = _unitWork.CoverType.GetAll().ToList();

        return View(objList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CoverType obj)
    {
        if (ModelState.IsValid)
        {
            _unitWork.CoverType.Add(obj);
            _unitWork.Save();

            TempData["success"] = "Cover Type was created successfully.";

            return RedirectToAction("Index");
        }

        return View(obj);
    }

    public IActionResult Edit(int? id)
    {
        if ((id ?? 0) == 0)
        {
            return NotFound();
        }

        var coverType = _unitWork.CoverType.GetFirstOrDefault(l => l.Id == id);

        if (coverType == null)
        {
            return NotFound();
        }

        return View(coverType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(CoverType obj)
    {
        if (ModelState.IsValid)
        {
            _unitWork.CoverType.Update(obj);
            _unitWork.Save();

            TempData["success"] = "Cover Type was updated successfully.";

            return RedirectToAction("Index");
        }

        return View(obj);
    }

    public IActionResult Delete(int? id)
    {
        if ((id ?? 0) == 0)
        {
            return NotFound();
        }

        var coverType = _unitWork.CoverType.GetFirstOrDefault(l => l.Id == id);

        if (coverType == null)
        {
            return NotFound();
        }

        return View(coverType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(CoverType obj)
    {
        _unitWork.CoverType.Remove(obj);
        _unitWork.Save();

        TempData["success"] = "Cover Type was deleted successfully.";

        return RedirectToAction("Index");
    }
}
