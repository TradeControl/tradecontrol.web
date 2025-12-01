using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbChangeLog", Schema = "Project")]
    [Index(nameof(ChangedOn), Name = "IX_Project_tbChangeLog_ChangedOn")]
    [Index(nameof(LogId), Name = "IX_Project_tbChangeLog_LogId", IsUnique = true)]
    [Index(nameof(TransmitStatusCode), nameof(ChangedOn), Name = "IX_Project_tbChangeLog_TransmitStatus")]
    public partial class Project_tbChangeLog
    {
        [Key]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        [Key]
        public int LogId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ChangedOn { get; set; }
        public short TransmitStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        public short ProjectStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }

        [ForeignKey(nameof(TransmitStatusCode))]
        [InverseProperty(nameof(Subject_tbTransmitStatus.TbProjectChangeLogs))]
        public virtual Subject_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }
    }
}
