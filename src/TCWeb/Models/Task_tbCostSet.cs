using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCostSet", Schema = "Task")]
    [Index(nameof(UserId), nameof(TaskCode), Name = "IX_Task_tbCostSet_UserId", IsUnique = true)]
    public partial class Task_tbCostSet
    {
        [Key]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Key]
        [StringLength(10)]
        public string UserId { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(TaskCode))]
        [InverseProperty(nameof(Task_tbTask.TbCostSets))]
        public virtual Task_tbTask TaskCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbCostSets))]
        public virtual Usr_tbUser User { get; set; }
    }
}
