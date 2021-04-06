using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Models
{
    [Table("AspNetUserRegistrations")]
    public partial class AspNet_UserRegistration
    {
        [Key]
        [StringLength(450)]
        public string Id { get; set; }
        [StringLength(256)]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }
        [StringLength(50)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name = "Confirmed?")]
        public bool IsConfirmed { get; set; }
        [Display(Name = "Registered?")]
        public bool IsRegistered { get; set; }
        [Display(Name = "Administrator?")]
        public bool IsAdministrator { get; set; }
        [Display(Name = "Manager?")]
        public bool IsManager { get; set; }
    }
}
