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
        public string ProjectCode { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        public short ProjectStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
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
