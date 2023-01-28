using System.Collections.Generic;

namespace LoadData
{
	public sealed class SortBySize : IComparer<TableInfo>
	{
		int IComparer<TableInfo>.Compare(TableInfo x, TableInfo y)
		{
			return (y.Size() - x.Size());
		}
	}
}
