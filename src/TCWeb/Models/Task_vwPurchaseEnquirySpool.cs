using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwPurchaseEnquirySpool
    {
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [StringLength(100)]
        public string NickName { get; set; }
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Column(TypeName = "ntext")]
        public string InvoiceAddress { get; set; }
        [Column(TypeName = "ntext")]
        public string DeliveryAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [StringLength(255)]
        public string TaskNotes { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Required]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TaxRate { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalCharge { get; set; }
        [StringLength(50)]
        public string MobileNumber { get; set; }
        [Column(TypeName = "image")]
        public byte[] Signature { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
    }
}
