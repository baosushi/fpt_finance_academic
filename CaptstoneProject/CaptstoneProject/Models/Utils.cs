using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Reflection;
namespace CaptstoneProject.Models
{
    public static class Utils
    {
        public static DateTime ToDateTime(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception("Chuỗi ngày tháng không đúng định dạng");
            }
        }

        public static DateTime GetEndOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59);
        }

        public static DateTime GetStartOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
        }

        public static string GetEnumDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>().GetName();
        }
    }
}