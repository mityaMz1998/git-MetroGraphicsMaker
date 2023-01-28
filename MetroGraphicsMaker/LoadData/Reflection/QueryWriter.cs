using System;
using System.Data;
using System.Linq;
using System.Text;

namespace WpfApplication1.LoadData.Reflection
{
    class QueryWriter
    {
        protected String separator = ", ";

        public String GenerateInsertQueryString(TableInfo table, DataRow row)
        {
            var names = GenerateNamesPart(table);
            var values = GenerateValuesPart(table, row);
            return String.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", table.Name, names, values);
        }

        protected String GenerateNamesPart(TableInfo table)
        {
            var names = new StringBuilder();
            foreach (var column in table.Columns)
                names.AppendFormat("[{0}]{1}", column.Name, separator);
            names.Remove(names.Length - separator.Length, separator.Length);
            return names.ToString();
        }

        protected String GenerateValuesPart(TableInfo table, DataRow row)
        {
            var values = new StringBuilder();
            foreach (var column in table.Columns)
            {
                var valueStr = "";
                if (!row[column.Name].ToString().Any() && column.IsNullable)
                    valueStr = "Null";
                else if (column.GetLogicType() == typeof (DateTime) || column.GetLogicType() == typeof (String))
                    valueStr = String.Format("\"{0}\"", row[column.Name]);
                else
                    valueStr = row[column.Name].ToString();
                values.AppendFormat("{0}{1}", valueStr, separator);
            }
            values.Remove(values.Length - separator.Length, separator.Length);
            return values.ToString();
        }

        public String GenerateSelectQueryString(TableInfo table)
        {
            var primaryKey = table.PrimaryKey();
            var orderByPart = ((primaryKey != null) ? String.Format(" ORDER BY [{0}]", primaryKey.Name) : String.Empty);
            return String.Format("SELECT * FROM [{0}]{1}", table.Name, orderByPart);
        }
    }
}
