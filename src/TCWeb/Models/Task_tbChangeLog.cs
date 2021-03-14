using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbChangeLog", Schema = "Task")]
    [Index(nameof(ChangedOn), Name = "IX_Task_tbChangeLog_ChangedOn")]
    [Index(nameof(LogId), Name = "IX_Task_tbChangeLog_LogId", IsUnique = true)]
    [Index(nameof(TransmitStatusCode), nameof(ChangedOn), Name = "IX_Task_tbChangeLog_TransmitStatus")]
    public partial class Task_tbChangeLog
    {
        [Key]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Key]
        public int LogId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ChangedOn { get; set; }
        public short TransmitStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        public short TaskStatusCode { get; set; }
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
        [InverseProperty(nameof(Org_tbTransmitStatus.TbTaskChangeLogs))]
        public virtual Org_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }
    }
}
