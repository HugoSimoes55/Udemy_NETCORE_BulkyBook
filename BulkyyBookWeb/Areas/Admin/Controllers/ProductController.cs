using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers;

public class ProductController : Controller
{
    private readonly IUnitOfWork _unitWork;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProductController(IUnitOfWork unit, IWebHostEnvironment hostEnvironment)
    {
        _unitWork = unit;
        _hostEnvironment = hostEnvironment;
    }

    public IActionResult Index()
    {
        return View();
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
    public IActionResult Upsert(ProductVM obj, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwroot = _hostEnvironment.WebRootPath;

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(file.FileName);
                var uploads = Path.Combine(wwwroot, @"images\products");
                var extension = Path.GetExtension(file.FileName);

                using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.Product.ImageURL = @"\images\products\" + fileName + extension;
            }

            _unitWork.Product.Add(obj.Product);
            _unitWork.Save();

            TempData["success"] = "Product created successfully.";

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

    #region API Calls

    [HttpGet]
    public IActionResult GetAll()
    {
        var productList = _unitWork.Product.GetAll(
            includeProperties: "Category,CoverType");

        return Json(new { data = productList });
    }

    #endregion
}
