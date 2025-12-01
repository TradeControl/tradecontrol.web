using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwInvoiceSummary
    {
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal PeriodValue { get; set; }
    }
}
