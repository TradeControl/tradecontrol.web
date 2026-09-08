using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbReportingProfile", Schema = "Cash")]
    public partial class Cash_tbReportingProfile
    {
        [Key]
        public int ReportingProfileId { get; set; }

        [Required, StringLength(50)]
        public string SubjectCode { get; set; }

        [StringLength(20)]
        public string TaxSourceCode { get; set; }

        [Required, StringLength(20)]
        public string AuthorityCode { get; set; }

        [Required, StringLength(20)]
        public string ReportingTypeCode { get; set; }

        [StringLength(100)]
        public string AuthorityReference { get; set; }

        [Column(TypeName = "date")]
        public DateTime ValidFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ValidTo { get; set; }

        public short StatusCode { get; set; }

        [Required, StringLength(10)]
        public string ValueSourceCode { get; set; }

        public bool IsReviewed { get; set; }

        [Required, StringLength(50)]
        public string InsertedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }

        [Required, StringLength(50)]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }

        [ForeignKey(nameof(TaxSourceCode))]
        public virtual Cash_tbTaxTagSource TaxSourceCodeNavigation { get; set; }

        [ForeignKey(nameof(AuthorityCode))]
        public virtual App_tbAuthority AuthorityCodeNavigation { get; set; }

        [ForeignKey(nameof(ReportingTypeCode))]
        public virtual App_tbReportingType ReportingTypeCodeNavigation { get; set; }

        [ForeignKey(nameof(StatusCode))]
        public virtual App_tbStatutoryStatus StatusCodeNavigation { get; set; }

        [ForeignKey(nameof(ValueSourceCode))]
        public virtual App_tbValueSource ValueSourceCodeNavigation { get; set; }
    }
}
