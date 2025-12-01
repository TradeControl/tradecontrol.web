using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Project_vwChangeLog
    {
        public int LogId { get; set; }
        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ChangedOn { get; set; }
        public short TransmitStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string TransmitStatus { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        public short ProjectStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "decimal(37, 11)")]
        public decimal TotalCharge { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TaxRate { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
    }
}
