using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
//C:\Program Files\Common Files\system\ado\msadox.dll
using ADOX;
using System.Windows;
//using System.Reflection;
// TODO: подходящее описание класса clsTask лежит пока в new_clsTask

using Messages;
using LoadData;
using Converters;
using Microsoft.Win32.SafeHandles;

namespace Core
{
    public class Loader
    {
        public static ConcurrentDictionary<String, DataTable> m_syncData;
        // TODO: Плохо продумана архитектура, работу с БД нужно локализовать
        String m_dbName = null;
        String m_connectionString = String.Empty;
        public Storage strg;

        public static FileInfo PathToDB;

        public Loader(String dbName = null)
        {

            Logger.Output(String.Format("Loader ctr --> {0}", dbName ?? "null"));
            m_syncData = new ConcurrentDictionary<String, DataTable>();
            try
            {
                CreateProviderString(dbName);
            }
            catch (Exception exception)
            {
                Logger.Output(exception.ToString(), GetType().FullName);
            }
        }

        public Boolean Init(String name = null)
        {

            var watch = Stopwatch.StartNew();

            try
            {
                CreateProviderString(name);
            }
            catch (Exception exception)
            {
                Logger.Output(exception.ToString(), GetType().FullName);
            }

            // @TODO: Логику нужно менять!!!

            var dao = new DAO(m_dbName);

            var filename = "test_xml.xml"; // "_standart.xml"

            if (!dao.CheckSchema(filename))
                return false;

            var db = dao.SnapshotDB();
            strg = new Storage(db);

            // на самом деле возможно и information
            var partition = new Partition(db); // NOTE: беда начинается тут.

            /* Параллельный блок кода - начало. */
            // 1. Создание таблиц
            var leftThread = new Thread(threadCreateTables);
            leftThread.Start(partition.Left);
            threadCreateTables(partition.Right);
            leftThread.Join();

            // 2. Заполнение таблиц
            leftThread = new Thread(threadFillTables);
            leftThread.Start(partition.Left);
            threadFillTables(partition.Right);
            leftThread.Join();

            /*
            // 3. Пробежка и добивание
            leftThread = new Thread(threadCorrection);
            leftThread.Start(partition.Left);
            threadCorrection(partition.Right);
            leftThread.Join();
            */

            /* Параллельный блок кода конец. */

            CreateCollections();
            LinkingCollections();
            ValidateCollections();
            /*
            if (!Dialogs.init(String.Format(@"..{0}..{0}Resources{0}ru_dialog.xml", System.IO.Path.DirectorySeparatorChar)))
                Logger.Output("Файл локализации загружен не полностью или составлен с ошибками", GetType().FullName);
            */
            // mdlCreateScheduleOfRepair.CreateGraphicOborota();
            
           //  dao.OutputToXml("test_xml_1.xml", false);

            watch.Stop();

            Logger.Output(String.Format("Итого затрачено времени [in loader.Init()]: {0}", watch.Elapsed.ToString(@"mm\:ss\.fff")), GetType().FullName);


           // reportToHTML();
            /*
            foreach (var route in MovementSchedule.colRoute)
            {                    
                Logger.output(route.PrintToHtml(), GetType().FullName, showInfo: false, lineNumbersOn: false, path: "report.html");    
            }
            */

            return true;
        }

