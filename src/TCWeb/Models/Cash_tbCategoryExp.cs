using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // RowVer retained (ignored in application logic; legacy concurrency)
        public byte[] RowVer { get; set; }

        // evaluation status (set by background generator)
        [Required]
        public bool IsError { get; set; }

        // full error message (null or empty when IsError = false)
        [Column(TypeName = "nvarchar(max)")]
        public string ErrorMessage { get; set; }

        [ForeignKey(nameof(CategoryCode))]
        [InverseProperty(nameof(Cash_tbCategory.TbCategoryExp))]
        public virtual Cash_tbCategory CategoryCodeNavigation { get; set; }
    }
}
