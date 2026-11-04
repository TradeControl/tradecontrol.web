using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCategory", Schema = "Cash")]
    public partial class Cash_tbCategory
    {
        public Cash_tbCategory()
        {
            TbCategoryTotalChildCodeNavigations = new HashSet<Cash_tbCategoryTotal>();
            TbCategoryTotalParentCodeNavigations = new HashSet<Cash_tbCategoryTotal>();
        }

        [StringLength(10)]
        public string VersionId { get; set; }
        [StringLength(10)]
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; }
        [Display(Name = "Category Type")]
        public short CategoryTypeCode { get; set; }
        [Display(Name = "Cash Mode")]
        public short CashPolarityCode { get; set; }
        [Display(Name = "Cash Type")]
        public short CashTypeCode { get; set; }
        [Display(Name = "Display Order")]
        public short DisplayOrder { get; set; }
        [Display(Name = "Enabled?")]
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
        [ForeignKey(nameof(CashPolarityCode))]
        [InverseProperty(nameof(Cash_tbPolarity.TbCategories))]
        public virtual Cash_tbPolarity CashPolarityCodeNavigation { get; set; }
        [ForeignKey(nameof(CashTypeCode))]
        [InverseProperty(nameof(Cash_tbType.TbCategories))]
        public virtual Cash_tbType CashTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(CategoryTypeCode))]
        [InverseProperty(nameof(Cash_tbCategoryType.TbCategories))]
        public virtual Cash_tbCategoryType CategoryTypeCodeNavigation { get; set; }
        [InverseProperty(nameof(Cash_tbCategoryTotal.ChildCodeNavigation))]
        public virtual ICollection<Cash_tbCategoryTotal> TbCategoryTotalChildCodeNavigations { get; set; }
        [InverseProperty(nameof(Cash_tbCategoryTotal.ParentCodeNavigation))]
        public virtual ICollection<Cash_tbCategoryTotal> TbCategoryTotalParentCodeNavigations { get; set; }
    }
}
