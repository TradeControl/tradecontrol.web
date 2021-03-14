using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Usr_vwUserMenu
    {
        public short MenuId { get; set; }
        public short InterfaceCode { get; set; }
    }
}
