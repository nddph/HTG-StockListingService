using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealCommon
{
    public class Helper
    {
        public static string FormatRequestString(string data)
        {
            var result = string.IsNullOrWhiteSpace(data) ? "null" : "N'" + data.Trim().Replace("{", "{{").Replace("}", "}}") + "'";
            return result;
        }

        public static string FormatRequestSmallInt(short? data)
        {
            var result = data == null ? "null" : "'" + data + "'";
            return result;
        }

        public static string FormatRequestInt(int? data)
        {
            var result = data == null ? "null" : "'" + data + "'";
            return result;
        }

        public static string FormatRequestBool(bool? data)
        {
            var result = data == null ? "null" : (data == true ? "'true'" : "'false'");
            return result;
        }

        public static string FormatRequestGuid(Guid? data)
        {
            var result = data == null || data == Guid.Empty ? "null" : "'" + data + "'";
            return result;
        }

        public static string FormatRequestDate(DateTime? date)
        {
            var result = date == null || date == DateTime.MinValue ? "null" : "'" + date + "'";
            return result;
        }

        public static string TrimString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            return text.Trim();
        }

        public static string LowerString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            text = TrimString(text);
            return text.ToLower();
        }

    }
}
