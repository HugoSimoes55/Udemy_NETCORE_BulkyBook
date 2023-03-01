using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers;

public class ProductController : Controller
{
    private readonly IUnitOfWork _unitWork;

    public ProductController(IUnitOfWork unit)
    {
        _unitWork = unit;
    }

    public IActionResult Index()
    {
        List<Product> objList = _unitWork.Product.GetAll().ToList();

        return View(objList);
    }

    public IActionResult Upsert(int? id)
    {
        ProductVM productVM = new ProductVM()
        {
            Product = new Product(),
            CategoryList = _unitWork.Category.GetAll().Select(
                l => new SelectListItem
                {
                    Text = l.Name,
                    Value = l.Id.ToString()
                }),
            CoverTypeList = _unitWork.CoverType.GetAll().Select(
                l => new SelectListItem
                {
                    Text = l.Name,
                    Value = l.Id.ToString()
                })
        };

        if ((id ?? 0) == 0)
        {
            // Create Product

            return View(productVM);
        }
        else
        {
            //Update Product

        }

        return View(productVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(Product obj)
    {
        if (ModelState.IsValid)
        {
            _unitWork.Product.Update(obj);
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

        var product = _unitWork.Product.GetFirstOrDefault(l => l.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Product obj)
    {
        _unitWork.Product.Remove(obj);
        _unitWork.Save();

        TempData["success"] = "Cover Type was deleted successfully.";

        return RedirectToAction("Index");
    }
}
