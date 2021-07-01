using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwFlowCategory
    {
        public int CashTypeCode { get; set; }
        public long? EntryId { get; set; }
        [StringLength(25)]
        public string CashType { get; set; }
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [StringLength(50)]
        public string Category { get; set; }
        public short? CashModeCode { get; set; }
    }
}
