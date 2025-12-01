using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwCandidatePurchase
    {
        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionedOn { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalCharge { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
        [StringLength(255)]
        public string ProjectNotes { get; set; }
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(50)]
        public string ActionName { get; set; }
        [Required]
        [StringLength(50)]
        public string OwnerName { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        public short ProjectStatusCode { get; set; }
    }
}
