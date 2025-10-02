using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace TradeControl.Web.Models
{

    [Table("vwType", Schema = "Invoice")]
    public class Invoice_vwType
    {
        [Key]
        [Display(Name = "Code")]
        public short InvoiceTypeCode { get; set; }
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
        [Display(Name = "Mode Code")]
        public short CashPolarityCode { get; set; }
        [Display(Name = "Mode")]
        public string CashPolarity { get; set; }
        [Display(Name = "Next Number")]
        public int NextNumber { get; set; }
    }
}
