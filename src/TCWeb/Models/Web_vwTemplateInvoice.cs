using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Web_vwTemplateInvoice
    {
        public short InvoiceTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Invoice Type")]
        public string InvoiceType { get; set; }
        public int TemplateId { get; set; }
        [StringLength(256)]
        [Display(Name = "Template Name")]
        public string TemplateFileName { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name ="Last Used")]
        [DataType(DataType.DateTime)]
        public DateTime? LastUsedOn { get; set; }
    }
}
