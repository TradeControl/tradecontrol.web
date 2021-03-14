using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMenuView", Schema = "Usr")]
    public partial class Usr_tbMenuView
    {
        public Usr_tbMenuView()
        {
            TbUsers = new HashSet<Usr_tbUser>();
        }

        [Key]
        public short MenuViewCode { get; set; }
        [Required]
        [StringLength(30)]
        public string MenuView { get; set; }

        [InverseProperty(nameof(Usr_tbUser.MenuViewCodeNavigation))]
        public virtual ICollection<Usr_tbUser> TbUsers { get; set; }
    }
}
