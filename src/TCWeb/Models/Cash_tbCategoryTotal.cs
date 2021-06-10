using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCategoryTotal", Schema = "Cash")]
    public partial class Cash_tbCategoryTotal
    {
        [Key]
        [StringLength(10)]
        public string ParentCode { get; set; }
        [Key]
        [StringLength(10)]
        public string ChildCode { get; set; }

        [ForeignKey(nameof(ChildCode))]
        [InverseProperty(nameof(Cash_tbCategory.TbCategoryTotalChildCodeNavigations))]
        public virtual Cash_tbCategory ChildCodeNavigation { get; set; }
        [ForeignKey(nameof(ParentCode))]
        [InverseProperty(nameof(Cash_tbCategory.TbCategoryTotalParentCodeNavigations))]
        public virtual Cash_tbCategory ParentCodeNavigation { get; set; }
    }
}
