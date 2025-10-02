using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbPolarity", Schema = "Cash")]
    public partial class Cash_tbPolarity
    {
        public Cash_tbPolarity()
        {
            TbAllocations = new HashSet<Project_tbAllocation>();
            TbCategories = new HashSet<Cash_tbCategory>();
            TbInvoiceType = new HashSet<Invoice_tbType>();
            TbSubjectType = new HashSet<Subject_tbType>();
        }

        [Key]
        public short CashPolarityCode { get; set; }
        [StringLength(10)]
        public string CashPolarity { get; set; }

        [InverseProperty(nameof(Project_tbAllocation.CashPolarityCodeNavigation))]
        public virtual ICollection<Project_tbAllocation> TbAllocations { get; set; }
        [InverseProperty(nameof(Cash_tbCategory.CashPolarityCodeNavigation))]
        public virtual ICollection<Cash_tbCategory> TbCategories { get; set; }
        [InverseProperty(nameof(Invoice_tbType.CashPolarityCodeNavigation))]
        public virtual ICollection<Invoice_tbType> TbInvoiceType { get; set; }
        [InverseProperty(nameof(Subject_tbType.CashPolarityCodeNavigation))]
        public virtual ICollection<Subject_tbType> TbSubjectType { get; set; }
    }
}
