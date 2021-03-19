using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Cash_proc_CodeDefaults
    {
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        [Column(TypeName = "smallint")]
        public NodeEnum.TaxType TaxTypeCode { get; set; } = NodeEnum.TaxType.General;
        [Column(TypeName = "smallint")]
        public NodeEnum.CashMode CashModeCode { get; set; } = NodeEnum.CashMode.Neutral;
        [Column(TypeName = "smallint")]
        public NodeEnum.CashType CashTypeCode { get; set; } = NodeEnum.CashType.Trade;

    }
}
