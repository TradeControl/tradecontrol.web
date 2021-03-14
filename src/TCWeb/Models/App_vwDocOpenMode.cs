using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwDocOpenMode
    {
        public short OpenMode { get; set; }
        [StringLength(20)]
        public string OpenModeDescription { get; set; }
    }
}
