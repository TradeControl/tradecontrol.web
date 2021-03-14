using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Usr_vwUserMenuList
    {
        public short MenuId { get; set; }
        public short InterfaceCode { get; set; }
        public short FolderId { get; set; }
        public long RowNumber { get; set; }
        public int ItemId { get; set; }
        [StringLength(265)]
        public string ItemText { get; set; }
        public short Command { get; set; }
        [StringLength(50)]
        public string ProjectName { get; set; }
        [StringLength(50)]
        public string Argument { get; set; }
        public short OpenMode { get; set; }
    }
}
