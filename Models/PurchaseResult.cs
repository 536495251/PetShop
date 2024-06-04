using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyPetShop_v3.Models
{
    public class PurchaseResult
    {
        public PurchaseResult() { }
        public int IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public PurchaseResult(int isSuccess, string errorMessage)
        {
            this.IsSuccess = isSuccess;
            this.ErrorMessage = errorMessage;
        }
    }
}