        /*
        public void reportToHTML(String filename = "")
        {
            if (filename == null)
                throw  new ArgumentNullException("filename");

            var _fileHead = String.Format("route report [{0}]", DateTime.Now.ToString("yyy-MM-dd hh.mm.ss"));

            var _filename = filename.Trim();
            if (_filename.Equals(""))
                _filename = _fileHead + ".html";


            var _fileText = new StringBuilder("<!DOCTYPE html><html><head><meta http-equiv='Content-Language' content='ru, en'><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><link rel='stylesheet' type='text/css' href='table_style.css'> <script type='text/javascript' src='jquery.js'></script><script type='text/javascript' src='tables.js'></script></head><body><table>");
            _fileText.AppendFormat("<caption>{0}</caption><tr><th>№ маршрутов</th><th>Выход составов</th><th> 1 </th><th> Элемент ГО </th><th> 2 </th><th>Заход составов</th><th>№ маршрутов</th></tr>", _fileHead);

            foreach (var route in MovementSchedule.colRoute)
            {
                _fileText.Append(route.PrintToHtml());
            }

            _fileText.Append("</table></body></html>");

            Logger.Output(_fileText.ToString(), path: _filename, messageTrimOn: false, showInfo: false, lineNumbersOn: false);

        }

         */
        /// <summary>
        /// Делегат для многопоточного создания таблиц БД
        /// </summary>
        /// <param name="data"></param>
        void threadCreateTables(Object data)
        {
            try
            {
                var storage = (data as Storage);
                if (storage == null)
                    return;

                foreach (var table in storage.Tables)
                {
                    var dataTable = new DataTable(table.Name);
                    foreach (var field in table.Fields.Values)
                        dataTable.Columns.Add(field.Name, field.type);
                    m_syncData.GetOrAdd(table.Name, dataTable);
                }
            }
            catch (Exception exception)
            {
                Logger.Output(exception.ToString(), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
                //Console.WriteLine(exception);
            }
        }


        DataTypeEnum TypeConverter(String typeStr)
        {
            return (DataTypeEnum) Convert.ToInt32(typeStr);
        }

        /// <summary>
        /// Делегат для многопоточного заполнения таблиц БД
        /// </summary>
        /// <param name="data"></param>
        void threadFillTables(Object data)
        {
            var connection = new OleDbConnection(m_connectionString);
            try
            {
                // TODO: Здесть стоит использовать блок using
                // Пытаемся открыть соединение
                connection.Open(); // TODO: Беда тут!!!
                var storage = data as Storage;
                if (storage == null)
                    return;

                foreach (var table in storage.Tables)
                {
                    // создаём анонимный адаптер
                    new OleDbDataAdapter(
                        // и результатами селективного запроса
                        String.Format("SELECT * FROM [{0}]{1}",
                            table.Name,
                            ((table.PrimaryKey != "") ? String.Format(" ORDER BY [{0}]", table.PrimaryKey) : "")
                    ), connection
                        // заполняем таблицу с именем table.name
                    ).Fill(m_syncData[table.Name]);
                }
            }
            catch (Exception exception)
            {
                // непотокобезопасно
                Logger.Output(exception.ToString(), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
                
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }


       

        private void CreateProviderString(String databaseName)
        {
            if (databaseName == null)
                throw new ArgumentNullException("databaseName");
            if (!databaseName.Any())
                throw new ArgumentException("Invalid Argument Exception", "databaseName");

            m_dbName = databaseName;
            m_connectionString = String.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source = {0}", m_dbName);
        }



        /// <summary>
        /// Делегат для многопоточной проверки соответствия sync_data эталону и исправления неточностей
        /// </summary>
        /// <param name="data"></param>
        void threadCorrection(Object data)
        {
            try
            {
                var storage = data as Storage;
                if (storage == null)
                    return;

                foreach (var table in storage.Tables)
                    foreach (var field in table.Fields.Values)
                        if (!m_syncData[table.Name].Columns.Contains(field.Name))
                            m_syncData[table.Name].Columns.Add(field.Name, field.type);
                
            }
            catch (Exception exception)
            {
                Logger.Output(exception.ToString(), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
                
            }
        }

        //public static List<Tuple<IList, string, Type>> initialData;

        public void CreateCollections()
        {
            //Tuple<List<Line>, string, Line>[] test =
            //    { Tuple.Create(MovementSchedule.colLine, "Линии", new Line()) };

            //var initialData = new List<Tuple<IEnumerable<Entity>, String, Type>>();

            //initialData.Add(MovementSchedule.colLine, "Линии", typeof (Line));
            //initialData.Add(MovementSchedule.colStation, "Станции", typeof (Station));
            //initialData.Add(MovementSchedule.colThread, "Нитки графика", typeof (TrainPath));

            //initialData.Add(MovementSchedule.colElementOfSchedule, "Элементы расписания", typeof(clsElementOfSchedule));
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());
            //initialData.Add(MovementSchedule., "", typeof());

            //var testLines = initialData.GetCollectionByName<Line>("Линии");
            //var line = testLines.First();

            //initialData.Add(MovementSchedule.colLine, "Линии", new Line());

            /*
            initialData.Add(Tuple.Create(MovementSchedule.colLine, "Линии", new Line()));
            initialData.Add(Tuple.Create(MovementSchedule.colStation, "Станции", new Station()));
            initialData.Add(Tuple.Create(MovementSchedule.colThread, "Нитки графика", new TrainPath()));
            initialData.Add(Tuple.Create(MovementSchedule.colElementOfSchedule, "Элементы расписания", new clsElementOfSchedule()));
            initialData.Add(Tuple.Create(MovementSchedule.colTask, "Задания", new Task()));
            initialData.Add(Tuple.Create(MovementSchedule.colRoute, "Таблица маршрутов", new Route()));
            initialData.Add(Tuple.Create(MovementSchedule.colDepot, "Таблица депо", new Depot()));
            initialData.Add(Tuple.Create(MovementSchedule.colRepairType, "Таблица типов ремонта", new RepairType()));
            initialData.Add(Tuple.Create(MovementSchedule.colInspectionPoint, "Таблица пунктов осмотра", new InspectionPoint()));
            initialData.Add(Tuple.Create(MovementSchedule.colNightStayPoint, "Точки ночного отстоя", new NightStayPoint()));

            initialData.Add(Tuple.Create(MovementSchedule.colRepair, "График оборота: Ремонты", new Repair()));
            initialData.Add(Tuple.Create(MovementSchedule.colLegend, "Надписи", new Legend()));
            initialData.Add(Tuple.Create(MovementSchedule.colMovementMode, "Типы расписаний", new MovementMode()));

            initialData.Add(Tuple.Create(MovementSchedule.colWagon, "Вагоны", new Wagon()));
            initialData.Add(Tuple.Create(MovementSchedule.colTrain, "Составы", new Train()));
            initialData.Add(Tuple.Create(MovementSchedule.colTimeNorm, "Таблица норм ремонтов", new TimeNorm()));
            initialData.Add(Tuple.Create(MovementSchedule.colWagonType, "Таблица типов подвижного состава", new WagonType()));
            */

            Logger.Output("in CreateCollections()", GetType().FullName);

            // NOTE: ВАЖНО! там где нет прямого обращения (MovementSchedule.<collectionName>) -- ссылка на коллекцию равна null (MovementSchedule.<collectionName> == null -- конструктор ещё не был вызван)
            /*
            var keys = m_syncData.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
                Logger.output(message: String.Format("  > {0}", key), source: GetType().FullName, messageTrimOn: false);
            */

            MovementSchedule.colLine = m_syncData["Линии"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new Line(row)).ToList();

            // TODO: Необходимо брать цифры не с потолка, а расчитывать каким-либо образом.
            //MovementSchedule.worker.ReportProgress(45);


            //MovementSchedule.colVisThread = m_syncData["Нитки графика"].Rows.AsParallel().AsOrdered().OfType<DataRow>().Select(row => new TrainPath(row)).ToList();

            MovementSchedule.colStation = m_syncData["Станции"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new Station(row)).ToList();
            
            MovementSchedule.colThread = m_syncData["Нитки графика"].Rows.AsParallel()
            .AsOrdered().OfType<DataRow>().Select(row => new TrainPath(row)).ToList();

            MovementSchedule.colElementOfSchedule = m_syncData["Элементы расписания"].Rows.AsParallel()
            .AsOrdered().OfType<DataRow>().Select(row => new clsElementOfSchedule(row)).ToList();
            //MovementSchedule.worker.ReportProgress(46);

            MovementSchedule.colTask = m_syncData["Задания"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new Task(row)).ToList();
            //MovementSchedule.worker.ReportProgress(47);

            MovementSchedule.colRoute = m_syncData["Таблица маршрутов"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new Route(row)).ToList();

            MovementSchedule.colDepot = m_syncData["Таблица депо"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new Depot(row)).ToList();

            MovementSchedule.colRepairType =
                m_syncData["Таблица типов ремонта"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new RepairType(row)).ToList();

            MovementSchedule.TO1 = MovementSchedule.colRepairType.SingleOrDefault(t => t.name.Equals("ТО1"));

            MovementSchedule.colInspectionPoint =
                m_syncData["Таблица пунктов осмотра"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new InspectionPoint(row)).ToList();

            MovementSchedule.colNightStayPoint =
                m_syncData["Точки_ночного_отстоя"].Rows.AsParallel()
                        .AsOrdered().OfType<DataRow>().Select(row => new NightStayPoint(row)).ToList();

            // заглушка!!!
                                    //MovementSchedule.colPrintObject =
                                        //m_syncData["Объекты печати"].Rows.AsParallel()
                                            //.AsOrdered()
                                            //.OfType<DataRow>()
                                            //.Select(row => new clsPrintObject(row)).ToList();
            // заглушка!!!
            try
            {
                //MovementSchedule.TimeBeginSheet =
                   // Convert.ToDateTime(m_syncData["Параметры графика"].Rows[0]["Время_начала_оборота"].ToString());
            }
            catch(Exception e)
            {
               // Logger.Output(e.ToString(), GetType().FullName);
               // MovementSchedule.TimeBeginSheet = new DateTime();
            }

            MovementSchedule.colRepair = m_syncData["График оборота: Ремонты"].Rows.AsParallel().AsOrdered().OfType<DataRow>().Select(row => new Repair(row)).ToList();

            // TODO: Надпись как вариант можно назвать legend или inscription
            MovementSchedule.colLegend =
                m_syncData["Надписи"].Rows.AsParallel()
                    .AsOrdered()
                    .OfType<DataRow>()
                    .Select(row => new Legend(row))
                    .ToList();

            // TODO: Переименовать класс clsRazmerDvizheniya во что-то более разумное
            var cRazDvizh = new List<clsRazmerDvizheniya>(); 
            foreach (DataRow row in m_syncData["Размеры движения"].Rows)
                cRazDvizh.Add(new clsRazmerDvizheniya(row));
            //MovementSchedule.colRazmerDvizheniya = cRazDvizh;

            //var elements = new List<clsElementOfSchedule>();
            //foreach (DataRow row in m_syncData["Элементы расписания"].Rows)
                //elements.Add(new clsElementOfSchedule(row));
            //MovementSchedule.colElementOfSchedule = elements;


            MovementSchedule.colMovementMode = 
                m_syncData["Типы расписаний"].Rows.AsParallel().AsOrdered().OfType<DataRow>().Select(row => new MovementMode(row)).ToList();

            try
            {

                MovementSchedule.colWagon =
                    m_syncData["Вагоны"].Rows.AsParallel()
                        .AsOrdered()
                        .OfType<DataRow>()
                        .Select(r => new Wagon(r))
                        .ToList();

                MovementSchedule.colTrain =
                    m_syncData["Составы"].Rows.AsParallel()
                        .AsOrdered()
                        .OfType<DataRow>()
                        .Select(r => new Train(r))
                        .ToList();

                MovementSchedule.colTimeNorm =
                    m_syncData["Таблица норм ремонтов"].Rows.AsParallel()
                        .AsOrdered()
                        .OfType<DataRow>()
                        .Select(r => new TimeNorm(r)).ToList();

                MovementSchedule.colWagonType =
                    m_syncData["Таблица типов подвижного состава"].Rows.AsParallel()
                        .AsOrdered()
                        .OfType<DataRow>()
                        .Select(r => new WagonType(r))
                        .ToList();

            }
            catch (Exception)
            {
                var message = String.Format("Необходимо добавить таблицы: [Вагоны], [Составы], [Таблица норм ремонтов], [Таблица типов подвижного состава].{0}Дальнейшее функционирование программы может быть неустойчивым.", Environment.NewLine);
                var title = "Вы используете устаревший формат файла!";
                // TODO: Записать в БД!!!

                MovementSchedule.colTimeNorm = new List<TimeNorm>();

                MovementSchedule.colWagon = new List<Wagon>();

                MovementSchedule.colWagonType = new List<WagonType>();

                MovementSchedule.colTrain = new List<Train>();



                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }


            /*
            try
            {
                var schedules = new List<ElementOfMovementSchedule>();
                foreach (DataRow row in m_syncData["График оборота: Элементы"].Rows)
                    schedules.Add(new ElementOfMovementSchedule(row));
                MovementSchedule.colElementsOfRepairs = schedules;
            }
            catch(Exception e)
            {
                Messages.Logger.output(e.ToString(), GetType().FullName);
            }
             */
            //MovementSchedule.worker.ReportProgress(56);
        }

        /// <summary>
        /// Процедура связи созданных коллекций
        /// </summary>
        void LinkingCollections()
        {
            Logger.Output("in LinkingCollections()", GetType().FullName);

            // TODO: Плохо продумана архитектура. Идея дёргать по именам не самая лучшая.

            //MovementSchedule.worker.ReportProgress(61);
            


            for (var index = 0; index < MovementSchedule.colLine.Count; ++index)
                MovementSchedule.colLine[index].Initialize(m_syncData["Линии"].Rows[index]);
            
            for (var index = 0; index < MovementSchedule.colStation.Count; ++index)
                MovementSchedule.colStation[index].Initialize(m_syncData["Станции"].Rows[index]);

            for (var index = 0; index < MovementSchedule.colTask.Count; ++index)
                MovementSchedule.colTask[index].Initialize(m_syncData["Задания"].Rows[index]);

            //Устанавливаем текущей линией первую линию из списка
            MovementSchedule.CurrentLine = MovementSchedule.colLine.FirstOrDefault();
            if (MovementSchedule.CurrentLine != null)
            {
                MovementSchedule.CurrentLine.oddDirection.Initialize(DirectionValue.ODD, MovementSchedule.CurrentLine, MovementSchedule.CurrentLine.evenDirection);
                MovementSchedule.CurrentLine.evenDirection.Initialize(DirectionValue.EVEN, MovementSchedule.CurrentLine, MovementSchedule.CurrentLine.oddDirection);
            }

            /*
            var line = MovementSchedule.colLine.Single(l => l.code == MovementSchedule.CurrentLine.code);
            line.oddDirection.Initialize(1, line, line.evenDirection);
            line.evenDirection.Initialize(-1, line, line.oddDirection);
            */
            /*
            foreach (var line in MovementSchedule.colLine)
            {
                if (line == MovementSchedule.CurrentLine)
                {
                    line.oddDirection.Initialize(DirectionValue.ODD, line, line.evenDirection);
                    line.evenDirection.Initialize(DirectionValue.EVEN, line, line.oddDirection);
                }
            }
            */
            for (var index = 0; index < MovementSchedule.colRazmerDvizheniya.Count; ++index)
                MovementSchedule.colRazmerDvizheniya[index].Initialize(m_syncData["Размеры движения"].Rows[index]);

            for (var index = 0; index < MovementSchedule.colElementOfSchedule.Count; ++index)
                MovementSchedule.colElementOfSchedule[index].Initialize(m_syncData["Элементы расписания"].Rows[index]);

            for (var index = 0; index < MovementSchedule.colDepot.Count; ++index)
                MovementSchedule.colDepot[index].Initialize(m_syncData["Таблица депо"].Rows[index]);

            for (var index = 0; index < MovementSchedule.colNightStayPoint.Count; ++index)
                MovementSchedule.colNightStayPoint[index].Initialize(m_syncData["Точки_ночного_отстоя"].Rows[index]);

            for (var index = 0; index < MovementSchedule.colTimeNorm.Count; ++index)
                MovementSchedule.colTimeNorm[index].Initialize(m_syncData["Таблица норм ремонтов"].Rows[index]);

            /* Беда_2 */
            for (var index = 0; index < MovementSchedule.colInspectionPoint.Count; ++index)
                MovementSchedule.colInspectionPoint[index].Initialize(m_syncData["Таблица пунктов осмотра"].Rows[index]);
            
            /* Беда_1 */
            for (var index = 0; index < MovementSchedule.colRoute.Count; ++index)
                MovementSchedule.colRoute[index].Initialize(m_syncData["Таблица маршрутов"].Rows[index]);

            /* Беда_3 */
            for (var index = 0; index < MovementSchedule.colRepair.Count; ++index)
                MovementSchedule.colRepair[index].Initialize(m_syncData["График оборота: Ремонты"].Rows[index]);

            /*
            Logger.output(message: "Информация о маршрутах, после связывания объектов (конец метода LinkingCollections())", source: GetType().FullName);
            foreach (var route in MovementSchedule.colRoute)
                Logger.output(route.ToString(), GetType().FullName);
             */

            // @TODO: Initialize() for colWagon, colWagonType, colTimeNorm, colTrain


            for (var index = 0; index < MovementSchedule.colWagonType.Count; ++index)
                MovementSchedule.colWagonType[index].Initialize(m_syncData["Таблица типов подвижного состава"].Rows[index]);

            for (var index = 0; index < MovementSchedule.colTrain.Count; ++index)
                MovementSchedule.colTrain[index].Initialize(m_syncData["Составы"].Rows[index]);

            for (var index = 0; index < MovementSchedule.colWagon.Count; ++index)
                MovementSchedule.colWagon[index].Initialize(m_syncData["Вагоны"].Rows[index]);

            //for (var index = 0; index < MovementSchedule.colVisThread.Count; ++index)
                //MovementSchedule.colVisThread[index].Initialize(m_syncData["Нитки графика"].Rows[index]);

            var timetables = new List<Timetable>();
            foreach (DataRow row in m_syncData["Времена хода"].Rows)
                timetables.Add(new Timetable(row));

            //var timetables = (from DataRow row in m_syncData["Времена хода"].Rows select new Timetable(row)).ToList();

            //var timetables = m_syncData["Времена хода"].Rows.OfType<DataRow>().Select(row => new Timetable(row));

            //var timetables = m_syncData["Времена хода"].Rows.AsParallel().AsOrdered().OfType<DataRow>().Select(row => new Timetable(row));


            MessageBox.Show(String.Format("result = {0}", timetables.Count()));
        }

        public void ValidateCollections()
        {
            var sb = new StringBuilder();

            // Работа с коллекцией маршрутов
            var messages = new List<String>
                {
                    Environment.NewLine + "Этот маршрут не имеет ссылки на предыдущий:",
                    Environment.NewLine + "Этот маршрут не имеет ссылки на следущий:" ,
                    Environment.NewLine + "Этот и предыдущий (для него) маршруты приписаны к разным депо:",
                    Environment.NewLine + "Этот и следущий (для него) маршруты приписаны к разным депо:"
                };

            var flags = new List<Boolean>
            {
                AddMessageToSB(MovementSchedule.colRoute.Where(r => r.prevRoute == null).Select(t => new Tuple<Route, Route>(t, t.prevRoute)).ToList(), messages[0], ref sb),
                AddMessageToSB(MovementSchedule.colRoute.Where(r => r.nextRoute == null).Select(t => new Tuple<Route, Route>(t, t.nextRoute)).ToList(), messages[1], ref sb)
            };
            
            if (!flags[0])
                flags.Add(AddMessageToSB(MovementSchedule.colRoute.Where(r => r.depot != r.prevRoute.depot).Select(t => new Tuple<Route, Route>(t, t.prevRoute)).ToList(), messages[2], ref sb));

            if (!flags[1])
                flags.Add(AddMessageToSB(MovementSchedule.colRoute.Where(r => r.depot != r.nextRoute.depot).Select(t => new Tuple<Route, Route>(t, t.nextRoute)).ToList(), messages[3], ref sb));
            
            var haveProblem = flags.Any(f => f);

            // очистили используемые коллекции и подготовили ...
            flags.Clear();
            messages.Clear();
            // ... задел на будущее
            //MessageBox.Show(MovementSchedule.colRoute.Count.ToString("D"));
            /*
#if LOGER_ON
            if (MovementSchedule.colRoute.Any(route => route.number == route.nextRoute.number))
            {
                Logger.output("График выходного дня!", GetType().FullName);
                var firstIndex = 0;
                var lastIndex  = MovementSchedule.colRoute.Count - 1;
                for (var index = firstIndex; index < lastIndex; ++index)
                {
                    MovementSchedule.colRoute[index].nextRoute = MovementSchedule.colRoute[index + 1];
                    MovementSchedule.colRoute[index + 1].prevRoute = MovementSchedule.colRoute[index];
                }
                MovementSchedule.colRoute[lastIndex].nextRoute = MovementSchedule.colRoute[firstIndex];
                MovementSchedule.colRoute[firstIndex].prevRoute = MovementSchedule.colRoute[lastIndex];
            }
#endif
            
            MessageBox.Show(MovementSchedule.colRoute.Count.ToString("D"));

            foreach (var route in MovementSchedule.colRoute)
            {
                
            }
            */

            sb.AppendLine("Маршруты: ");
            var flag = false;
            foreach (var route in MovementSchedule.colRoute)
            {
                if (flag || route.number + 1 == route.nextRoute.number)
                    continue;
                sb.Append(route.number.ToString("000") + " --> ");
                flag = true;
            }

            var title = "Нарушена связь";
            var start = "Проблемы со связями:" + Environment.NewLine;
            if (haveProblem)
                MessageBox.Show(start + sb, title, MessageBoxButton.OK, MessageBoxImage.Error);

            // @TODO: Переписать класс!!!
            //new RepairCollections().Show();

        }

        /// <summary>
        /// Анализирует вектор "проблемных" кортежей (пуст ли?), добавляет информацию (в виде строк в строкопостроителе) и уведомляет (резульатом) о наличии проблем.
        /// </summary>
        /// <param name="routeTuples">Вектор кортежей (двоек) ссылок на маршруты. Первый элемент текущий, второй -- зависимый (контекст).</param>
        /// <param name="message">Сообщение пользователю, обуславливающее контекст.</param>
        /// <param name="sb">Строкопостроитель (инстанс класс StringBuilder).</param>
        /// <returns>Флаг наличия искомой проблемы (true -- проблема есть).</returns>
        private static Boolean AddMessageToSB(List<Tuple<Route, Route>> routeTuples, String message, ref StringBuilder sb)
        {
            if (routeTuples == null) throw new ArgumentNullException("routeTuples");

            if (sb == null) throw new ArgumentNullException("sb");

            var result = routeTuples.Count > 0;
            if (result)
                sb.AppendLine(message);
            foreach (var tuple in routeTuples)
                sb.AppendLine(String.Format("№{0} ({1})", tuple.Item1.number.ToString("000"), tuple.Item2 != null ? "№" + tuple.Item2.number.ToString("000") : "null"));
            return result;
        }
    }
}
