using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;

namespace DbAccess.Tools
{
    public class DataMapper<T> where T : class
    {
        PropertyInfo[] properties;

        private static readonly DataMapper<T> _instance = new DataMapper<T>(); // readonly makes it thread safe (Should be smart enough maybe??)

        private DataMapper()
        {
            properties = typeof(T).GetProperties();
        }

        public static DataMapper<T> Instance
        {
            get { return _instance; }
        }

        public T MapToObject(IDataReader reader)
        {
            IEnumerable<string> columns = reader.GetSchemaTable().Rows.Cast<DataRow>().Select(c => c["ColumnName"].ToString().ToLower());

            T obj = Activator.CreateInstance<T>();

            foreach(PropertyInfo pinfo in properties)
            {
                if(columns.Contains(pinfo.Name.ToLower()))
                {
                    if(reader[pinfo.Name] != DBNull.Value)
                    {
                        if (reader[pinfo.Name].GetType() == typeof(decimal))
                        {
                            pinfo.SetValue(obj, reader.GetDouble(pinfo.Name));
                        }
                        else
                        {
                            pinfo.SetValue(obj, (reader.GetValue(reader.GetOrdinal(pinfo.Name)) ?? null), null);
                        }
                    }
                }
            }

            return obj;
        }
    }

    public static class DataHelper
    {
        // extensions
        public static double GetDouble(this IDataReader reader, string columnName) // this extends class, so you can invoke it only using one assignment
        {
            double dbl = 0;
            double.TryParse(reader[columnName].ToString(), out dbl);
            return dbl;
        }

        public static double GetDouble(this IDataReader reader, int columnIndex)
        {
            double dbl = 0;
            double.TryParse(reader[columnIndex].ToString(), out dbl);
            return dbl;
        }

        public static T GetParamValue<T>(this IDbDataParameter[] dbParams, string paramName)
        {
            foreach (IDbDataParameter param in dbParams)
            {
                if (param.ParameterName.ToLower() == paramName.ToLower())
                {
                    return (T)param.Value;
                }
            }
            return default(T);
        }
    }
}
