using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTransmitStatus", Schema = "Subject")]
    public partial class Subject_tbTransmitStatus
    {
        public Subject_tbTransmitStatus()
        {
            TbTaskChangeLogs = new HashSet<Task_tbChangeLog>();
            TbInvoiceChangeLogs = new HashSet<Invoice_tbChangeLog>();
            TbCashMirrors = new HashSet<Cash_tbMirror>();
            TbObjectMirrors = new HashSet<Object_tbMirror>();
            TbSubjects = new HashSet<Subject_tbSubject>();
        }

        [Key]
        public short TransmitStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string TransmitStatus { get; set; }

        [InverseProperty(nameof(Task_tbChangeLog.TransmitStatusCodeNavigation))]
        public virtual ICollection<Task_tbChangeLog> TbTaskChangeLogs { get; set; }
        [InverseProperty(nameof(Invoice_tbChangeLog.TransmitStatusCodeNavigation))]
        public virtual ICollection<Invoice_tbChangeLog> TbInvoiceChangeLogs { get; set; }
        [InverseProperty(nameof(Cash_tbMirror.TransmitStatusCodeNavigation))]
        public virtual ICollection<Cash_tbMirror> TbCashMirrors { get; set; }
        [InverseProperty(nameof(Object_tbMirror.TransmitStatusCodeNavigation))]
        public virtual ICollection<Object_tbMirror> TbObjectMirrors { get; set; }
        [InverseProperty(nameof(Subject_tbSubject.TransmitStatusCodeNavigation))]
        public virtual ICollection<Subject_tbSubject> TbSubjects { get; set; }
    }
}
