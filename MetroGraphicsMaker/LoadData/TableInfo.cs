using System;
using System.Collections.Generic;

namespace LoadData
{
	public class TableInfo : IComparable
	{
		#region IComparable implementation
		// не вызывается в процессе работы!
		public int CompareTo(object obj)
		{
			var table = obj as TableInfo;

			String tmpStr_1 = Name,
			tmpStr_2 = table.Name;

			foreach (var field in Fields.Values)
				tmpStr_1 += field.ToString();

			foreach (var field in table.Fields.Values)
				tmpStr_2 += field.ToString();

			Console.WriteLine("({0}).CompareTo({1})", Name, table.Name);

			return tmpStr_1.CompareTo(tmpStr_2);
		}

		#endregion

/*
		overload
		public override bool operator== (TableInfo arg)

		public overload bool operator== (TableInfo arg)
		{


		}
*/

		public override bool Equals(Object obj)
		{
			if (obj == null)
				return false;

			var tmpObj = obj as TableInfo;
			// if cast failed return false
			if (tmpObj == null)
				return false;

			// Console.WriteLine("[{0}] eq [{1}]", Name, tmpObj.Name);

			if (Name != tmpObj.Name)
				return false;

			bool flag = true;
			foreach (var field in Fields)
				// Fields.ContainsKey(tmpField.Key) & 
				//routeHasBeenRepairedInDepot = routeHasBeenRepairedInDepot && tmpObj.Fields.ContainsValue(field.Value);
				flag &= tmpObj.Fields.ContainsValue(field.Value);
			return flag;
		}

		// по умолчанию спецификатор доступа -- private
		Int32 m_size = 0;
        
        /// <summary>
        /// Имя данной таблицы.
        /// </summary>
        public String Name       = String.Empty;

        /// <summary>
        /// 
        /// </summary>
		public String Required   = String.Empty;

        /// <summary>
        /// Первичный ключ данной таблицы.
        /// </summary>
		public String PrimaryKey = String.Empty;

        /// <summary>
        /// Колличество строк в данной таблице.
        /// </summary>
		public Int32 RowCounter  = 0;

        /// <summary>
        /// Коллекция (словарь) объектов, содержащих информацию о данной таблице.
        /// </summary>
        public Dictionary<String, FieldInfo> Fields;


        public TableInfo(String tableName)
        {
            Name = tableName;
            Fields = new Dictionary<String, FieldInfo>();
        }

		public Int32 Size()
		{
			return m_size * RowCounter;
		}

        /// <summary>
        /// Добавляем описание поля в описание таблицы.
        /// </summary>
        /// <param name="field">Информация о поле таблицы.</param>
		public void AddField(FieldInfo field)
		{
			m_size += field.Size();
			Fields.Add(field.Name, field);
		}

        /// <summary>
        /// Получаем информацию о поле таблицы по его имени.
        /// </summary>
        /// <param name="name">Имя поля таблицы.</param>
        /// <returns>Информация о поле таблицы.</returns>
		public FieldInfo GetField(String name)
		{
			return Fields[name];
		}
	}
}
