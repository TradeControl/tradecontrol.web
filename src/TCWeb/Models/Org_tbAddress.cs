using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAddress", Schema = "Org")]
    public partial class Org_tbAddress
    {
        public Org_tbAddress()
        {
            TbOrgs = new HashSet<Org_tbOrg>();
            TbTaskAddressCodeFromNavigations = new HashSet<Task_tbTask>();
            TbTaskAddressCodeToNavigations = new HashSet<Task_tbTask>();
        }

        [Key]
        [StringLength(15)]
        public string AddressCode { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [Column(TypeName = "ntext")]
        public string Address { get; set; }
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
        [InverseProperty(nameof(Org_tbOrg.TbAddresses))]
        public virtual Org_tbOrg AccountCodeNavigation { get; set; }
        [InverseProperty(nameof(Org_tbOrg.AddressCodeNavigation))]
        public virtual ICollection<Org_tbOrg> TbOrgs { get; set; }
        [InverseProperty(nameof(Task_tbTask.AddressCodeFromNavigation))]
        public virtual ICollection<Task_tbTask> TbTaskAddressCodeFromNavigations { get; set; }
        [InverseProperty(nameof(Task_tbTask.AddressCodeToNavigation))]
        public virtual ICollection<Task_tbTask> TbTaskAddressCodeToNavigations { get; set; }
    }
}
