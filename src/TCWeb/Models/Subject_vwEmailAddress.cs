using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace TradeControl.Web.Models
{
    [Keyless]
    public class Subject_vwEmailAddress
    {
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        [Display(Name = "Contact")]
        public string ContactName { get; set; }
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Display(Name = "Admin?")]
        public bool IsAdmin { get; set; }
    }
}
