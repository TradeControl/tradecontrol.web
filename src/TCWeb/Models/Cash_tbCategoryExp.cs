using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCategoryExp", Schema = "Cash")]
    public partial class Cash_tbCategoryExp
    {
        [Key]
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(256)]
        public string Expression { get; set; }
        [Required]
        [StringLength(100)]
        public string Format { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(CategoryCode))]
        [InverseProperty(nameof(Cash_tbCategory.TbCategoryExp))]
        public virtual Cash_tbCategory CategoryCodeNavigation { get; set; }
    }
}
