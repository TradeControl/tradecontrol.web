using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbType", Schema = "Cash")]
    public partial class Cash_tbType
    {
        public Cash_tbType()
        {
            TbCategories = new HashSet<Cash_tbCategory>();
        }

        [Key]
        public short CashTypeCode { get; set; }
        [StringLength(25)]
        public string CashType { get; set; }

        [InverseProperty(nameof(Cash_tbCategory.CashTypeCodeNavigation))]
        public virtual ICollection<Cash_tbCategory> TbCategories { get; set; }
    }
}
