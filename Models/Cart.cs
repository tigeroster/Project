using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class Cart
    {
        public string image { get; set; }
        public string game { get; set; }
        public double price { get; set; }

        public Cart()
        {

        }
        public Cart(string image, string game, double price)
        {
            this.image = image;
            this.game = game;
            this.price = price;
        }

        List<Cart> cartList = new List<Cart>();

        public List<Cart> GetCarts()
        {

            return cartList;
        }
    }

}