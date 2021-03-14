using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMenu", Schema = "Usr")]
    [Index(nameof(MenuName), nameof(MenuId), Name = "IX_Usr_tbMenu", IsUnique = true)]
    public partial class Usr_tbMenu
    {
        public Usr_tbMenu()
        {
            TbMenuEntries = new HashSet<Usr_tbMenuEntry>();
            TbMenuUsers = new HashSet<Usr_tbMenuUser>();
        }

        [Key]
        public short MenuId { get; set; }
        [Required]
        [StringLength(50)]
        public string MenuName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        public short InterfaceCode { get; set; }

        [ForeignKey(nameof(InterfaceCode))]
        [InverseProperty(nameof(Usr_tbInterface.TbMenus))]
        public virtual Usr_tbInterface InterfaceCodeNavigation { get; set; }
        [InverseProperty(nameof(Usr_tbMenuEntry.Menu))]
        public virtual ICollection<Usr_tbMenuEntry> TbMenuEntries { get; set; }
        [InverseProperty(nameof(Usr_tbMenuUser.Menu))]
        public virtual ICollection<Usr_tbMenuUser> TbMenuUsers { get; set; }
    }
}
