using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.Web.Security;
using System.IO;
using PayPal.Api;

namespace Project.Controllers
{
    public class CartController : Controller
    {
        MonarchyEntities Registerentity = new MonarchyEntities();
        
        Cart cart = new Cart();
        public static List<Cart> cartList = new List<Cart>();
        public ActionResult AddToCart(int id)
        {

            var data = from g in Registerentity.Games where g.Game_Id == id select g;
            foreach (var details in data)
            {
                cartList.Add(new Cart(details.Game_Image, details.Game_Name, (double)details.Price));

            }

            Session["count"] = cartList.Count();
            return RedirectToAction("ProductCatalogue","Home");
        }

        // GET: Cart
        public ActionResult ViewCart()
        {
            double total = 0;

            for (int i = 0; i < cartList.Count(); i++)
            {
                total = total + cartList[i].price;
            }

            Session["total"] = total;
            
            return View(cartList);
        }
        public ActionResult RemoveProduct(string name)
        {
            for (int i = 0; i < cartList.Count(); i++)
            {
                if (cartList[i].game == name)
                {
                    cartList.Remove(cartList[i]);

                }
            }

            Session["count"] = cartList.Count();
            return RedirectToAction("ViewCart", cartList);
        }
        
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext  
            
            APIContext apiContext = PaypalConfig.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Cart/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("Failure");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Failure");
            }
            //on successful payment, show success page to user.  
            cartList.Clear();
            Session["count"] = 0;
            return RedirectToAction("Order");
            
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        public Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            //create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };
            List<Cart> list = (List<Cart>)cartList; 
            //Adding Item Details like name, currency, price etc
            foreach(var cart in list)
            {
                itemList.items.Add(new Item()
                {
                    name = cart.game,
                    currency = "USD",
                    price = cart.price.ToString(),
                    quantity = "1",
                    sku = "sku"
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Adding Tax, shipping and Subtotal details
            var details = new Details()
            {
                tax = "1",
                shipping = "0",
                subtotal = Session["total"].ToString(),
                shipping_discount = "0"
            };

            //Final amount with details
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(Session["total"]) + 1).ToString(), // Total must be equal to sum of tax, shipping and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);
        }

        public ActionResult Order(OrderDetail orderDetails)
        {
            int userId = (int)Session["id"];
            double total = (double)Session["total"];
            orderDetails.Customer_Id = userId;
            orderDetails.Order_Amount = total;
            orderDetails.Order_Date = DateTime.Now;
            Registerentity.OrderDetails.Add(orderDetails);
            Registerentity.SaveChanges();
            return View("Success");
        }
    }
}