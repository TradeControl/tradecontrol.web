using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwTaxTypes", Schema = "Cash")]
    public class Cash_vwTaxType
    {
        [Key]
        [Required]
        [Display(Name = "Type Code")]
        public short TaxTypeCode { get; set; }
        [Display(Name = "Tax Type")]
        public string TaxType { get; set; }
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [Display(Name = "Cash Desc.")]
        public string CashDescription { get; set; }
        [Display(Name = "Month No.")]
        public short MonthNumber { get; set; }
        [Display(Name = "Start Month")]
        public string MonthName { get; set; }
        [Display(Name = "Recurrence Code")]
        public short RecurrenceCode { get; set; }
        [Display(Name = "Recurrence")]
        public string Recurrence { get; set; }
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }
        [Display(Name = "Account Name")]
        public string SubjectName { get; set; }
        [Display(Name = "Offset Days")]
        public short OffsetDays { get; set; }        
    }
}
