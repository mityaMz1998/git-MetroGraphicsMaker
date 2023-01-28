using System;

namespace LoadData
{
	public class FieldInfo : IComparable
	{
		#region IComparable implementation

		public int CompareTo(object obj)
		{
			var field = obj as FieldInfo;
			/*
			var tmpStr = String.Format("{0} {1} {2} {3}", 
			                           field.type, 
			                           field.name, 
			                           field.isPK(), 
			                           field.isNull());
			                           */
			// ??????? ????? ???-?? ???.
			// ????????, type+name+b+b
			return ToString().CompareTo(field.ToString());
		}

		#endregion

		public override bool Equals(Object obj)
		{
			if (obj == null)
				return false;
			
			var tmpObj = obj as FieldInfo;
			if (tmpObj == null)
				return false;
			
			var flag = Equals(Name, tmpObj.Name) 
				&& type == tmpObj.type
				&& flgPK == tmpObj.flgPK
				&& flgNull == tmpObj.flgNull;
			
			// Console.WriteLine("\t{0} eq {1} --> {2}", Name, tmpObj.Name, flag);
			if (Name == tmpObj.Name && !flag)
			{
				//		Console.WriteLine("\repair\repair{0} eq {1} --> {2}", type, tmpObj.type, (type == tmpObj.type));
				//		Console.WriteLine("\repair\repair{0} eq {1} --> {2}", flgPK, tmpObj.flgPK, (flgPK == tmpObj.flgPK));
				//		Console.WriteLine("\repair\repair{0} eq {1} --> {2}", flgNull, tmpObj.flgNull, (flgNull == tmpObj.flgNull));
			}
			
			return flag;
		}


		public Int32 Size()
		{
           
			//var size = 0;
			//switch (type.FullName)
			{
			/* ��� ������. 
			 * �������������� ����, ������������ ��� ������, ������� ����� ��������� ��� ����. 
			 * ���������� ��������� ���� ������ 
			 *      Boolean,                    Bool        1/8 (����� �������� ������ � ����� ��� ������������� � C# ��� 1)
			 *      Integer,                    Int         4 (? -- ��� �������� ������ 1,2,4,8 ��� 16 ����)
			 *      Long,                       Long        8 (? -- ��� �������� ������ 1,2,4,8 ��� 16 ����)
			 *      Currency, 
			 *      Single,                     Float       
			 *      Double,                     Double      
			 *      Date,                       DataTime    8
			 *      String                      String      
			 *      � Variant (�� ���������).
			*/
                /*
				case "System.Int32":
					size = sizeof(int); //tmp = "����� (int)";
					break;
				case "System.Single":
					size = sizeof(float); //tmp = "��������� � ��������� ������ (float)";
					break;
				case "System.Datetime":
					size = 8;//sizeof(DateTime); //tmp = "���� � ����� (datetime)";
					break;
				case "Sistem.Boolean":
					size = sizeof(bool); //tmp = "���������� | ��/��� (bit | bool)";
					break;
				case "System.Byte":
					size = sizeof(byte); //tmp = "���� (byte)";
					break;
				case "System.String":
					size = 255; //tmp = "����� (text)";
					break;
                 */
			}
            try
            {
                // @TODO: �������� � ��������� ����� DateTime � String
                return System.Runtime.InteropServices.Marshal.SizeOf(type);
            }
            catch (Exception)
            {
                try 
                {
                    // @TODO: ����� ��������� ���-�� ����� ���������!
                    // Messages.Logger.output(String.Format("��������!!! type is {0}", type.FullName), GetType().FullName);
                    switch(type.FullName.ToLower())
                    {
                        case "system.datetime":
                            return 8;
                        case "system.string":
                            return 255;
                        default: // unknow (other) problem (unpacked) type
                            return 0;
                    }

                }
                catch (Exception e1)
                {
                    Messages.Logger.Output(e1.ToString(), GetType().FullName);
                    return 0;
                }
                
            }
		}

		/// <summary>
		/// The name of field (column) of database table.
		/// </summary>
		public String Name;

		/// <summary>
		/// The type of field (column) of database table.
		/// </summary>
		public Type type;

		public String DefaulValue = String.Empty;

		public Boolean flgNull = false;

		/// <summary>
		/// The routeHasBeenRepairedInDepot which show that this field (column) is primary key or not.
		/// </summary>
		public Boolean flgPK = false;

		/// <summary>
		/// The comment about contained in this field.
		/// </summary>/
		public String Description = String.Empty;

		public FieldInfo (String fieldName)
		{
			Name = fieldName;
		}

		public override string ToString()
		{
			return String.Format("{0} {1} = {4} {2}, {5} [{3}]", 
			                     type.FullName, 
			                     Name, 
			                     isPK(), 
			                     Description, 
			                     DefaulValue,
			                     isNull());
		}

		public String isPK()
		{
			return String.Format("{0}", flgPK ? "PRIMARY KEY" : "");
		}

		public String isNull()
		{
			return String.Format("{0}NULL", flgNull ? "" : "NOT ");
		}
	}
}

