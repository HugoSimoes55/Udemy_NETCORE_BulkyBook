using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private ShoppingCartVM cartVM { get; set; }
    private int OrderTotal { get; set; }

    public CartController(IUnitOfWork unit)
    {
        _unitOfWork = unit;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        cartVM = new ShoppingCartVM()
        {
            ListCart = _unitOfWork.ShoppingCart.GetAll(l => l.ApplicationUserId == claim.Value, includeProperties: "Product")
        };

        foreach (var cart in cartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
        }

        return View(cartVM);
    }

    private decimal GetPriceBasedOnQuatity(int quant, decimal price, decimal price50, decimal price100)
    {
        if (quant <= 50)
        {
            return price;
        }
        else if (quant <= 100)
        {
            return price50;
        }
        else
        {
            return price100;
        }
    }
}
