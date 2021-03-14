using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMenuUser", Schema = "Usr")]
    public partial class Usr_tbMenuUser
    {
        [Key]
        [StringLength(10)]
        public string UserId { get; set; }
        [Key]
        public short MenuId { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(MenuId))]
        [InverseProperty(nameof(Usr_tbMenu.TbMenuUsers))]
        public virtual Usr_tbMenu Menu { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbMenuUsers))]
        public virtual Usr_tbUser User { get; set; }
    }
}
