using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class App_vwYear
    {
        [Display(Name ="Year")]
        public short YearNumber { get; set; }
        [Display(Name = "Start Month")]
        public string StartMonth { get; set; }
        [Display(Name = "Status Code")]
        public short CashStatusCode { get; set; }
        [Display(Name = "Status")]
        public string CashStatus { get; set; }
        [StringLength(10)]
        [Display(Name = "Financial Year")]
        public string Description { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Inserted")]
        public DateTime InsertedOn { get; set; }
    }
}
