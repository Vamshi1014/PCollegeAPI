using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Helpers
{
    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetInt32OrDefault(this SqlDataReader reader, string columnName)
        {
            if (reader.HasColumn(columnName) && !reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetInt32(reader.GetOrdinal(columnName));
            }
            return default;
        }

        public static string GetSafeString(this SqlDataReader reader, string columnName)
        {
            if (reader.HasColumn(columnName) && !reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetString(reader.GetOrdinal(columnName));
            }
            return null;
        }

        public static bool GetBooleanOrDefault(this SqlDataReader reader, string columnName, bool defaultValue = false)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) ? reader.GetBoolean(ordinal) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal GetSafeDecimal(this SqlDataReader reader, string columnName)
        {
            if (reader.HasColumn(columnName) && !reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetDecimal(reader.GetOrdinal(columnName));
            }
            return default;
        }

        public static int GetSafeInt(this SqlDataReader reader, string columnName)
        {
            if (reader.HasColumn(columnName) && !reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetInt32(reader.GetOrdinal(columnName));
            }
            return default;
        }

        //public static string GetSafeString(this SqlDataReader reader, string columnName)
        //{
        //    return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(reader.GetOrdinal(columnName));
        //}

        //public static int? GetSafeInt(this SqlDataReader reader, string columnName)
        //{
        //    return reader.IsDBNull(reader.GetOrdinal(columnName)) ? (int?)null : reader.GetInt32(reader.GetOrdinal(columnName));
        //}

        //public static decimal? GetSafeDecimal(this SqlDataReader reader, string columnName)
        //{
        //    return reader.IsDBNull(reader.GetOrdinal(columnName)) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal(columnName));
        //}

        public static DateTime GetSafeDateTime(this SqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal(columnName));
        }
    }

}
