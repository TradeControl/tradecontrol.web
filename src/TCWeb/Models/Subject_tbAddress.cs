using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAddress", Schema = "Subject")]
    public partial class Subject_tbAddress
    {
        public Subject_tbAddress()
        {
            TbSubjects = new HashSet<Subject_tbSubject>();
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
        //[Required]
        //public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbAddresses))]
        public virtual Subject_tbSubject AccountCodeNavigation { get; set; }
        [InverseProperty(nameof(Subject_tbSubject.AddressCodeNavigation))]
        public virtual ICollection<Subject_tbSubject> TbSubjects { get; set; }
        [InverseProperty(nameof(Task_tbTask.AddressCodeFromNavigation))]
        public virtual ICollection<Task_tbTask> TbTaskAddressCodeFromNavigations { get; set; }
        [InverseProperty(nameof(Task_tbTask.AddressCodeToNavigation))]
        public virtual ICollection<Task_tbTask> TbTaskAddressCodeToNavigations { get; set; }
    }
}
