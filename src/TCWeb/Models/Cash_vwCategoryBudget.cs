using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwCategoryBudget
    {
        [Required]
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        public short CategoryTypeCode { get; set; }
        public short CashPolarityCode { get; set; }
        public short CashTypeCode { get; set; }
        public short DisplayOrder { get; set; }
        public short IsEnabled { get; set; }
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
    }
}
