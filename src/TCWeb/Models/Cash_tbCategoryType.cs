using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCategoryType", Schema = "Cash")]
    public partial class Cash_tbCategoryType
    {
        public Cash_tbCategoryType()
        {
            TbCategories = new HashSet<Cash_tbCategory>();
        }

        [Key]
        public short CategoryTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string CategoryType { get; set; }

        [InverseProperty(nameof(Cash_tbCategory.CategoryTypeCodeNavigation))]
        public virtual ICollection<Cash_tbCategory> TbCategories { get; set; }
    }
}
