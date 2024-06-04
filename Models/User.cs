using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyPetShop_v3.Models
{
    public class User
    {
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public bool IsVip { get; set; }
        public decimal Balance { get; set; }

        public User() { }
        public User(string Phone, string Password, string Name, string State, string City, string Address, bool IsVip, decimal Balance)
        {
            this.Phone = Phone;
            this.Password = Password;
            this.State = State;
            this.City = City;
            this.Address = Address;
            this.IsVip = IsVip;
            this.Balance = Balance;
            this.Name = Name;
        }
    }
}