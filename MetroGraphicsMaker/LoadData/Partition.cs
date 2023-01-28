using System;
using System.Collections.Generic;
using System.Text;

namespace LoadData
{
	public class Partition
	{
		public Storage Left = new Storage();
		public Storage Right = new Storage();

		public Partition (List<TableInfo> list)
		{
			if (list != null)
			{
				// сортируем по убыванию
				list.Sort(new SortBySize());
				// жадный алгоритм
				foreach (var item in list)
				{
					if (Left.Size() < Right.Size())
					{
						Left.Add(item);
					}
					else
					{
						Right.Add(item);
					}
				}
			}
		}

		override public String ToString()
		{
            StringBuilder result = new StringBuilder(String.Format("Left storage ({0:N}):\n", Left.Size()));
            foreach (TableInfo item in Left.Tables)
                result.AppendFormat("\t{0}\t({1:N})\n", item.Name, item.Size());

            result.AppendFormat("\nRight storage ({0:N}):\n", Right.Size());
			foreach (TableInfo item in Right.Tables)
                result.AppendFormat("\t{0}\t({1:N})\n", item.Name, item.Size());

			return result.ToString();
		}
	}
}
