using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbEventLog", Schema = "App")]
    [Index(nameof(EventTypeCode), nameof(LoggedOn), Name = "IX_App_tbEventLog_EventType")]
    [Index(nameof(LoggedOn), Name = "IX_App_tbEventLog_LoggedOn")]
    public partial class App_tbEventLog
    {
        [Key]
        [StringLength(20)]
        public string LogCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LoggedOn { get; set; }
        public short EventTypeCode { get; set; }
        public string EventMessage { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(EventTypeCode))]
        [InverseProperty(nameof(App_tbEventType.TbEventLogs))]
        public virtual App_tbEventType EventTypeCodeNavigation { get; set; }
    }
}
