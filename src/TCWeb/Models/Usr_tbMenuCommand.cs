using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMenuCommand", Schema = "Usr")]
    public partial class Usr_tbMenuCommand
    {
        public Usr_tbMenuCommand()
        {
            TbMenuEntries = new HashSet<Usr_tbMenuEntry>();
        }

        [Key]
        public short Command { get; set; }
        [StringLength(50)]
        public string CommandText { get; set; }

        [InverseProperty(nameof(Usr_tbMenuEntry.CommandNavigation))]
        public virtual ICollection<Usr_tbMenuEntry> TbMenuEntries { get; set; }
    }
}
