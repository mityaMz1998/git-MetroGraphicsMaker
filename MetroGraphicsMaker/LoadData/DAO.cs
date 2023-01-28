/** 
 * @organization Departament "Control and Informatics in Technical Systems" (CITS@MIIT)
 * @author Konstantin Filipchenko 
 * @e-mail konstantin-649@mail.ru, konstantin.filipchenko@gmail.com
 * @version 0.1 
 * @date 2012.12.17
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Core;
using Messages;

namespace LoadData
{
	public class DAO
	{

		List<String> columnsNamesForTables = null;
		/*
		public List<String> TablesHeader = null;
		public List<String> ColumnsHeader = null;
		public List<String> PrimaryKeysHeader = null;
        */
        static Dictionary<OleDbType, Type> m_accessDataTypes = null;

		String dbName = String.Empty;
		String dbUser = "admin";
		String dbPass = String.Empty;

		List<TableInfo> information = null;
		List<String> schemaElements = null;

		public DAO (String dbName)
		{
			information = new List<TableInfo>();
			schemaElements = new List<String>();
			columnsNamesForTables = new List<String>();

			this.dbName = dbName;
            Logger.Output(String.Format("DAO constructed:\t [{0}]", dbName), GetType().FullName);
			/*
            TablesHeader = new List<String>();
            ColumnsHeader = new List<String>();
            PrimaryKeysHeader = new List<String>();
            */
			dictionaryInit();
		}

	    public void LoadToDatabase(DataSet data, String tableName)
	    {
            var connection = new OleDbConnection(
                String.Format("Provider=Microsoft.JET.OLEDB.4.0;data source={0};User Id={1};Password={2};", dbName, dbUser, dbPass));
	        try
	        {
	            // и пытаемся открыть его
	            connection.Open();
	            information[0].Fields["Код"].Name = "";
	            foreach (var tableInfo in information)
	            {
	                var sb = new StringBuilder(String.Format("INSERT INTO [{0}] (", tableInfo.Name));
	                foreach (var field in tableInfo.Fields)
	                    sb.AppendFormat("[{0}], ", field.Value.Name);
	                sb.Remove(sb.Length - 2, 2);
	                sb.Append(") VALUES(");
	               // foreach (var VARIABLE in tableName)
	               // {
	                    
	                //}
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(");");
	            }

                // Insert Into Employee (UserName, fname, sname, email) values ('amar','Amar Prakash','Jaiswal','amar@gmail.com')";
	            foreach (var table in Loader.m_syncData)
	            {
                    // tableInfo
                    var commandText = String.Format("INSERT INTO [{0}]() VALUES()", table.Key);
                    new OleDbCommand(commandText, connection).ExecuteNonQuery();
	            }

	        }
	        finally
	        {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
	        }
	    }


	    public DAO (String dbName, String dbUser, String dbPass)
		{
			information = new List<TableInfo>();
			schemaElements = new List<String>();
			columnsNamesForTables = new List<String>();

			this.dbName = dbName;
			this.dbUser = dbUser;
			this.dbPass = dbPass;
			/*
			TablesHeader = new List<String>();
			ColumnsHeader = new List<String>();
			PrimaryKeysHeader = new List<String>();
            */
			dictionaryInit();
		}

        Type ConvertTypes(Byte code)
        {
            Type type = null;

            switch (code)
            {
                case 0: type = typeof(Nullable);
                    break;

                case 2: type = typeof(Int16);
                    break;
                case 3: type = typeof(Int32);
                    break;
                case 4: type = typeof(Single);
                    break;
                case 5: type = typeof(Double);
                    break;

                case 6:
                case 14:
                case 131:
                case 139: type = typeof(Decimal);
                    break;

                case 7:
                case 64:
                case 133:
                case 135: type = typeof(DateTime);
                    break;

                case 9:
                case 12:
                case 13:
                case 138: type = typeof(Object);
                    break;

                case 10: type = typeof(Exception);
                    break;
                case 11: type = typeof(Boolean);
                    break;

                case 16: type = typeof(SByte);
                    break;
                case 17: type = typeof(Byte);
                    break;
                case 18: type = typeof(UInt16);
                    break;
                case 19: type = typeof(UInt32);
                    break;
                case 20: type = typeof(Int64);
                    break;
                case 21: type = typeof(UInt64);
                    break;

                case 72: type = typeof(Guid);
                    break;

                case 128: 
                case 204: 
                case 205: type = typeof(Byte[]);
                    break;

                case 129:
                case 130:
                case 200:
                case 201:
                case 202:
                case 203: type = typeof(String);
                    break;

                case 134: type = typeof(TimeSpan);
                    break;

                default: type = null;
                    break;
            }

            if (type == null)
                throw new ArgumentException("Undefined type code.");

            return type;
        }

        Type ConvertTypes(OleDbType oleDbType)
        {
            Type type = ConvertTypes(Convert.ToByte(oleDbType));
            /*
            switch (oleDbType)
            {
                case OleDbType.Empty: type = typeof(Nullable);
                    break;

                 // ...
                
                default: type = null;
                    break;
            }
            */
            if (type == null)
                throw new ArgumentException("Undefined OleDbType");

            return type;
        }

		// по умолчанию спецификатор доступа -- private
		static void dictionaryInit()
		{/**
          * Типы данных в MS Access:
          *     Text                    = Текстовый  = до    255 знаков
          *     Memo                    = Поле MEMO  = до 64 000 знаков
          *     Number                  = Числовой   = 1, 2, 4 или 8 байт                      
          *         Byte         - Байт               - 1 байт
          *         Integer      - Целое              - 2 байта
          *         Long Integer - Длинное целое      - 4 байта
          *         Single       - С плавающей точкой - 4 байта -  7 десятичных позиций
          *         Double       - С плавающей точкой - 8 байт  - 15 десятичных позиций                
          *     Date/Time               = Дата/Время = 8 байт
          *     Yes/No                  = Логический = 1 бит (0 или -1)
          *     AutoNumber (Counter v2) = Счётчик    = 4 байта
          *     Currency                = Денежный   = 8 байт
          */
            /*
            m_accessDataTypes = new Dictionary<Int16, String>()
            {
                {2, "int"},     //"Целое"
                {3, "int"},     //"Длинное целое"
                {4, "float"},   //"Одинарное с плавающей точкой"
                {7, "datetime"},//"Дата и время"
                {11, "bit"},    //"Логическое"
                {17, "byte"},   //"Байт"
                {130, "text"}   //"Текст"
            };
            */
            m_accessDataTypes = new Dictionary<OleDbType, Type>()
            {
                { OleDbType.SmallInt,        typeof(Int16)    },
                { OleDbType.Integer,         typeof(Int32)    },
                { OleDbType.Single,          typeof(Single)   },
                { OleDbType.Date,            typeof(DateTime) },
                { OleDbType.Boolean,         typeof(Boolean)  },
                { OleDbType.UnsignedTinyInt, typeof(Byte)     },
                { OleDbType.WChar,           typeof(String)   }
                
            };
		}

		/// <summary>
		/// Получение информации о структуре базы данных.
		/// </summary>
		/// <returns>
		/// Список объектов хранящих, информацию об объектах.
		/// </returns>
		//public void SnapshotDB()
		public List<TableInfo> SnapshotDB()
		{
			Logger.Output("DAO.SnapshotDB() execute", GetType().FullName);
			// Создаём соединение с базой данных
			var connection = new OleDbConnection(String.Format("Provider=Microsoft.JET.OLEDB.4.0;data source={0};User Id={1};Password={2};", dbName, dbUser, dbPass));
		    try
		    {
		        Logger.Output("DAO.SnapshotDB(): try open connection", GetType().FullName);
		        // и пытаемся открыть его
		        connection.Open(); // TODO: Тут беда!!!!
		        //DataTable tables -- служебная таблица, содержащая список таблиц
		        var tables = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] {null, null, null, "TABLE"});
		        // Создание пустых записей о таблицах, отличающихся только именами таблиц.
		        // сколько имён (строк в служебной таблице), столько и таблиц
		        foreach (DataRow row in tables.Rows)
		        {
		            // в служебной таблице tables имя таблицы хранится в третьем столбце
		            // |TABLE_CATALOG | TABLE_SCHEMA (имя БД) | TABLE_NAME (имя таблицы) | ...
		            // поэтому и индекс 2
		            try
		            {
		                // можно и так, и эдак: и по имени столбца, и по его индексу
		                // information.Add(new TableInfo(row[2].ToString()));
		                information.Add(new TableInfo(row["TABLE_NAME"].ToString()));
		            }
		            catch (IndexOutOfRangeException ex)
		            {
		                Console.WriteLine(ex);
		            }
		        }

		        // заполняем информацию о таблицах
		        foreach (var table in information) // куча раз!!! ужас!!!
		        {
		            //-------- Получаем  информацию о колличестве строк в таблице table.name --------------
		            var result = new DataTable();
		            // инстанцируем безымянный объект класса OleDbDataAdapter, посредством которого
		            // исполняем запрос на подсчёт колличества строк в таблице с именем table.name
		            // результирующую таблицу записываем в переменную result

		            new OleDbDataAdapter(String.Format("SELECT COUNT(*) FROM [{0}]", table.Name), connection).Fill(result);
		            try
		            {
		                // так как на запрос вернётся таблица содержащая только одну ячейку (0; 0), 
		                // то её мы и вытаскиваем из результирующей таблицы
		                table.RowCounter = Convert.ToInt32(result.Rows[0][0].ToString());
		                //table.RowCounter = Convert.ToInt32(result.Rows[0].ItemArray[0].ToString());
		                // получаем список столбцов таблицы с именем table.name
		            }
		            catch (Exception ex) // нужен более специализированный тип
		            {
		                Console.WriteLine(ex);
		            }
		            //------------------------------------------------------------------------------------
		            //------------ Получаем список имён столбцов таблицы table.name ----------------------
		            var fields = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[]
		            {
		                null,
		                null,
		                table.Name,
		                null
		            });
		            //------------------------------------------------------------------------------------
		            //------------ получаем список первичных ключей для таблицы table.name ---------------
		            var primaryKeys = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[]
		            {
		                null,
		                null,
		                table.Name
		            });
		            //------------------------------------------------------------------------------------
		            foreach (DataRow row in fields.Rows)
		            {
		                var tmpField = new FieldInfo(row["COLUMN_NAME"].ToString());
		                tmpField.DefaulValue = row["COLUMN_DEFAULT"].ToString();
		                tmpField.flgNull = Convert.ToBoolean(row["IS_NULLABLE"].ToString());
		                //tmpField.type = Microsoft.AnalysisServices.OleDbTypeConverter.Convert((OleDbType)Convert.ToInt32(row["DATA_TYPE"].ToString()));
		                //m_accessDataTypes[(OleDbType)Convert.ToInt32(row["DATA_TYPE"].ToString())];
		                try
		                {
		                    tmpField.type = ConvertTypes(Convert.ToByte(row["DATA_TYPE"].ToString()));
		                }
		                catch (Exception e)
		                {
		                    tmpField.type = typeof (Object);
		                    Logger.Output(String.Format("field name = {0} --> {1}", tmpField.Name, e.ToString()),
		                        GetType().FullName);
		                }
		                tmpField.Description = row["DESCRIPTION"].ToString();
		                table.AddField(tmpField);

		                if (table.Name.Equals("~TMPCLP424311"))
		                    Logger.Output(String.Format("field '{0}' added in table '{1}'", tmpField.Name, table.Name),
		                        GetType().FullName);
		            }

		            foreach (DataRow row in primaryKeys.Rows)
		            {
		                table.Fields[row["COLUMN_NAME"].ToString()].flgPK = true;
		                table.PrimaryKey = row["COLUMN_NAME"].ToString();
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        Console.WriteLine(ex);
		    }
		    finally
		    {
                if (connection.State != ConnectionState.Closed)
		            connection.Close();
		    }
		    return information;
		}

		public List<TableInfo> InputFromXml(String filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");
			
			try
			{
				var document = XDocument.Load(filename);
				
				foreach (var table in document.Element("database").Elements())
				{
					var tmpTable = new TableInfo(table.Attribute("name").Value);
					//tmpTable.Required = Convert.ToBoolean(table.Attribute("required").Value);
					foreach (var field in table.Elements())
					{
						var tmpField = new FieldInfo(field.Attribute("name").Value);
                        tmpField.type = Type.GetType(field.Attribute("type").Value);
                        //Logger.output(String.Format("Тип поля [{2}].[{0}] = {1}", tmpField.Name, tmpField.type.FullName, tmpTable.Name), GetType().FullName);
						tmpField.flgPK = Convert.ToBoolean(field.Attribute("isPK").Value);
						// @TODO: Возможно стоит сделать ссылку на tmpField вместо строки имени
                        if (tmpField.flgPK)
							tmpTable.PrimaryKey = tmpField.Name;
						tmpField.flgNull = Convert.ToBoolean(field.Attribute("null").Value);
						tmpTable.AddField(tmpField);
					}
					information.Add(tmpTable);
				}
			}
			catch (Exception ex)
			{
				Logger.Output(String.Format("Ошибка при чтении {0}. Произошла исключительная ситуация {1}", filename, ex), GetType().FullName);
			}
			return information;
		}

		/// <summary>
		/// Выводим информацию о структуре базы данных в xml-файл используя классы XDocument, XElement и XAttribute.
		/// </summary>
		/// <param name="filename">Имя файла, в который выводим</param>
		/// <param name="cssFlag">Флаг, показывающий какую структуру применять: false -- пригодная для чтения ПО, false -- пригодная для отображения браузером и чтения человеком.</param>
		public void OutputToXml(String filename, Boolean cssFlag)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");
			
			try
			{
				var xmlDocument = new XDocument();
                if (cssFlag)
                    xmlDocument.Add(new XProcessingInstruction("xml-stylesheet", "type=\"text/css\" href=\"table_xml.css\""));
				
                var root = new XElement("database", new XAttribute("name", dbName));
				foreach (var table in information)
				{
					var element = new XElement("table", new XAttribute("name", table.Name));
                    // Console.WriteLine("  --> table '{0}' has {1} fields", table.Name, table.Fields.Count);
					foreach (var field in table.Fields.Values)
					{
						element.Add(new XElement("field",
                                        new XAttribute("name", field.Name),
                                        new XAttribute("type", field.type.FullName),
                                        new XAttribute("isPK", field.flgPK.ToString().ToLower()),
                                        new XAttribute("null", field.flgNull.ToString().ToLower())
						));
					}
					root.Add(element);
				}
				
				xmlDocument.Add(root);
				xmlDocument.Save(filename);
			}
			catch (OutOfMemoryException ex)
			{
                Logger.Output(String.Format("Недостаточно памяти! {0}", ex), GetType().FullName);
			}
		}
		
		/// <summary>
        ///  самописный велосипед
		/// </summary>
		/// <param name="filename"></param>
		public void OutputToFile(String filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			/* Возможно гененрируется исключение, хотя спорно 
             * пример msdn не содержит try-catch блока */
			using (var file = new System.IO.StreamWriter(filename))
			{
				file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
					"\n\n<database name = \"{0}\">", dbName);
				foreach (var table in information)
				{
					file.WriteLine("\t<table name=\"{0}\" required=\"{1}\">", table.Name, table.Required);
					foreach (var field in table.Fields.Values)
					{
						file.WriteLine("\t\t<field name=\"{0}\" type=\"{1}\" isPK=\"{2}\" null=\"{3}\"/>", 
						               field.Name, 
						               field.type.FullName,
						               field.flgPK.ToString().ToLower(),
						               field.flgNull.ToString().ToLower());
					}
					file.WriteLine("\t</table>\n");
				}
				file.WriteLine("</database>");
			}
		}
	
		List<String> LoadSchemaFromXml(String filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");
			
			// оставить?
			try
			{
				var document = XDocument.Load(filename);
				var ns = document.Root.Name.Namespace;
                var name = XName.Get("sequence", ns.NamespaceName);

                if (new XElement(ns + "sequence").Name == name)
                    MessageBox.Show("Угу куку!");

			    var elements =
			        document.Descendants(ns + "element")
			            .Where(element => element.Parent.Name == (new XElement(ns + "sequence")).Name);
				foreach (var element in elements)
				{
				    schemaElements.Add(element.FirstAttribute.Value);
				}
			}
			catch (System.IO.FileNotFoundException ex)
			{
				Logger.Output(String.Format("Файл {0} не был найден. {1}", filename, ex), GetType().FullName);
			}

			return schemaElements;
		}

		public void OutputHTML(String filename)
		{
			const string m_sysTabs = "system_tables.xml";
            //String m_sysCols = "system_fields.xml";
			//String m_sysKeys = "system_prymary_keys.xml";

			if (filename == null)
				throw new ArgumentNullException("filename");
			
			using (var file = new System.IO.StreamWriter(filename))
			{
				var text = "<!DOCTYPE html>\n<html>\n\t<head>" +
					"\n\t\t<meta http-equiv='Content-Language' content='ru, en'>" +
					"\n\t\t<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>" +
					"\n\n\t\t<link rel='stylesheet' type='text/css' href='table_style.css'> " +
					"\n\n\t\t<script type='text/javascript' src='jquery.js'></script>" +
					"\n\t\t<script type='text/javascript' src='tables.js'></script>" +
					"\n\t</head>";
				
				text += String.Format("\n\n\t<body>" +
					"\n\t\t<div class='dbSource'>Источник: {0};</div>" +
					"\n\t\t<div class='dbName'>База данных: {1}</div>",
				                      filename,
				                      dbName);
				
				file.WriteLine(text);

				file.Write("<table><caption>TABLES</caption><tr>");

				columnsNamesForTables = LoadSchemaFromXml(m_sysTabs);
				foreach (var columnName in columnsNamesForTables)
					file.Write("<th>{0}</th>", columnName);
				file.Write("</tr>");

				var connection = new OleDbConnection(
						String.Format("Provider=Microsoft.JET.OLEDB.4.0;data source={0};User Id={1};Password={2};", 
				              dbName, 
				              dbUser, 
				              dbPass)
				);
				try
				{
					// и пытаемся открыть его
					connection.Open();
					//DataTable tables -- служебная таблица, содержащая список таблиц
					var tables = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[]{null,null,null,"TABLE"});
				
					foreach (DataRow row in tables.Rows)
					{
						file.Write("<tr>");
						foreach (var cell in row.ItemArray)
							file.Write("<td>{0}</td>", cell.ToString());
						file.Write("</tr>");
					}
					file.Write("</table>");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
				// TODO: стоит обдумать -- нужно что-то вроде final-блока
				connection.Close();

				/*
				var columnsNamesForColumns = LoadSchemaFromXml(m_sysCols);
				
				foreach (var table in information)
				{
					file.Write("<table><caption>FIELDS OF TABLE {0}</caption>", table.name);
					file.Write("<thead><tr>");
					foreach (var columnName in columnsNamesForColumns)
					{
						file.Write("<th>{0}</th>", columnName);
					}
					file.Write("</tr></thead><tbody><tr><td /><td /><td />");
					foreach (var field in table.Fields.Values)
					{
						file.Write("<td>{0}</td><td /><td /><td />", field.name, field.type);
					}
					file.Write("</tr></tbody>");
				}
				*/
				// дописать!!!
			}
		}

		public Boolean CheckSchema(String filename)
		{
            Logger.Output("DAO.CheckSchema", GetType().FullName);
			
            var standart = InputFromXml(filename); // по-моему тут беда... information не заполняется ни чем
			//SnapshotDB();
			var flag = standart.Aggregate(true, (current, table) => current & information.Contains(table));

		    information.Clear();

			Logger.Output(String.Format("{1}овпадают! [{0}]", filename, flag ? "С" : "Не с"), GetType().FullName);
			
            return flag;
		}
	}
}

