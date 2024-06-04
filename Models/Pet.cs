using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyPetShop_v3.Models
{
    public class Pet
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Descn { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public Pet() { }
        // 首页展示用的
        public Pet(int id, string category, string name, decimal price, string image, string descn)
        {
            ID = id;
            Name = name;
            Category = category;
            Descn = descn;
            Image = "/Images/" + image;
            Price = price;
        }
    }
}