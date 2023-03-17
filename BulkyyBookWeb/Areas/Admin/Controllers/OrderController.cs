using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    [BindProperty]
    public OrderVM orderVM { get; set; }

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Details(int orderId)
    {
        orderVM = new OrderVM()
        {
            OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(l => l.Id == orderId, includeProperties: "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetAll(l => l.OrderId == orderId, includeProperties: "Product")
        };

        return View(orderVM);
    }

    #region API Calls

    [HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> orderHeaders;

        if (User.IsInRole(SD.Role_Admin)
            || User.IsInRole(SD.Role_Employee))
        {
            orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
        }
        else
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            orderHeaders = _unitOfWork.OrderHeader.GetAll(l => l.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
        }

        switch (status)
        {
            case "pending":
                orderHeaders = orderHeaders.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
                break;
            case "inprocess":
                orderHeaders = orderHeaders.Where(x => x.PaymentStatus == SD.StatusInProcess);
                break;
            case "complete":
                orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusShipped);
                break;
            case "approved":
                orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusApproved);
                break;
            default:
                break;
        }

        return Json(new { data = orderHeaders });
    }

    #endregion
}
