using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbEventType", Schema = "App")]
    public partial class App_tbEventType
    {
        public App_tbEventType()
        {
            TbAllocationEvents = new HashSet<Task_tbAllocationEvent>();
            TbEventLogs = new HashSet<App_tbEventLog>();
            TbMirrorEvents = new HashSet<Invoice_tbMirrorEvent>();
        }

        [Key]
        public short EventTypeCode { get; set; }
        [Required]
        [StringLength(15)]
        public string EventType { get; set; }

        [InverseProperty(nameof(Task_tbAllocationEvent.EventTypeCodeNavigation))]
        public virtual ICollection<Task_tbAllocationEvent> TbAllocationEvents { get; set; }
        [InverseProperty(nameof(App_tbEventLog.EventTypeCodeNavigation))]
        public virtual ICollection<App_tbEventLog> TbEventLogs { get; set; }
        [InverseProperty(nameof(Invoice_tbMirrorEvent.EventTypeCodeNavigation))]
        public virtual ICollection<Invoice_tbMirrorEvent> TbMirrorEvents { get; set; }
    }
}
