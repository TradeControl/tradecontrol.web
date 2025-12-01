using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwCompanyLogo
    {
        [Column(TypeName = "image")]
        public byte[] Logo { get; set; }
    }
}
