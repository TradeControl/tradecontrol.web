using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbText", Schema = "App")]
    public partial class App_tbText
    {
        [Key]
        public int TextId { get; set; }
        [Required]
        [Column(TypeName = "ntext")]
        public string Message { get; set; }
        public short Arguments { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
