using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTransmitStatus", Schema = "Org")]
    public partial class Org_tbTransmitStatus
    {
        public Org_tbTransmitStatus()
        {
            TbTaskChangeLogs = new HashSet<Task_tbChangeLog>();
            TbInvoiceChangeLogs = new HashSet<Invoice_tbChangeLog>();
            TbCashMirrors = new HashSet<Cash_tbMirror>();
            TbActivityMirrors = new HashSet<Activity_tbMirror>();
            TbOrgs = new HashSet<Org_tbOrg>();
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
        [InverseProperty(nameof(Activity_tbMirror.TransmitStatusCodeNavigation))]
        public virtual ICollection<Activity_tbMirror> TbActivityMirrors { get; set; }
        [InverseProperty(nameof(Org_tbOrg.TransmitStatusCodeNavigation))]
        public virtual ICollection<Org_tbOrg> TbOrgs { get; set; }
    }
}
