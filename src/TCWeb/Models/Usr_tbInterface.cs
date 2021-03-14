using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbInterface", Schema = "Usr")]
    public partial class Usr_tbInterface
    {
        public Usr_tbInterface()
        {
            TbMenus = new HashSet<Usr_tbMenu>();
        }

        [Key]
        public short InterfaceCode { get; set; }
        [Required]
        [StringLength(30)]
        public string Interface { get; set; }

        [InverseProperty(nameof(Usr_tbMenu.InterfaceCodeNavigation))]
        public virtual ICollection<Usr_tbMenu> TbMenus { get; set; }
    }
}
