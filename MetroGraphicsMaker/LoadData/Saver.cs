using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using ADOX;
using Core;
using LoadData;
using Messages;

namespace WpfApplication1.LoadData
{
    class Saver
    {
        protected Storage storage;

        public static ConcurrentDictionary<String, DataTable> m_syncData;

        public Saver(Storage s, ConcurrentDictionary<String, DataTable> data)
        {
            storage = s;
            m_syncData = data;
        }

        private void threadLoadToDatabase(Object data)
        {
            var connectionString = GenerateConnectionString(GenerateDatabaseName());
            var tableNames = new List<String> { "Таблица типов ремонта", "Таблица депо"};//, "Таблица маршрутов", "Станции" };
            var tableInfos = GetTableInfos(data, tableNames);

            CreateDatabaseFile(connectionString, tableInfos);
            FillDatabaseFile(connectionString, tableInfos);
        }

        private String GenerateConnectionString(String newName)
        {
            return String.Format("Provider=Microsoft.Jet.OLEDB.{1}.0; data source = {0}; Jet OLEDB:Engine Type=5", newName, IntPtr.Size == 4 ? 4 : 12);
        }

        private String GenerateDatabaseName()
        {
            var pathSeparator = System.IO.Path.DirectorySeparatorChar;
            var directoryName = String.Format("{0}{1}backup_db", Environment.CurrentDirectory, pathSeparator);
            if (!System.IO.Directory.Exists(directoryName))
                System.IO.Directory.CreateDirectory(directoryName);
            return String.Format("{0}{1}{2}.mdb", directoryName, pathSeparator, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"));
        }

        private void FillDatabaseFile(string connectionString, IEnumerable<TableInfo> tableInfos)
        {
            var connection = new OleDbConnection(connectionString);
            try
            {
                // TODO: Здесть стоит использовать блок using
                // Пытаемся открыть соединение
                connection.Open(); // TODO: Беда тут!!!

                var command = new OleDbCommand { Connection = connection };
                foreach (var table in tableInfos)
                {
                    var names = GenerateNamesPart(table);
                    foreach (DataRow row in m_syncData[table.Name].Rows)
                    {
                        var str = String.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", table.Name, names,
                            GenerateValuesPart(table, row));

                        Logger.Output(str, typeof(Loader).FullName);

                        command.CommandText = str;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exception)
            {
                // непотокобезопасно
                MessageBox.Show(exception.ToString());
                Logger.Output(exception.ToString(),
                    GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            finally
            {
                connection.Close();
            }
        }

        private void CreateDatabaseFile(String connectionString, IEnumerable<TableInfo> tableInfos)
        {
            var catalog = new Catalog();
            catalog.Create(connectionString);

            try
            {
                var tables = tableInfos.Select(tableInfo => GenerateTable(tableInfo, catalog));
                foreach (var table in tables)
                    catalog.Tables.Append(table);
            }
            catch (Exception exception)
            {
                Logger.Output(exception.ToString(),
                    GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            Marshal.FinalReleaseComObject(catalog.ActiveConnection);
            Marshal.FinalReleaseComObject(catalog);
        }

        private IEnumerable<TableInfo> GetTableInfos(object data, IEnumerable<String> tableNames)
        {
            var storage = data as Storage;
            if (storage == null)
                return Enumerable.Empty<TableInfo>();

            return storage.Tables.Where(t => tableNames.Contains(t.Name));
        }

        private String GenerateValuesPart(TableInfo table, DataRow row)
        {
            var separator = ", ";
            var fieldValues = new StringBuilder();
            foreach (var value in table.Fields.Values)
            {
                var valueStr = "";
                if (!row[value.Name].ToString().Any() && value.flgNull)
                    valueStr = "Null";
                else if (value.type == typeof(DateTime) || value.type == typeof(String))
                    valueStr = String.Format("\"{0}\"", row[value.Name]);
                else
                    valueStr = row[value.Name].ToString();
                fieldValues.AppendFormat("{0}{1}", valueStr, separator);
            }
            fieldValues.Remove(fieldValues.Length - separator.Length, separator.Length);
            return fieldValues.ToString();
        }

        private string GenerateNamesPart(TableInfo table)
        {
            var separator = ", ";
            var fieldNames = new StringBuilder();
            foreach (var field in table.Fields.Values)
                fieldNames.AppendFormat("[{0}]{1}", field.Name, separator);
            fieldNames.Remove(fieldNames.Length - separator.Length, separator.Length);
            return fieldNames.ToString();
            //return table.Fields.Values.Select(x => x.Name).Aggregate((current, next) => String.Format("{0}, [{1}]", current, next));
        }


        private Table GenerateTable(TableInfo tableInfo, Catalog catalog)
        {
            var table = new Table { Name = tableInfo.Name };
            foreach (var field in tableInfo.Fields.Values)
            {
                table.Columns.Append(GenerateColumn(field, catalog));
                if (field.flgPK)
                    table.Keys.Append(field.Name, KeyTypeEnum.adKeyPrimary, field.Name);
            }
            return table;
        }

        private Column GenerateColumn(FieldInfo field, Catalog catalog)
        {
            var column = new Column
            {
                Name = field.Name,
                Type = TypeConverter1(field.type)
            };

            //column.Properties["Nullable"].Value = field.flgNull;
            if (field.flgNull)
                column.Attributes = ColumnAttributesEnum.adColNullable;
            if (field.flgPK)
            {
                column.ParentCatalog = catalog;
                column.Properties["AutoIncrement"].Value = true;
            }
            return column;
        }

        public void LoadToDatabase()
        {
            var thread = new Thread(threadLoadToDatabase);
            thread.Start(storage);
            thread.Join();
            MessageBox.Show("Записано");
        }

        private static DataTypeEnum TypeConverter(Type type)
        {
            switch (type.FullName)
            {
                case "System.Boolean": return DataTypeEnum.adBoolean;
                case "System.Byte": return DataTypeEnum.adUnsignedTinyInt;
                case "System.Int16": return DataTypeEnum.adSmallInt;
                case "System.Int32": return DataTypeEnum.adInteger;
                case "System.Single": return DataTypeEnum.adSingle;
                case "System.Double": return DataTypeEnum.adDouble;
                case "System.DateTime": return DataTypeEnum.adDate;
                case "System.String": return DataTypeEnum.adLongVarWChar;
                default:
                    {
                        MessageBox.Show(type.FullName);
                        return DataTypeEnum.adError;
                    }
                // default : return DataTypeEnum.adEmpty;
            }
        }

        private DataTypeEnum TypeConverter1(Type type)
        {
            // ToString() и switch-case!!!

            if (type == typeof(Boolean))
                return DataTypeEnum.adBoolean;

            if (type == typeof(Char))
                return DataTypeEnum.adChar;

            if (type == typeof(SByte))
                return DataTypeEnum.adTinyInt;
            if (type == typeof(Int16))
                return DataTypeEnum.adSmallInt;
            if (type == typeof(Int32))
                return DataTypeEnum.adInteger;
            if (type == typeof(Int64))
                return DataTypeEnum.adBigInt;

            if (type == typeof(Byte))
                return DataTypeEnum.adUnsignedTinyInt;
            if (type == typeof(UInt16))
                return DataTypeEnum.adUnsignedSmallInt;
            if (type == typeof(UInt32))
                return DataTypeEnum.adUnsignedInt;
            if (type == typeof(UInt64))
                return DataTypeEnum.adUnsignedBigInt;

            if (type == typeof(Single))
                return DataTypeEnum.adSingle;
            if (type == typeof(Double))
                return DataTypeEnum.adDouble;

            if (type == typeof(String))
                return DataTypeEnum.adVarWChar; // adVarChar;

            if (type == typeof(DateTime)) // Здесь могут быть проблемы
                return DataTypeEnum.adDate;
            // ...

            return DataTypeEnum.adEmpty;
        }
    }
}
