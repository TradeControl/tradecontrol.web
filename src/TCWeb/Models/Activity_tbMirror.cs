using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirror", Schema = "Activity")]
    [Index(nameof(AccountCode), nameof(AllocationCode), Name = "IX_Activity_tbMirror_AllocationCode", IsUnique = true)]
    [Index(nameof(TransmitStatusCode), nameof(AllocationCode), Name = "IX_Activity_tbMirror_TransmitStatusCode")]
    public partial class Activity_tbMirror
    {
        [Key]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        [Key]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Key]
        [StringLength(50)]
        public string AllocationCode { get; set; }
        public short TransmitStatusCode { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Org_tbOrg.TbMirrors))]
        public virtual Org_tbOrg AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(ActivityCode))]
        [InverseProperty(nameof(Activity_tbActivity.TbMirrors))]
        public virtual Activity_tbActivity ActivityCodeNavigation { get; set; }
        [ForeignKey(nameof(TransmitStatusCode))]
        [InverseProperty(nameof(Org_tbTransmitStatus.TbActivityMirrors))]
        public virtual Org_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }
    }
}
