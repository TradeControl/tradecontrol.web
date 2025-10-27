using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwCode", Schema = "Cash")]
    public class Cash_vwCode
    {
        [Key]
        [Required]
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Description")]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; }
        public short CashPolarityCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Polarity")]
        public string CashPolarity { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxDescription { get; set; }
        public short CashTypeCode { get; set; }
        [StringLength(25)]
        [Display(Name = "Type")]
        public string CashType { get; set; }
        [Display(Name = "Cash On?")]
        public bool IsCashEnabled { get; set; }
        [Display(Name = "Category On?")]
        public bool IsCategoryEnabled { get; set; }
        [StringLength(50)]
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Inserted")]
        public DateTime InsertedOn { get; set; }
        [StringLength(50)]
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
    }
}
