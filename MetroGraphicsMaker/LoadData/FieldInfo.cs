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
			/* Тип данных. 
			 * Характеристика поля, определяющая тип данных, который может содержать это поле. 
			 * Существуют следующие типы данных 
			 *      Boolean,                    Bool        1/8 (будем смотреть правде в глаза при представлении в C# уже 1)
			 *      Integer,                    Int         4 (? -- для числовых данных 1,2,4,8 или 16 байт)
			 *      Long,                       Long        8 (? -- для числовых данных 1,2,4,8 или 16 байт)
			 *      Currency, 
			 *      Single,                     Float       
			 *      Double,                     Double      
			 *      Date,                       DataTime    8
			 *      String                      String      
			 *      и Variant (по умолчанию).
			*/
                /*
				case "System.Int32":
					size = sizeof(int); //tmp = "Целое (int)";
					break;
				case "System.Single":
					size = sizeof(float); //tmp = "Одинарное с плавающей точкой (float)";
					break;
				case "System.Datetime":
					size = 8;//sizeof(DateTime); //tmp = "Дата и время (datetime)";
					break;
				case "Sistem.Boolean":
					size = sizeof(bool); //tmp = "Логическое | Да/Нет (bit | bool)";
					break;
				case "System.Byte":
					size = sizeof(byte); //tmp = "Байт (byte)";
					break;
				case "System.String":
					size = 255; //tmp = "Текст (text)";
					break;
                 */
			}
            try
            {
                // @TODO: проблема с упаковкой типов DateTime и String
                return System.Runtime.InteropServices.Marshal.SizeOf(type);
            }
            catch (Exception)
            {
                try 
                {
                    // @TODO: Стоит придумать что-то более вменяемое!
                    // Messages.Logger.output(String.Format("Заплатка!!! type is {0}", type.FullName), GetType().FullName);
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

