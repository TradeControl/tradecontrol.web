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
        [StringLength(10)]
        public string VersionId { get; set; }
        [StringLength(10)]
        public string ParentCode { get; set; }
        [StringLength(10)]
        public string ChildCode { get; set; }
        [Display(Name = "Display Order")]
        public short DisplayOrder { get; set; }

        [InverseProperty(nameof(Cash_tbCategory.TbCategoryTotalChildCodeNavigations))]
        public virtual Cash_tbCategory ChildCodeNavigation { get; set; }
        [InverseProperty(nameof(Cash_tbCategory.TbCategoryTotalParentCodeNavigations))]
        public virtual Cash_tbCategory ParentCodeNavigation { get; set; }
        

    }
}
