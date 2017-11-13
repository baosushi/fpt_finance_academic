using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Reflection;
using DataService.Model;

namespace CaptstoneProject.Models
{

    public static class DataContextExtensions
    {
        public static DB_Finance_AcademicEntities BulkInsert<T>(this DB_Finance_AcademicEntities context, T entity, int count, int batchSize) where T : class
        {
            context.Set<T>().Add(entity);

            if (count % batchSize == 0)
            {
                context.SaveChanges();
                context.Dispose();
                context = new DB_Finance_AcademicEntities();

                // This is optional, allow to tracking
                //context.Configuration.AutoDetectChangesEnabled = false;
            }
            return context;
        }
    }

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