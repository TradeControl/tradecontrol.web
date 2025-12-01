using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwProject
    {
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
        public short ProjectStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string ActionById { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime PaymentOn { get; set; }
        [StringLength(20)]
        public string SecondReference { get; set; }
        [StringLength(255)]
        public string ProjectNotes { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalCharge { get; set; }
        [StringLength(15)]
        public string AddressCodeFrom { get; set; }
        [StringLength(15)]
        public string AddressCodeTo { get; set; }
        public bool Printed { get; set; }
        public bool Spooled { get; set; }
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
        public short Period { get; set; }
        [StringLength(10)]
        public string BucketId { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(50)]
        public string OwnerName { get; set; }
        [Required]
        [StringLength(50)]
        public string ActionName { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [StringLength(255)]
        public string SubjectStatus { get; set; }
        [Required]
        [StringLength(50)]
        public string SubjectType { get; set; }
        public short CashPolarityCode { get; set; }
        [StringLength(10)]
        public string CashPolarity { get; set; }
    }
}
