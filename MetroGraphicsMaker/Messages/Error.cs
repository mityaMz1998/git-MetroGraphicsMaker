using System;
using System.Collections.Generic;
using System.Reflection;

using Core;

namespace Messages
{

    class Error
    {
        static Dictionary<String, String> objectNames = new Dictionary<String, String>();

        static void init()
        {
            if (objectNames.Count != 0) return;

            objectNames.Add("GraphicPL.Depot", "Депо");
        }

        public static void showErrorMessage(Object cause, Object source, String note = "")
        {

            note = note.Trim().ToLower();

            var causeType = cause.GetType().FullName;
            var causeName = "-";
            var causeCode = "-";

            switch (causeType)
            {
                case "GraphicPL.Depot": 
                    causeName = "Депо";
                    causeCode = ((Depot)cause).code.ToString();
                    break;

                case "GraphicPL.ElementOfMovementSchedule":
                    causeName = "Элемент графика оборота";
                    causeCode = ((ElementOfMovementSchedule)source).code.ToString();
                    break;

                case "GraphicPL.clsIspectionPoint":
                    causeName = "Пункт осмотра";
                    causeCode = ((InspectionPoint)cause).code.ToString();
                    break;

                case "GraphicPL.Line":
                    causeName = "Линия";
                    causeCode = ((Line)cause).code.ToString();
                    break;

                case "GraphicPL.NightStayPoint":
                    causeName = "Точка ночной расстановки";
                    causeCode = ((NightStayPoint)cause).code.ToString();
                    break;

                case "GraphicPL.Repair":
                    causeName = "Ремонт";
                    // causeCode = ((Repair) cause).code.ToString();
                    break;

                case "GraphicPL.RepairType":
                    causeName = "Тип ремонта";
                    causeCode = ((RepairType)cause).code.ToString();
                    break;

                case "GraphicPL.Route":
                    causeName = "Маршрут";
                    causeCode = ((Route)cause).number.ToString();
                    break;

                case "GraphicPL.Station":
                    causeName = "Станция";
                    if (note != String.Empty)
                        causeName += " по " + note + " пути";
                    causeCode = ((Station)cause).code.ToString();
                    break;

                case "GraphicPL.Train":
                    causeName = "Состав";
                    causeCode = ((Train) cause).code.ToString();
                    break;
                    
                case "GraphicPL.Wagon":
                    causeName = "Вагон";
                    causeCode = ((Wagon) cause).number.ToString();
                    break;

                case "GraphicPL.WagonType":
                    causeName = "Тип подвижного состава";
                    causeCode = ((WagonType) cause).code.ToString();
                    break;

                default:
                    Console.WriteLine("Тип причины {0} не найден в словаре типов.", causeType);
                    break;
            }

            var sourceType = source.GetType().FullName;
            var sourceName = "-";
            var sourceCode = "-";

            switch (sourceType)
            {
                case "GraphicPL.Depot": 
                    sourceName = "депо";
                    sourceCode = ((Depot)source).code.ToString();
                    break;

                case "GraphicPL.ElementOfMovementSchedule":
                    sourceName = "элементу графика оборота";
                    sourceCode = ((ElementOfMovementSchedule)source).code.ToString();
                    break;

                case "GraphicPL.InspectionPoint":
                    sourceName = "пункту осмотра";
                    sourceCode = ((InspectionPoint)source).code.ToString();
                    break;

                case "GraphicPL.Line":
                    sourceName = "линии";
                    sourceCode = ((Line)source).code.ToString();
                    break;

                case "GraphicPL.NightStayPoint":
                    sourceName = "точке ночной расстановки";
                    sourceCode = ((NightStayPoint)source).code.ToString();
                    break;

                case "GraphicPL.Repair":
                    sourceName = "ремонту";
                    break;

                case "GraphicPL.RepairType":
                    sourceName = "типу ремонта";
                    sourceCode = ((RepairType)source).code.ToString();
                    break;

                case "GraphicPL.Route":
                    sourceName = "маршруту";
                    sourceCode = ((Route)source).number.ToString();
                    break;

                case "GraphicPL.Station":
                    sourceName = "станции";
                    sourceCode = ((Station)source).code.ToString();
                    break;

                case "GraphicPL.Train":
                    sourceName = "составу";
                    sourceCode = ((Train)cause).code.ToString();
                    break;

                case "GraphicPL.Wagon":
                    sourceName = "вагону";
                    sourceCode = ((Wagon)cause).number.ToString();
                    break;

                case "GraphicPL.WagonType":
                    sourceName = "типу подвижного состава";
                    sourceCode = ((WagonType)cause).code.ToString();
                    break;

                default:
                    Console.WriteLine("Тип источника {0} не найден в словаре типов.", causeType);
                    break;
            }

            if (note != String.Empty && causeType != "GraphicPL.Station")
                note = " " + note;
            else
                note = String.Empty;


            var str = String.Format("{0}{2} c кодом {1} не был(а/о) присвоен(а/о) {3} с кодом {4}, так как отсутствует в базе данных.", causeName, causeCode, note, sourceName, sourceCode);

            Console.WriteLine(str);
            Logger.Output(str, sourceType);
        }



        // Для Depot.Initialize()
        public static String stationMissing = "Станция с id = {0} по {1} пути не была присвоена депо с id = {2}, так как такая станция не была найдена в БД.";

        // Для clsInspecionPoint.Initialize(), Route.Initialize()
        public static String depotMissing = "Депо с id = {0} для пункта осмотра с id = {1} не было присвоено, так как такое депо не было найдено в БД.";

        // Для clsInspecionPoint.Initialize()
        public static String nightStayPointMissing = "Точка ночной расстановки №{0} с id = {1} для пункта осмотра с id = {2} не была присвоена, так как такая точки ночной расстановки не была найдена в БД.";

        // Для Station.Initialize()
        public static String lineMissing = "Линия с id = {0} не была присвоена станции с id = {1}, так как такая линия не была найдена в БД.";

        // Для Repair.Initialize()
        public static String repairTypeMissing = "Тип ремонта с id = {0} не был присвоен ремонту, так как такой тип ремонта не был найден в БД.";

        // Для Repair.Initialize()
        public static String routeMissing = "Маршрут с id = {0} не был присвоен ремонту, так как такой маршрут не был найден в БД.";

        // Для Repair.Initialize()
        public static String inspectionPointMissing = "Пункт осмотра с id = {0} не был присвоен ремонту, так как такой пункт осмотра не был найден в БД.";

    }
}
