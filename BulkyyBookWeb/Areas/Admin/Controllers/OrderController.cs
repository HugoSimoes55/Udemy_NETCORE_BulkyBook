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

    [ActionName("Details")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Details_PayNow()
    {
        orderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(l => l.Id == orderVM.OrderHeader.Id, tracked: false);
        orderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(l => l.OrderId == orderVM.OrderHeader.Id, includeProperties: "Product");

        // Add Stripe payment code

        return RedirectToAction("PaymentConfirmation", new { orderHeaderId = orderVM.OrderHeader.Id });
    }

    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(l => l.Id == orderHeaderId);

        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            //Add Stripe Get Session Code
            bool paymentPaid = true;

            if (paymentPaid)
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        return View(orderHeaderId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateOrderDetail()
    {
        var orderHeaderFromDB = _unitOfWork.OrderHeader.GetFirstOrDefault(l => l.Id == orderVM.OrderHeader.Id, tracked: false);

        orderHeaderFromDB.Name = orderVM.OrderHeader.Name;
        orderHeaderFromDB.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
        orderHeaderFromDB.StreetAddress = orderVM.OrderHeader.StreetAddress;
        orderHeaderFromDB.City = orderVM.OrderHeader.City;
        orderHeaderFromDB.State = orderVM.OrderHeader.State;
        orderHeaderFromDB.PostalCode = orderVM.OrderHeader.PostalCode;

        if (orderVM.OrderHeader.Carrier != null)
        {
            orderHeaderFromDB.Carrier = orderVM.OrderHeader.Carrier;
        }

        if (orderVM.OrderHeader.TrackingNumber != null)
        {
            orderHeaderFromDB.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
        }

        _unitOfWork.OrderHeader.Update(orderHeaderFromDB);
        _unitOfWork.Save();

        TempData["success"] = "Order Details were updated successfully";

        return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDB.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
        _unitOfWork.Save();

        TempData["success"] = "Order status was updated successfully";

        return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ShipOrder()
    {
        var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(l => l.Id == orderVM.OrderHeader.Id, tracked: false);

        orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
        orderHeader.Carrier = orderVM.OrderHeader.Carrier;
        orderHeader.OrderStatus = SD.StatusShipped;
        orderHeader.ShippingDate = DateTime.Now;

        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
        }

        _unitOfWork.OrderHeader.Update(orderHeader);
        _unitOfWork.Save();

        TempData["success"] = "Order shipped successfully";

        return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult CancelOrder()
    {
        var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(l => l.Id == orderVM.OrderHeader.Id, tracked: false);

        if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
        {
            // Stripe code for refund

            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
        }

        _unitOfWork.Save();

        TempData["success"] = "Order cancelled successfully";

        return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
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
