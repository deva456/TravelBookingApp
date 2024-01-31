using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using TravelBooking.Helper;
using TravelBooking.Models;
using Stripe;
using TravelBooking.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TravelBooking.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly TravelBookingContext _context;
        private readonly ILogger<CheckoutController> logger;
        [TempData]
        public string Amount { get; set; }

        public List<PurchasedFlight> PurchasedList;
        public List<PlannedFlight> PlannedList;

        public CheckoutController(TravelBookingContext context, ILogger<CheckoutController> logger)
        {
            this._context = context;
            this.logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            // var cart = WorkingWithSession.GetObjectFromJson<List<Flight>>(HttpContext.Session, "cart");
            // ViewBag.cart = cart;
            // ViewBag.DollarAmount = cart.Sum(item => item.Price * 1);
            // ViewBag.total = Math.Round(ViewBag.DollarAmount, 2) * 100;
            // ViewBag.total = Convert.ToInt64(ViewBag.total);
            // long total = ViewBag.total;
            // TotalAmount = total.ToString();

            var userName = User.Identity.Name; // userName is email
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();
            if (user != null)
            {
                Amount = _context.PlannedFlights.Include("Flight").Include("User").Where(u => u.User.UserName == userName).Sum(flight => (double)flight.Flight.Price).ToString();
                ViewBag.Total = Amount;
            }
            ViewBag.count = HttpContext.Session.GetString("Count");
            return View();
        }

        [HttpPost]
        public ActionResult CreateCheckoutSession(string total)
        {
            ViewBag.count = Convert.ToInt32(HttpContext.Session.GetString("Count"));
            if (ViewBag.count == 0)
            {
                TempData["DeleteCartItem"] = "Your cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            var userName = User.Identity.Name;
            PlannedList = _context.PlannedFlights.Include("User").Include("Flight").Include("Flight.Origin").Include("Flight.Destination").Where(u => u.User.UserName == userName).ToList();
            List<SessionLineItemOptions> lineItems = new List<SessionLineItemOptions>();

            for (int i = 0; i < PlannedList.Count; i++)
            {
                //var description=PlannedList[i].DepartureDate.ToString("YYYY-MM-DD");
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = Convert.ToInt32(PlannedList[i].Flight.Price) * 100,
                        Currency = "CAD",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{PlannedList[i].Flight.Origin.Name} - {PlannedList[i].Flight.Destination.Name}",
                            Description = PlannedList[i].DepartureDate.ToString("yyyy-MMM-dd")

                        },
                    },
                    Quantity = 1,
                    TaxRates = new List<string> { "txr_1MYy7VFlggO7VCcxohbs6bVb", "txr_1MYy4aFlggO7VCcx7cIMZZzG" }
                });
            }

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                // AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },

                LineItems = lineItems,
                // LineItems = new List<SessionLineItemOptions>
                // {
                //     new SessionLineItemOptions
                //     {
                //          PriceData = new SessionLineItemPriceDataOptions
                //         {
                //             UnitAmount = Convert.ToInt32(Amount) * 100,
                //             Currency = "CAD",
                //             ProductData = new SessionLineItemPriceDataProductDataOptions
                //             {
                //                 Name = "Flight",
                //             },

                //         },
                //         Quantity = 1,
                //         TaxRates = new List<string> { "txr_1MYy7VFlggO7VCcxohbs6bVb", "txr_1MYy4aFlggO7VCcx7cIMZZzG" }
                //     },                    
                // },
                Mode = "payment",
                AllowPromotionCodes = true,

                // Discounts = new List<SessionDiscountOptions>
                // {
                //     new SessionDiscountOptions
                //     {
                //         Coupon = "VimDmyfr",
                //     },
                // },

                SuccessUrl = "https://zgztravelbooking.azurewebsites.net/Checkout/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://zgztravelbooking.azurewebsites.net/Checkout/Cancel",
            };
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            HttpContext.Session.SetString("Count", "0");
            return new StatusCodeResult(303);
        }

        [HttpGet("/checkout/success")]
        public async Task<IActionResult> success([FromQuery] string session_id)
        {
            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id);

            // var customerService = new CustomerService();
            // Customer customer = customerService.Get(session.CustomerId);

            // var userName=session.Customer.Name;
            var userName = User.Identity.Name; // userName is email
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

            // var service = new PaymentIntentService();
            // var intent = service.Get(session_id);
            var purchaseConfirmationCode = session.PaymentIntentId;
            ;
            var whenPaid = DateTime.Now;
            var TotalAmount = Math.Round((decimal)session.AmountTotal / 100, 2);
            var newPurchase = new Purchase { User = user, PurchaseConfirmationCode = purchaseConfirmationCode, WhenPaid = whenPaid, TotalAmount = TotalAmount };
            _context.Add(newPurchase);
            await _context.SaveChangesAsync();

            var purchase = _context.Purchases.Include("User").Where(p => p.PurchaseConfirmationCode == purchaseConfirmationCode && p.User.UserName == userName).FirstOrDefault();

            PlannedList = _context.PlannedFlights.Include("User").Include("Flight").Where(u => u.User.UserName == userName).ToList();

            foreach (var pf in PlannedList)
            {
                var newPurchasedFlight = new PurchasedFlight { Purchase = purchase, DepartureDate = pf.DepartureDate, Flight = pf.Flight, Price = pf.Flight.Price };
                _context.Add(newPurchasedFlight);
                await _context.SaveChangesAsync();
                _context.Remove(pf);
                await _context.SaveChangesAsync();
            }

            // Create a new invoice
            string filePath = GenerateInvoice(session_id);

            // Send the invoice
            SendInvoice(session.CustomerDetails.Email, filePath);

            ViewBag.AmountPaid = TotalAmount;
            ViewBag.Customer = userName;
            ViewBag.Email = session.CustomerDetails.Email;
            return View();
        }

        public ActionResult cancel()
        {
            return View();
        }

        public void SendInvoice(string recipientEmail, string filePath)
        {
            using (var client = new SmtpClient())
            {
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;


                var credential = new System.Net.NetworkCredential
                {
                    UserName = "zibin.guo13@gmail.com",  // replace with valid value
                    Password = "******"  // replace with valid value (SMTP generated password)
                };
                client.Credentials = credential;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("TravelBookingZGZ@gmail.com", "TravelBookingZGZ");
                mailMessage.To.Add(recipientEmail);
                mailMessage.Subject = "Invoice for your travel booking purchase";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = "Please find attached your invoice for your recent purchase.";
                mailMessage.Attachments.Add(new Attachment(filePath));

                client.Send(mailMessage);
            }
        }
        public string GenerateInvoice(string session_id)
        {
            var options = new SessionGetOptions { Expand = new List<string> { "line_items.data.price.product" } };
            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id, options);

            //generate a pdf file of invoice
            string fileName = $"Invoice_{session_id.Substring(session_id.Length - 6).ToUpper()}.pdf";
            string filePath = @"..\" + fileName;
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                using (var document = new Document())
                {
                    var writer = PdfWriter.GetInstance(document, fs);

                    document.Open();

                    var font = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var newLineParagraph = new Paragraph($"\n", font);

                    var bookingParagraph = new Paragraph($"Travel Booking ZGZ") { Font = FontFactory.GetFont(FontFactory.HELVETICA, 20), Alignment = Element.ALIGN_CENTER };
                    document.Add(bookingParagraph);
                    document.Add(newLineParagraph);

                    var invoiceParagraph = new Paragraph($"Invoice Number:{session_id.Substring(session_id.Length - 6).ToUpper()}\n") { Font = FontFactory.GetFont(FontFactory.HELVETICA, 16) };
                    document.Add(invoiceParagraph);
                    document.Add(newLineParagraph);

                    var dateParagraph = new Paragraph($"Date: {DateTime.Now.ToString("yyyy-MMM-dd")}", font);
                    document.Add(dateParagraph);
                    document.Add(newLineParagraph);

                    var itemsTable = new PdfPTable(4) { DefaultCell = { Border = Rectangle.NO_BORDER } };
                    itemsTable.WidthPercentage = 100;
                    itemsTable.SetWidths(new float[] { 1, 1, 1, 1 });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Origin - Destination", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("DepartureDate", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Quantity", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Price(CAD)", font)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });

                    foreach (var item in session.LineItems)
                    {
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Price.Product.Name)));
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Price.Product.Description)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString())) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        itemsTable.AddCell(new PdfPCell(new Phrase($"{Math.Round((decimal)item.AmountSubtotal / 100, 2)} ")) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        // itemsTable.AddCell(new PdfPCell(new Phrase($"{Math.Round((decimal)item.AmountDiscount / 100, 2)} {session.Currency}", font)));
                        // itemsTable.AddCell(new PdfPCell(new Phrase($"{Math.Round((decimal)item.AmountTax / 100, 2)} {session.Currency}", font)));
                        // itemsTable.AddCell(new PdfPCell(new Phrase($"{Math.Round((decimal)item.AmountTotal / 100, 2)} {session.Currency}", font)));
                    }

                    document.Add(itemsTable);
                    document.Add(newLineParagraph);

                    var subtotalParagraph = new Paragraph($"Subtotal: ${Math.Round((decimal)session.AmountSubtotal / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(subtotalParagraph);

                    var discountParagraph = new Paragraph($"Discount: ${Math.Round((decimal)session.TotalDetails.AmountDiscount / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(discountParagraph);

                    // var shippingParagraph = new Paragraph($"Shipping: ${Math.Round((decimal)session.TotalDetails.AmountShipping / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    // document.Add(shippingParagraph);

                    var taxParagraph = new Paragraph($"Tax: ${Math.Round((decimal)session.TotalDetails.AmountTax / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(taxParagraph);

                    var totalParagraph = new Paragraph($"Total: ${Math.Round((decimal)session.AmountTotal / 100, 2)} {session.Currency.ToUpper()}", font) { Alignment = Element.ALIGN_RIGHT };
                    document.Add(totalParagraph);
                    document.Add(newLineParagraph);

                    var thanksParagraph = new Paragraph($"Thank you for your purchase!\n") { Font = FontFactory.GetFont(FontFactory.HELVETICA, 20), Alignment = Element.ALIGN_CENTER };
                    document.Add(thanksParagraph);

                    document.Close();

                }
            }
            return filePath;

        }

    }


}
