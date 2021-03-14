using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwOp
    {
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        public short OperationNumber { get; set; }
        public short Period { get; set; }
        [StringLength(10)]
        public string BucketId { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        public short SyncTypeCode { get; set; }
        public short OpStatusCode { get; set; }
        [Required]
        [StringLength(50)]
        public string Operation { get; set; }
        [Column(TypeName = "ntext")]
        public string Note { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EndOn { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Duration { get; set; }
        public short OffsetDays { get; set; }
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
        [StringLength(100)]
        public string TaskTitle { get; set; }
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalCharge { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        public byte[] OpRowVer { get; set; }
        [Required]
        public byte[] TaskRowVer { get; set; }
    }
}
