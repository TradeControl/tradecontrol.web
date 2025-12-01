using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCostSet", Schema = "Project")]
    [Index(nameof(UserId), nameof(ProjectCode), Name = "IX_Project_tbCostSet_UserId", IsUnique = true)]
    public partial class Project_tbCostSet
    {
        [Key]
        [StringLength(20)]
        public string ProjectCode { get; set; }
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

        [ForeignKey(nameof(ProjectCode))]
        [InverseProperty(nameof(Project_tbProject.TbCostSets))]
        public virtual Project_tbProject ProjectCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbCostSets))]
        public virtual Usr_tbUser User { get; set; }
    }
}
