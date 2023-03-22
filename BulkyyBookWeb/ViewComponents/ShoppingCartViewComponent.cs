using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents;

public class ShoppingCartViewComponent : ViewComponent
{
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartViewComponent(IUnitOfWork unit)
    {
        _unitOfWork = unit;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            HttpContext.Session.Clear();

            return View(0);
        }

        if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
        {
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(l => l.ApplicationUserId == claim.Value).Count());
        }

        return View(HttpContext.Session.GetInt32(SD.SessionCart));
    }
}
