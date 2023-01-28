using System;
using System.Data;
using ADOX;
using Messages;

namespace WpfApplication1.LoadData.Reflection
{
    class ColumnInfo : NamedEntity
    {
        public Boolean IsPrimaryKey { get; set; }

        public Boolean IsNullable { get; protected set; }

        protected String defaultValue = String.Empty;

        public String DefaultValue
        {
            get
            {
                return defaultValue;
            }
            protected set
            {
                if (value == null)
                    return;

                var trimmed = value.Trim();
                if (trimmed.Equals(String.Empty))
                    return;

                defaultValue = trimmed;
            }
        }

        protected String description = String.Empty;

        public String Description
        {
            get
            {
                return description;
            }
            protected set
            {
                if (value == null)
                    return;

                var trimmed = value.Trim();
                if (trimmed.Equals(String.Empty))
                    return;

                description = trimmed;
            }
        }

        protected Type type;

        protected IDbTypeConverter converter;

        public Type GetLogicType()
        {
            return type;
        }

        protected Type GetLogicType(Byte code)
        {
            return converter.Convert((DataTypeEnum) code);
        }

        public DataTypeEnum GetDomainType()
        {
            return converter.Convert(type);
        }

        public ColumnInfo(String name, Boolean isPrimaryKey = false, Boolean isNullable = false, IDbTypeConverter aConverter = null)
        {
            Name = name;
            IsPrimaryKey = isPrimaryKey;
            IsNullable = isNullable;
            converter = aConverter ?? new SwitchDbTypeConverter();
        }

        public ColumnInfo(DataRow row)
        {
            converter = new SwitchDbTypeConverter();

            Name = row["COLUMN_NAME"].ToString();
            DefaultValue = row["COLUMN_DEFAULT"].ToString();
            Description = row["DESCRIPTION"].ToString();
            IsNullable = Convert.ToBoolean(row["IS_NULLABLE"].ToString());
            try
            {
                var byteCode = Convert.ToByte(row["DATA_TYPE"].ToString());
                type = GetLogicType(byteCode);
                Logger.Output(String.Format("{0} {1} ({2})", Name, type.FullName, byteCode ), GetType().FullName);
            }
            catch (Exception e)
            {
                type = typeof (Object);
                Logger.Output(String.Format("field name = {0} --> {1}", Name, e), GetType().FullName);
            }
            //tmpField.type = Microsoft.AnalysisServices.OleDbTypeConverter.Convert((OleDbType)Convert.ToInt32(row["DATA_TYPE"].ToString()));
            //m_accessDataTypes[(OleDbType)Convert.ToInt32(row["DATA_TYPE"].ToString())];
        }

        public Column GenerateColumn(Catalog catalog)
        {
            var column = new Column
            {
                Name = Name,
                Type = GetDomainType(),
                ParentCatalog = catalog,
                DefinedSize = 100
            };

//            if (GetLogicType() == typeof (String))
//            {
//                column.DefinedSize = 100;
//            }

            Logger.Output(String.Format("{1} {0} {2}", column.Type, column.Name, column.DefinedSize), GetType().FullName);

            if (IsNullable)
                column.Attributes = ColumnAttributesEnum.adColNullable;

            if (IsPrimaryKey)
            {
           //     column.ParentCatalog = catalog;
                column.Properties["AutoIncrement"].Value = true;
            }

            column.Properties["Nullable"].Value = IsNullable;
            column.Properties["Default"].Value = DefaultValue;
            return column;
        }

        public DataColumn GenerateColumn()
        {
            return new DataColumn
            {
                ColumnName = Name, 
                DataType = GetLogicType(), 
                AllowDBNull = IsNullable
            };
        }
    }
}
