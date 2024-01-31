using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Microsoft.AspNetCore.Authorization;

namespace TravelBooking.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateCheckoutSession(string amount)
        {
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = Convert.ToInt32(amount) * 100,
                            Currency = "CAD",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Flight",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:5299/Home/success",
                CancelUrl = "http://localhost:5299/Home/cancel",
            };
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            //return new HttpStatusCodeResult(303);

        }
        [HttpPost]
        public ActionResult GetPayment()
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = 1099,
                SetupFutureUsage = "off_session",
                Currency = "CAD",
            };
            var paymentIntent = service.Create(options);
            return Json(paymentIntent);
        }

        public ActionResult success()
        {
            return View();
        }

        public ActionResult cancel()
        {
            return View();
        }
    }
}
