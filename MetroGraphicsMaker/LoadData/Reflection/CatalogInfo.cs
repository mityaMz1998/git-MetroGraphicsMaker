using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using ADOX;
using Messages;

namespace WpfApplication1.LoadData.Reflection
{
    internal class CatalogInfo : NamedEntity
    {
        public List<TableInfo> Tables { get; protected set; }

        protected static ConcurrentDictionary<String, DataTable> syncData;

        protected String oldConnectionString;

        protected String OldConnectionString 
        {
            get
            {
                return oldConnectionString ?? (oldConnectionString = GenerateConnectionString(Name));
            }
        }

        protected String newConnectionString;

        protected String NewConnectionString
        {
            get
            {
                return newConnectionString ?? (newConnectionString = GenerateConnectionString(GenerateDatabaseName(path)));
            }
        }

        protected String GenerateConnectionString(String p)
        {
            return String.Format("Provider=Microsoft.Jet.OLEDB.{1}.0; data source = {0}; Jet OLEDB:Engine Type=5", p, IntPtr.Size == 4 ? 4 : 12);
        }

        protected FileInfo path;

        public CatalogInfo(FileInfo path, ConcurrentDictionary<String, DataTable> data)
        {
            Tables = new List<TableInfo>();

            Name = path.ToString();

            this.path = path;

            syncData = data;

            MakeSnapshot();
        }

        protected InformationProvider informationProvider;

        protected void GenerateCatalog()
        {
            var catalog = new Catalog();
            catalog.Create(NewConnectionString);

            try
            {
                Logger.Output(Name, GetType().FullName);
                var tables = Tables.Select(table => table.GenerateTable(catalog));
                foreach (var table in tables)
                    catalog.Tables.Append(table);
            }
            catch (Exception exception)
            {
                Logger.Output(exception.ToString(), GetType().FullName + "." + MethodBase.GetCurrentMethod().Name);
            }

            Marshal.FinalReleaseComObject(catalog.ActiveConnection);
            Marshal.FinalReleaseComObject(catalog);

//            catalog.ActiveConnection = null;
//            catalog = null;
        }

        protected void FillDataBase()
        {
            var connection = new OleDbConnection(NewConnectionString);
            try
            {
                connection.Open();

//                Tables.Select(table => syncData[table.Name].Rows.AsParallel().AsOrdered().OfType<DataRow>().Select(row =>
//                {
//                    command.CommandText = writer.GenerateInsertQueryString(table, row);
//                    return command.ExecuteNonQuery();
//                }));

                var command = new OleDbCommand { Connection = connection };
                var writer = new QueryWriter();
                foreach (var table in Tables)
                {
                    foreach (DataRow row in syncData[table.Name].Rows)
                    {
                        command.CommandText = writer.GenerateInsertQueryString(table, row);
                        Logger.Output(command.CommandText, GetType().FullName);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exception)
            {
                // непотокобезопасно
                MessageBox.Show(exception.ToString());
                Logger.Output(exception.ToString(), GetType().FullName + "." + MethodBase.GetCurrentMethod().Name);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        protected void CreateSchema()
        {
            try
            {
                foreach (var table in Tables)
                {
                    var dataTable = table.GenerateTable();
                    syncData.GetOrAdd(table.Name, dataTable);
                }
            }
            catch (Exception exception)
            {
                Logger.Output(exception.ToString(), GetType().FullName + "." + MethodBase.GetCurrentMethod().Name);
            }
        }

        protected String GenerateDatabaseName(FileInfo fullPath) // Environment.CurrentDirectory
        {
            var pathSeparator = Path.DirectorySeparatorChar;
            var directoryName = String.Format("{0}{1}backup_db", fullPath.DirectoryName, pathSeparator);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            return String.Format("{0}{1}{2}.mdb", directoryName, pathSeparator, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"));
        }

        public void Save()
        {
            GenerateCatalog();
            FillDataBase();
        }

        public void MakeSnapshot()
        {
            var connection = new OleDbConnection(OldConnectionString);
            try
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();

                connection.Open();
                // =========================================================================================
                informationProvider = new InformationProvider(connection);

                Tables.AddRange(informationProvider.GetTablesNames());

                foreach (var table in Tables)
                {
                    var columns = informationProvider.GetColumns(table.Name);
                    table.Columns.AddRange(columns);

//                    if (table.Name.Equals("~TMPCLP424311"))
//                        Logger.Output(String.Format("field '{0}' added in table '{1}'", tmpField.Name, table.Name), GetType().FullName);

                    var primaryKeys = informationProvider.GetPrimaryKeys(table.Name);
                    foreach (var primaryKey in primaryKeys)
                    {
                        var column = table.GetColumnByName(primaryKey);
                        if (column != null)
                            column.IsPrimaryKey = true;
                    }
                }
                // =========================================================================================
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        public void FillDatabase()
        {
            var connection = new OleDbConnection(OldConnectionString);
            try
            {
                connection.Open();
                // =========================================================================================
                var writer = new QueryWriter();
                foreach (var table in Tables)
                    new OleDbDataAdapter(writer.GenerateSelectQueryString(table), connection).Fill(syncData[table.Name]);
                // =========================================================================================
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        public void Load()
        {
           // MakeSnapshot();
            CreateSchema();
            FillDataBase();
        }
    }
}
