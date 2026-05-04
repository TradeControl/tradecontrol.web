using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbVirtual", Schema = "Subject")]
    public partial class Subject_tbVirtual
    {
        [Key]
        [StringLength(50)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }

        [Display(Name = "Employees")]
        public int NumberOfEmployees { get; set; }

        [StringLength(20)]
        [Display(Name = "Company Number")]
        public string CompanyNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Vat Number")]
        public string VatNumber { get; set; }

        [Column("EUJurisdiction")]
        [Display(Name = "EU?")]
        public bool Eujurisdiction { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "Description")]
        public string BusinessDescription { get; set; }

        [Column(TypeName = "varbinary(max)")]
        [Display(Name = "Logo")]
        public byte[] Logo { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Turnover")]
        public decimal Turnover { get; set; }

        [StringLength(255)]
        [Display(Name = "Web Site")]
        public string WebSite { get; set; }

        [StringLength(100)]
        [Display(Name = "Source")]
        public string SubjectSource { get; set; }

        //public byte[] RowVer { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbVirtual))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
    }
}
