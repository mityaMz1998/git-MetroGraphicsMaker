using System;
using System.Collections.Generic;


namespace LoadData
{
	public class Storage
	{
		/// <summary>
		/// The sum of bytes stored in this storage.
		/// </summary>
		Int32 m_size = 0;

		/// <summary>
		/// The list of tables, which will be load from DB in this currentThread.
		/// </summary>
		List<TableInfo> m_tables = null;

		public List<TableInfo> Tables
		{ 
			get
			{ 
				return m_tables; 
			}
		}

		public Storage ()
		{
			m_tables = new List<TableInfo>();
		}

		public Storage (List<TableInfo> info)
		{
			m_tables = new List<TableInfo>();
			foreach (var item in info)
				Add(item);
		}

		/// <summary>
		/// Add the information about added table (sum of byte).
		/// </summary>
		/// <param name='element'>
		/// Information about table.
		/// </param>
		public void Add(TableInfo element)
		{
			m_size += element.Size();
			m_tables.Add(element);
		}

		/// <summary>
		/// Sum of bytes in this storage.
		/// </summary>
		public Int32 Size()
		{
			return m_size;
		}
	}
}
