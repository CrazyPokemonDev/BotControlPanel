using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotControlPanel.Helpers
{
    public static class Extensions
    {
        public static string ToFileNameString(this DateTime dt)
        {
            return $"{dt.Year}-{dt.Month}-{dt.Day}_{dt.Hour}-{dt.Minute}-{dt.Second}";
        }
    }
}
