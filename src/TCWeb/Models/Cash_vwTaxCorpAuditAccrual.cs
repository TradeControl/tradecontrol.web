using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxCorpAuditAccrual
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(21)]
        public string YearPeriod { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [Column(TypeName = "decimal(19, 4)")]
        public decimal QuantityRemaining { get; set; }
        [Column(TypeName = "decimal(21, 5)")]
        public decimal OrderValue { get; set; }
        public float TaxDue { get; set; }
    }
}
