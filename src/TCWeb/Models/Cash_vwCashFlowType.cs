using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwCashFlowType
    {
        public short CashTypeCode { get; set; }
        [StringLength(25)]
        public string CashType { get; set; }
    }
}
