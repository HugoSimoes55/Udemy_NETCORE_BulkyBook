using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public ShoppingCartVM cartVM { get; set; }
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
            ListCart = _unitOfWork.ShoppingCart.GetAll(l => l.ApplicationUserId == claim.Value, includeProperties: "Product"),
            OrderHeader = new()
        };

        foreach (var cart in cartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            cartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        return View(cartVM);
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        cartVM = new ShoppingCartVM()
        {
            ListCart = _unitOfWork.ShoppingCart.GetAll(l => l.ApplicationUserId == claim.Value, includeProperties: "Product"),
            OrderHeader = new()
        };

        cartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(l => l.Id == claim.Value);
        cartVM.OrderHeader.Name = cartVM.OrderHeader.ApplicationUser.Name;
        cartVM.OrderHeader.PhoneNumber = cartVM.OrderHeader.ApplicationUser.PhoneNumber;
        cartVM.OrderHeader.StreetAddress = cartVM.OrderHeader.ApplicationUser.StreetAddress;
        cartVM.OrderHeader.City = cartVM.OrderHeader.ApplicationUser.City;
        cartVM.OrderHeader.State = cartVM.OrderHeader.ApplicationUser.State;
        cartVM.OrderHeader.PostalCode = cartVM.OrderHeader.ApplicationUser.PostalCode;

        foreach (var cart in cartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            cartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        return View(cartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult SummaryPOST()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        cartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(l => l.ApplicationUserId == claim.Value, includeProperties: "Product");

        cartVM.OrderHeader.OrderDate = DateTime.Now;
        cartVM.OrderHeader.ApplicationUserId = claim.Value;


        foreach (var cart in cartVM.ListCart)
        {
            cart.Price = GetPriceBasedOnQuatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            cartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        ApplicationUser appUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(l => l.Id == claim.Value);

        if (appUser.CompanyId.GetValueOrDefault() == 0)
        {
            cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            cartVM.OrderHeader.OrderStatus = SD.StatusPending;
        }
        else
        {
            cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            cartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        }

        _unitOfWork.OrderHeader.Add(cartVM.OrderHeader);
        _unitOfWork.Save();

        foreach (var cart in cartVM.ListCart)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = cart.ProductId,
                OrderId = cartVM.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count
            };

            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
        }


        if (appUser.CompanyId.GetValueOrDefault() == 0)
        {
            //////////
            // Stripe code would be added here - Find by looking up "Checkout" in Stripe Documentation
            //////////


            bool successfulTransaction = true;

            if (successfulTransaction)
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentIds(cartVM.OrderHeader.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

                return RedirectToAction("OrderConfirmation", "Cart", new { orderHeaderId = cartVM.OrderHeader.Id });
            }
        }
        else
        {
            return RedirectToAction("OrderConfirmation", "Cart", new { orderHeaderId = cartVM.OrderHeader.Id });
        }

        return RedirectToAction("Index");
    }

    public IActionResult OrderConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(l => l.Id == orderHeaderId);

        if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
        {
            //Add Stripe Get Session Code
            bool paymentPaid = true;

            if (paymentPaid)
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(l => l.ApplicationUserId == orderHeader.ApplicationUserId).ToList();


        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        _unitOfWork.Save();

        return View(orderHeaderId);
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

    public IActionResult Plus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(l => l.Id == cartId);

        _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(l => l.Id == cartId);

        if (cart.Count <= 1)
        {
            _unitOfWork.ShoppingCart.Remove(cart);
        }
        else
        {
            _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
        }

        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Remove(int cartId)
    {
        var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(l => l.Id == cartId);

        _unitOfWork.ShoppingCart.Remove(cart);
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }
}
