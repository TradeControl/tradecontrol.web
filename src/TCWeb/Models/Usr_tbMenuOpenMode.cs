using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMenuOpenMode", Schema = "Usr")]
    public partial class Usr_tbMenuOpenMode
    {
        public Usr_tbMenuOpenMode()
        {
            TbDocs = new HashSet<App_tbDoc>();
            TbMenuEntries = new HashSet<Usr_tbMenuEntry>();
        }

        [Key]
        public short OpenMode { get; set; }
        [StringLength(20)]
        public string OpenModeDescription { get; set; }

        [InverseProperty(nameof(App_tbDoc.OpenModeNavigation))]
        public virtual ICollection<App_tbDoc> TbDocs { get; set; }
        [InverseProperty(nameof(Usr_tbMenuEntry.OpenModeNavigation))]
        public virtual ICollection<Usr_tbMenuEntry> TbMenuEntries { get; set; }
    }
}
