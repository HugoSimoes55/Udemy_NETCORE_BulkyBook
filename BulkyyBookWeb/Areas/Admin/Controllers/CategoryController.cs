using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitWork;

    public CategoryController(IUnitOfWork unit)
    {
        _unitWork = unit;
    }

    public IActionResult Index()
    {
        List<Category> objCategoryList = _unitWork.Category.GetAll().ToList();

        return View(objCategoryList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category obj)
    {
        if (obj.Name == obj.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Name", "The DisplayOrder cannot match the Name.");
        }

        if (ModelState.IsValid)
        {
            _unitWork.Category.Add(obj);
            _unitWork.Save();

            TempData["success"] = "Category was created successfully.";

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

        var category = _unitWork.Category.GetFirstOrDefault(l => l.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category obj)
    {
        if (obj.Name == obj.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Name", "The DisplayOrder cannot match the Name.");
        }

        if (ModelState.IsValid)
        {
            _unitWork.Category.Update(obj);
            _unitWork.Save();

            TempData["success"] = "Category was updated successfully.";

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

        var category = _unitWork.Category.GetFirstOrDefault(l => l.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Category obj)
    {
        _unitWork.Category.Remove(obj);
        _unitWork.Save();

        TempData["success"] = "Category was deleted successfully.";

        return RedirectToAction("Index");
    }
}
