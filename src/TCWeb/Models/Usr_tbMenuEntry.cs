using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMenuEntry", Schema = "Usr")]
    [Index(nameof(MenuId), nameof(FolderId), nameof(ItemId), Name = "IX_Usr_tbMenuEntry_MenuFolderItem", IsUnique = true)]
    public partial class Usr_tbMenuEntry
    {
        [Key]
        public short MenuId { get; set; }
        [Key]
        public int EntryId { get; set; }
        public short FolderId { get; set; }
        public short ItemId { get; set; }
        [StringLength(255)]
        public string ItemText { get; set; }
        public short Command { get; set; }
        [StringLength(50)]
        public string ProjectName { get; set; }
        [StringLength(50)]
        public string Argument { get; set; }
        public short OpenMode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(Command))]
        [InverseProperty(nameof(Usr_tbMenuCommand.TbMenuEntries))]
        public virtual Usr_tbMenuCommand CommandNavigation { get; set; }
        [ForeignKey(nameof(MenuId))]
        [InverseProperty(nameof(Usr_tbMenu.TbMenuEntries))]
        public virtual Usr_tbMenu Menu { get; set; }
        [ForeignKey(nameof(OpenMode))]
        [InverseProperty(nameof(Usr_tbMenuOpenMode.TbMenuEntries))]
        public virtual Usr_tbMenuOpenMode OpenModeNavigation { get; set; }
    }
}
