using System.Collections.Generic;
using TradeControl.Web.Models;

namespace TradeControl.Web.Model
{
    public partial class Usr_tbTheme
    {
        public string ThemeCode { get; set; } = null!;

        public string ThemeName { get; set; } = null!;

        public string CssFile { get; set; } = null!;

        public bool IsEnabled { get; set; }

        public virtual ICollection<Usr_tbUser> Usr_tbUsers { get; set; }
            = new List<Usr_tbUser>();
    }
}
