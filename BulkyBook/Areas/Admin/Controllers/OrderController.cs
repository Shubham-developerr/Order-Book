using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModel;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        private readonly IUnitOfWork _db;
		public OrderController(IUnitOfWork db)
		{
			_db = db;
		}
		public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details(int orderId)
        {
            OrderVM = new()
			{
				OrderHeader = _db.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetail = _db.OrderDetail.GetAll(u => u.Id == orderId, includeProperties: "Product")
			};
			return View(OrderVM);
        }
		[ActionName("Details")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Details_PAY_NOW(int orderId)
		{
			OrderVM.OrderHeader = _db.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			OrderVM.OrderDetail = _db.OrderDetail.GetAll(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "Product");
			
				var domain = "https://localhost:44320/";
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					PaymentMethodTypes = new List<string>
				{

					"card",
				},
					LineItems = new List<SessionLineItemOptions>()
					,
					Mode = "payment",
					SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.OrderHeader.Id}",
					CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
				};
				foreach (var item in OrderVM.OrderDetail)
				{

					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),
							Currency = "inr",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title,
							},

						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);
				}
				var service = new Stripe.Checkout.SessionService();
				Session session = service.Create(options);
				_db.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_db.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			
		}
		public IActionResult PaymentConfirmation(int orderHeaderid)
		{
			OrderHeader orderHeader = _db.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);
			if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				var service = new Stripe.Checkout.SessionService();
				Stripe.Checkout.Session session = service.Get(orderHeader.SessionId);
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_db.OrderHeader.UpdateStripePaymentID(orderHeaderid, orderHeader.SessionId, session.PaymentIntentId);
					_db.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
					_db.Save();
				}
			}
			return View(orderHeaderid);
			
		}

		[HttpPost]
		[Authorize(Roles =SD.Role_Admin + ","+ SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateOrderDetail()
		{
			var orderHeaderFromDb = _db.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id,tracked:false);
			orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
			orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
			orderHeaderFromDb.City = OrderVM.OrderHeader.City;
			orderHeaderFromDb.State = OrderVM.OrderHeader.State;
			orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
			if(OrderVM.OrderHeader.Carrier != null)
			{
				orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;

			}
			if (OrderVM.OrderHeader.TrackingNumber != null)
			{
				orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

			}
			//AsNoTracking means EF will not automatically saves the value in Table automatically but we can set values and later
			//update the value with KeyWord Update 

			_db.OrderHeader.Update(orderHeaderFromDb);
			_db.Save();
			TempData["success"] = "Order Details Updated";
			return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });

		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult CancelOrder()
		{
			var orderHeader = _db.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
			
			if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeader.PaymentIntentId,

				};
				var service = new RefundService();
				Refund refund = service.Create(options);
				_db.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_db.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
			}
			//AsNoTracking means EF will not automatically saves the value in Table automatically but we can set values and later
			//update the value with KeyWord Update 
			_db.Save();
			TempData["success"] = "Order Cancelled Successfully";
			return RedirectToAction("Details", "Order", new { orderId = orderHeader.Id });

		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult StartProcessing()
		{
			_db.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
			_db.Save();
			TempData["success"] = "Order Status Updated Successfully";
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
			//AsNoTracking means EF will not automatically saves the value in Table automatically but we can set values and later
			//update the value with KeyWord Update 
			
		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		[ValidateAntiForgeryToken]
		public IActionResult ShipOrder()
		{
			var orderHeader = _db.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
			orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
			orderHeader.OrderStatus = SD.StatusShipped;
			orderHeader.ShippingDate = DateTime.Now;
			if(orderHeader.PaymentStatus== SD.PaymentStatusDelayedPayment)
			{
				orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
			}
			_db.OrderHeader.Update(orderHeader);
			_db.Save();
			TempData["success"] = "Order Status Updated Successfully";
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
			//AsNoTracking means EF will not automatically saves the value in Table automatically but we can set values and later
			//update the value with KeyWord Update 

		}
		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders;
			if(User.IsInRole(SD.Role_Admin )||User.IsInRole( SD.Role_Employee))
			{
                orderHeaders = _db.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
			else
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _db.OrderHeader.GetAll(u=>u.ApplicationUserId==claims.Value,includeProperties: "ApplicationUser");
            }

			switch(status)
			{
				case "pending":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.PaymentStatusPending || u.OrderStatus == SD.PaymentStatusDelayedPayment);
					break;
				case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
					break;
				case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
					break;
				case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
				default:
					break;
            }
            return Json(new { data = orderHeaders });
		}
		#endregion
	}
}
