using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

        return View(productList);
    }

    public IActionResult Details(int productId)
    {
        ShoppingCart shopCart = new ShoppingCart()
        {
            ProductId = productId,
            Product = _unitOfWork.Product.GetFirstOrDefault(l => l.Id == productId, includeProperties: "Category,CoverType"),
            Count = 1
        };

        return View(shopCart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShoppingCart shopCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.GetFirstOrDefault(l => l.ApplicationUserId == claim.Value && l.ProductId == shopCart.ProductId);

        shopCart.ApplicationUserId = claim.Value;

        if (cartFromDB == null)
        {
            _unitOfWork.ShoppingCart.Add(shopCart);
        }
        else
        {
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDB, shopCart.Count);
        }

        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}