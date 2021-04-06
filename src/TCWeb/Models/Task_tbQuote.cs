using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbQuote", Schema = "Task")]
    public partial class Task_tbQuote
    {
        [Key]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Key]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal RunOnQuantity { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal RunBackQuantity { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalPrice { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal RunOnPrice { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal RunBackPrice { get; set; }
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
        public byte[] RowVer { get; set; }
        [ForeignKey(nameof(TaskCode))]
        [InverseProperty(nameof(Task_tbTask.TbQuotes))]
        public virtual Task_tbTask TaskCodeNavigation { get; set; }
    }
}
