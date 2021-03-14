using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwDocSalesOrder
    {
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        [Required]
        [StringLength(10)]
        public string ActionById { get; set; }
        [StringLength(10)]
        public string BucketId { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [StringLength(255)]
        public string TaskNotes { get; set; }
        [Required]
        [StringLength(50)]
        public string OwnerName { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(100)]
        public string CashDescription { get; set; }
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Required]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalCharge { get; set; }
        [Column(TypeName = "ntext")]
        public string FromAddress { get; set; }
        [Column(TypeName = "ntext")]
        public string ToAddress { get; set; }
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
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string ActionName { get; set; }
        public short Period { get; set; }
        public bool Printed { get; set; }
        public bool Spooled { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
