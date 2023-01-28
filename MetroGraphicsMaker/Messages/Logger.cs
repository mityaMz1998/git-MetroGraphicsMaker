using System;
using System.IO;
using System.Text;
using System.Windows;
using Core;


namespace Messages
{
    public static class Logger
    {
        /// <summary>
        /// Полное имя файла журнала по умолчанию.
        /// </summary>
        public static String filename;

        private static readonly object locker = new object();

        /// <summary>
        /// Флаг доступности журналирования. Журналирование производится только в случае значения флага -- true.
        /// </summary>
        public static Boolean isEnable = true;

        /// <summary>
        /// Смещение, опеределяющее вложенность.
        /// </summary>
        private static StringBuilder shiftSB = new StringBuilder();

        private static String defaultShiftStr = "--> ";

        /// <summary>
        /// Текущий номер строки.
        /// </summary>
        private static Int32 currentLineNumber = 0;

        /// <summary>
        /// Ширина столбца номера строки.
        /// </summary>
        private static Int32 numberWidth = 4;


        private static String currentDate = DateTime.Now.ToString("yyyy-MM-dd hh.mm.ss");

        public static void shiftInner()
        {
            shiftSB.Append(defaultShiftStr);
        }

        public static void shiftOuter()
        {
            var length = defaultShiftStr.Length;
            shiftSB.Remove(shiftSB.Length - length, length);
        }

        public static void printTrace(String methodName, Boolean comeIn = true)
        {
            if (comeIn)
                shiftInner();
            else 
                shiftOuter(); 
            File.AppendAllText(String.Format("stackTrace_{0}.txt", currentDate), shiftSB + methodName + Environment.NewLine);
        }

        public static void Output(String format, params Object[] values)
        {
            Output(String.Format(format, values));
        }

        /// <summary>
        /// Статический метод журналирования сообщений.
        /// </summary>       
        /// <param name="message">Сообщение заносимое в журнал.</param>
        /// <param name="source">Тип (и имя) источника сообщения.</param>
        /// <param name="messageTrimOn">Флаг, указывающий включена ли обрезка (начальных и конечеых) пробелов в строке сообщения. По умолчанию включена.</param>
        /// <param name="showInfo">Флаг, указывающий выводить ли инофрмацию дате и источнике сообщения.</param>
        /// <param name="path">Полное имя (содержащее путь) файла журнала.</param>
        /// <param name="lineNumbersOn">Флаг, указывающий выводить ли номера строк.</param>
        public static void Output(String message, String source = "", Boolean messageTrimOn = true, Boolean showInfo = true, String path = "", Boolean lineNumbersOn = true)
        {
            if (!isEnable)
                return;

            if (message == null)
                throw new ArgumentNullException("message");

            if (source == null)
                throw new ArgumentNullException("source");

            if (path == null)
                throw new ArgumentNullException("path");

            if (messageTrimOn)
            {
                message = message.Trim();

                if (message.Equals(""))
                    message = "empty message";
            }

            source = source.Trim();

            if (source.Equals(""))
                source = "undefined";

            path = path.Trim();

            if (path.Equals(""))
                path = filename;

            // Открываем для дозаписи (или создаём для записи) файл расположенный по адресу path и (до)записываем в него строку с сообщением и указанием даты (и времени с точностью до милисекунды) произведённой записи.

            var number = lineNumbersOn ? Convert.ToString(++currentLineNumber).PadLeft(numberWidth, ' ') : "";

            var str = (showInfo) ?
                String.Format("{4} {0} | {3} | {1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message, Environment.NewLine, source, number) :
                String.Format("{2} {0}{1}", message, Environment.NewLine, number);

            
            lock (locker)
            {
                File.AppendAllText(path, str);
            }

        }

        public static void getArch()
        {
            var sourceName = typeof(Logger).FullName;

            Output("Текущая операционная система " + Environment.OSVersion + Environment.NewLine, sourceName, false);
            /*
            var defaultMethodsName = typeof(Object).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Select(m => m.Name).ToArray(); 

            var types = new[] { 
                typeof(clsElementOfSchedule),
                typeof(ElementOfMovementSchedule),
                typeof(Repair), 
                typeof(RepairType),
                typeof(Route), 
                typeof(mdlCreateScheduleOfRepair),
                typeof(Munkres.Solver<Int32>),
                typeof(Munkres.Matrix<Int32>)
            }.
            By(t => t.FullName);

            var length = 80 - numberWidth - 1;
            var borderString = new String('-', length);

            foreach (var t in types)
            {
                var methods = t.GetMethods().Where(m => !defaultMethodsName.Contains(m.Name)).Select(m => m.ToString()).ToArray();
                var name = String.Format("{0} methods [{1}]:", t.FullName, methods.Length);
                output(name.PadLeft((length + name.Length) / 2, ' '), sourceName, false, false);
                foreach (var m in methods)
                    output(message: m, source: sourceName, showInfo: false);
                output(borderString + Environment.NewLine, sourceName, false, false);
            }

            */

            Output("Журнал событий:" + Environment.NewLine, sourceName, false);
        }



        public static void PrintMovementSchedule(Route route)
        {
            if (route == null)
                throw new ArgumentNullException("route");

            var sb = new StringBuilder();
            sb.AppendFormat("Маршрут №{0} [{1}]", route.number, route.depot != null ? route.depot.name : "null");

#if IS_REPAIR_COLLECTION
            sb.Append(route.GetRepairsInfo(", "));
#else
            if (route.repair != null)
                sb.AppendFormat(" {0} с {1} по {2}", route.repair.inspectionPoint != null ? "на осмотре в " + route.repair.inspectionPoint.name : @"/ПТО == null/", route.repair.beginTime, route.repair.endTime);
            else
                sb.AppendFormat("Ремонт не назначен");
#endif
            Output(sb.ToString(), typeof(Logger).FullName);
        }
    }

    public static class StatisticLogger
    {
        /// <summary>
        /// Полное имя файла журнала по умолчанию.
        /// </summary>
        public static String filename;

        private static readonly object locker = new object();


        public static void Output(Int32 index, Double crytery, String path, String message = "")
        {
            var str = String.Format("{0}{4}\t{1}\t{2}{3}", index, path, crytery, Environment.NewLine, message.Equals(String.Empty) ? String.Empty : String.Format(" [{0}] ", message));

            try
            {
                lock (locker)
                {
                    File.AppendAllText(filename, str);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), typeof(StatisticLogger).FullName, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


    }
}
