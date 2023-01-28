using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace WpfApplication1.LoadData.Reflection
{
    class InformationProvider
    {
        protected OleDbConnection connection;

        public InformationProvider(OleDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            this.connection = connection;
        }

        public IEnumerable<TableInfo> GetTablesNames()
        {
            var tables = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new Object[] { null, null, null, "TABLE" });
            if (tables == null)
                return Enumerable.Empty<TableInfo>();

            return tables.Rows.OfType<DataRow>().Where(row => row["TABLE_NAME"].ToString() == "Таблица типов ремонта").Select(row => new TableInfo(row["TABLE_NAME"].ToString()));
        }

        public IEnumerable<ColumnInfo> GetColumns(String tableName)
        {
            // в служебной таблице tables имя таблицы хранится в третьем столбце
            // |TABLE_CATALOG | TABLE_SCHEMA (имя БД) | TABLE_NAME (имя таблицы) | COLUMN_NAME
            // поэтому и индекс 2
            // можно и так, и эдак: и по имени столбца, и по его индексу
            // information.Add(new TableInfo(row[2].ToString()));

            var columns = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new Object[] { null, null, tableName, null });
            if (columns == null)
                return Enumerable.Empty<ColumnInfo>();

            return columns.Rows.OfType<DataRow>().Select(row => new ColumnInfo(row));
        }

        public IEnumerable<String> GetPrimaryKeys(String tableName)
        {
            var primaryKeys = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new Object[] { null, null, tableName });
            if (primaryKeys == null)
                return Enumerable.Empty<String>();

            return primaryKeys.Rows.OfType<DataRow>().Select(row => row["COLUMN_NAME"].ToString()); 
        }
    }
}
