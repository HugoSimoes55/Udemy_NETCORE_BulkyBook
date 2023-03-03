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

        if ((id ?? 0) != 0)
        {
            //Update Product

            productVM.Product = _unitWork.Product.GetFirstOrDefault(l => l.Id == id);
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

                if (obj.Product.ImageURL != null)
                {
                    var oldImagePath = Path.Combine(wwwroot, obj.Product.ImageURL.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.Product.ImageURL = @"\images\products\" + fileName + extension;
            }

            if (obj.Product.Id == 0)
            {
                _unitWork.Product.Add(obj.Product);
                TempData["success"] = "Product created successfully.";
            }
            else
            {
                _unitWork.Product.Update(obj.Product);
                TempData["success"] = "Product updated successfully.";
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
        var productList = _unitWork.Product.GetAll(
            includeProperties: "Category,CoverType");

        return Json(new { data = productList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitWork.Product.GetFirstOrDefault(l => l.Id == id);

        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting." });
        }

        string wwwroot = _hostEnvironment.WebRootPath;

        if (obj.ImageURL != null)
        {
            var oldImagePath = Path.Combine(wwwroot, obj.ImageURL.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
        }

        _unitWork.Product.Remove(obj);
        _unitWork.Save();

        return Json(new { success = true, message = "Deleted successfully." });
    }

    #endregion
}
