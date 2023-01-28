using System;
using ADOX;
using Messages;

namespace WpfApplication1.LoadData.Reflection
{
    internal class SwitchDbTypeConverter : IDbTypeConverter
    {
        public DataTypeEnum Convert(Type type)
        {
            var resultType = DataTypeEnum.adEmpty;

            if (type == typeof (SByte))
                resultType = DataTypeEnum.adTinyInt;
            else if (type == typeof (Int16))
                resultType = DataTypeEnum.adSmallInt;
            else if (type == typeof (Int32))
                resultType = DataTypeEnum.adInteger;
            else if (type == typeof (Int64))
                resultType = DataTypeEnum.adBigInt;
            else if (type == typeof (Byte))
                resultType = DataTypeEnum.adUnsignedTinyInt;
            else if (type == typeof (UInt16))
                resultType = DataTypeEnum.adUnsignedSmallInt;
            else if (type == typeof (UInt32))
                resultType = DataTypeEnum.adUnsignedInt;
            else if (type == typeof (UInt64))
                resultType = DataTypeEnum.adUnsignedBigInt;
            else if (type == typeof (Boolean))
                resultType = DataTypeEnum.adBoolean;
            else if (type == typeof (Single))
                resultType = DataTypeEnum.adSingle;
            else if (type == typeof (Double))
                resultType = DataTypeEnum.adDouble;
            else if (type == typeof (Decimal))
                resultType = DataTypeEnum.adDecimal;
            else if (type == typeof (DateTime))
                resultType = DataTypeEnum.adDBTimeStamp;
            else if (type == typeof (Guid))
                resultType = DataTypeEnum.adGUID;
            else if (type == typeof (String))
                resultType = DataTypeEnum.adWChar;
            else if (type == typeof (String))
                resultType = DataTypeEnum.adVarWChar;
            else throw new ArgumentOutOfRangeException("type");

            return resultType;
        }

        public Type Convert(DataTypeEnum type)
        {
            Type resultType = null;
            Logger.Output(type.ToString(), GetType().FullName);

            switch (type)
            {
                case DataTypeEnum.adEmpty:
                    break;
                case DataTypeEnum.adTinyInt:
                    resultType = typeof (SByte);
                    break;
                case DataTypeEnum.adSmallInt:
                    resultType = typeof (Int16);
                    break;
                case DataTypeEnum.adInteger:
                    resultType = typeof (Int32);
                    break;
                case DataTypeEnum.adBigInt:
                    resultType = typeof (Int64);
                    break;
                case DataTypeEnum.adUnsignedTinyInt:
                    resultType = typeof (Byte);
                    break;
                case DataTypeEnum.adUnsignedSmallInt:
                    resultType = typeof (UInt16);
                    break;
                case DataTypeEnum.adUnsignedInt:
                    resultType = typeof (UInt32);
                    break;
                case DataTypeEnum.adUnsignedBigInt:
                    resultType = typeof (UInt64);
                    break;
                case DataTypeEnum.adSingle:
                    resultType = typeof (Single);
                    break;
                case DataTypeEnum.adDouble:
                    resultType = typeof (Double);
                    break;
                case DataTypeEnum.adCurrency:
                    resultType = typeof (Decimal);
                    break;
                case DataTypeEnum.adDecimal:
                    resultType = typeof (Decimal);
                    break;
                case DataTypeEnum.adNumeric:
                    resultType = typeof (Decimal);
                    break;
                case DataTypeEnum.adBoolean:
                    resultType = typeof (Boolean);
                    break;
                case DataTypeEnum.adError:
                    break;
                case DataTypeEnum.adUserDefined:
                    break;
                case DataTypeEnum.adVariant:
                    break;
                case DataTypeEnum.adIDispatch:
                    break;
                case DataTypeEnum.adIUnknown:
                    break;
                case DataTypeEnum.adGUID:
                    resultType = typeof (Boolean);
                    break;
                case DataTypeEnum.adDate:
                    resultType = typeof (DateTime);
                    break;
                case DataTypeEnum.adDBDate:
                    resultType = typeof (DateTime);
                    break;
                case DataTypeEnum.adDBTime:
                    resultType = typeof (DateTime);
                    break;
                case DataTypeEnum.adDBTimeStamp:
                    resultType = typeof (DateTime);
                    break;
                case DataTypeEnum.adBSTR:
                    resultType = typeof (String);
                    break;
                case DataTypeEnum.adChar:
                    resultType = typeof (String);
                    break;
                case DataTypeEnum.adVarChar:
                    resultType = typeof (String);
                    break;
                case DataTypeEnum.adLongVarChar:
                    resultType = typeof (String);
                    break;
                case DataTypeEnum.adWChar:
                    resultType = typeof (String);
                    break;
                case DataTypeEnum.adVarWChar:
                    resultType = typeof (String);
                    break;
                case DataTypeEnum.adLongVarWChar:
                    resultType = typeof (String);
                    break;
                case DataTypeEnum.adBinary:
                    break;
                case DataTypeEnum.adVarBinary:
                    break;
                case DataTypeEnum.adLongVarBinary:
                    break;
                case DataTypeEnum.adChapter:
                    break;
                case DataTypeEnum.adFileTime:
                    break;
                case DataTypeEnum.adPropVariant:
                    break;
                case DataTypeEnum.adVarNumeric:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            return resultType;
        }
    }
}