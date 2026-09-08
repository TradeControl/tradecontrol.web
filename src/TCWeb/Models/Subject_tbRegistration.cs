using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbRegistration", Schema = "Subject")]
    public partial class Subject_tbRegistration
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required, StringLength(50)]
        public string SubjectCode { get; set; }

        [Required, StringLength(20)]
        public string RegistrationSchemeCode { get; set; }

        [Required, StringLength(255)]
        public string RegistrationValue { get; set; }

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

        [ForeignKey(nameof(RegistrationSchemeCode))]
        public virtual App_tbRegistrationScheme RegistrationSchemeCodeNavigation { get; set; }

        [ForeignKey(nameof(StatusCode))]
        public virtual App_tbStatutoryStatus StatusCodeNavigation { get; set; }

        [ForeignKey(nameof(ValueSourceCode))]
        public virtual App_tbValueSource ValueSourceCodeNavigation { get; set; }
    }
}
