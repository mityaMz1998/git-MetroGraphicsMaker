using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ADOX;
using Messages;

namespace WpfApplication1.LoadData.Reflection
{
    class TableInfo : NamedEntity
    {
        public List<ColumnInfo> Columns { get; protected set; }

        public ColumnInfo GetColumnByName(String columnName)
        {
            return Columns.SingleOrDefault(x => x.Name == columnName);
        }

        public TableInfo(String tableName)
        {
            Columns = new List<ColumnInfo>();

            Name = tableName;
        }

        public Table GenerateTable(Catalog catalog)
        {
            var table = new Table { Name = Name };
            Logger.Output(Name, GetType().FullName);
            foreach (var column in Columns)
            {
                table.Columns.Append(column.GenerateColumn(catalog));
                if (column.IsPrimaryKey)
                    table.Keys.Append(column.Name, KeyTypeEnum.adKeyPrimary, column.Name);
            }
            Logger.Output(Name + " out", GetType().FullName);
            return table;
        }

        public DataTable GenerateTable()
        {
            Logger.Output(Name, GetType().FullName);
            var table = new DataTable(Name);
            table.Columns.AddRange(Columns.Select(column => column.GenerateColumn()).ToArray());
            return table;
        }

        public ColumnInfo PrimaryKey()
        {
            return Columns.SingleOrDefault(x => x.IsPrimaryKey);
        }
    }
}
