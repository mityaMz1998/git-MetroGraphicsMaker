using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;

using Converters;

namespace Core
{
    public class clsRazmerDvizheniya : IDisposable
    {
        //Информация из БД
        
        /// <summary>
        /// Код доступа к экземпляру класса,                         
        /// </summary>                             
        public int code;

        /// <summary>
        /// Время начала действия размера,в секундах                           
        /// </summary>                 
        public int beginTime;
            
        /// <summary>
        /// Время окончания действия размера,в секундах                        
        /// </summary>
        public int endTime;
        
        /// <summary>
        /// Парность
        /// </summary>             
        public int Parnost;
        
        /// <summary>
        /// Время полного оборота, в секундах                                                    
        /// </summary>            
        public Int32 TimeOfCompleteMovement;

        /// <summary>
        /// Допустимая величина захлеста при обороте с 1 пути на 2,в секундах                  kkkkkkk
        /// </summary>                     
        public int Zahlest1;

        /// <summary>
        ///Допустимая величина захлеста при обороте с 2 пути на 1,в секундах                   kkkkkkk
        /// </summary>                     
        public int Zahlest2;

        /// <summary>
        /// Отношение допустимой величины захлеста при обороте
        /// c 1 пути на 2 к величине оборота
        /// </summary>                    
        public Single OtnZahlest1;

        /// <summary>
        /// Отношение допустимой величины захлеста при обороте
        /// с 2 пути на 1 к величине оборота
        /// </summary>
        public Single OtnZahlest2;

        /// <summary>
        /// Признак возможности заполнения точек до общей
        /// ночной расстановки
        /// 0 - нельзя
        /// 1 - один тупик
        /// 2 - два тупика
        /// </summary>        
        public int flgRasstanovka;
                             
        /// <summary>
        /// Флаг определения часа-пик вручную
        /// </summary>
        public bool flgPikManual;
                             




        //Текущее состояние
                             
        /// <summary>
        /// Число поездов, уже прошедших в данном часе по
        /// первому пути
        /// </summary>
        public int NPoezdov1;
                             
        /// <summary>
        /// Число поездов, уже прошедших в данном часе по
        /// второму пути
        /// </summary>
        public int NPoezdov2;
                             
        /// <summary>
        /// Интервал
        /// </summary>
        public int Interval;

                             
        /// <summary>
        /// Величина возможной коррекции интервала
        /// </summary>
        public int dInterval;
        
        /// <summary>
        /// Величина последнего рассчитанного интервала в
        /// размере по 1 пути
        /// </summary>
        public int LastInt1;
                             
        /// <summary>
        /// Величина последнего рассчитанного интервала в
        /// размере по 2 пути
        /// </summary>
        public int LastInt2;

        /// <summary>
        /// Величина последнего планового интервала
        /// </summary>                   
        public int LastInterval;
                             
        /// <summary>
        /// Признак изменения интервала в течение часа
        /// </summary>
        public bool VarInterval;
                             
        /// <summary>
        /// Количество составов, которые надо вставить
        /// </summary>
        public Int32 RequiredNumberOfTrains;

        /// <summary>
        /// Потребное (плановое/расчётное) колличество составов.
        /// </summary>
        public Int32 PlannedNumberOfTrains;
       
        /// <summary>
        /// Реальное количество составов.
        /// </summary>
        public Int32 RealNumberOfTrains;
                             
        /// <summary>
        /// Реальное количество составов по первому пути.
        /// </summary>
        public Int32 RealNumberOfTrainsOnFirstTrack;
                             
        /// <summary>
        /// Реальное количество составов по второму пути.
        /// </summary>
        public Int32 RealNumberOfTrainsOnSecondTrack;
                             
        /// <summary>
        /// Количество составов (c учётом ввода/снятия) по первому пути
        /// </summary>
        public int MSostavov1tmp;
                             
        /// <summary>
        /// Количество составов (с учётом ввода/снятия) по второму пути
        /// </summary>
        public int MSostavov2tmp;
        
        /// <summary>
        /// Коллекция доступных маршрутов
        /// </summary>
        public IList<Route> colRoute;

        /// <summary>
        /// Коллекция доступных точек ночной расстановки по первому пути при нечётной расстановке
        /// </summary>
        public List<NightStayPoint> AvailableNightStayPointsOnFirstTrackByOdd;

        /// <summary>
        /// Коллекция доступных точек ночной расстановки по первому пути при чётной расстановке
        /// </summary>
        public List<NightStayPoint> AvailableNightStayPointsOnFirstTrackByEven;

        /// <summary>
        /// Коллекция доступных точек ночной расстановки по второму пути при нечётной расстановке
        /// </summary>
        public List<NightStayPoint> AvailableNightStayPointsOnSecondTrackByOdd;

        /// <summary>
        /// Коллекция доступных точек ночной расстановки по второму пути при чётной расстановке
        /// </summary>
        public List<NightStayPoint> AvailableNightStayPointsOnSecondTrackByEven;

        /// <summary>
        /// Коллекция доступных точек ночной расстановки при чётной расстановке с неопределённым путём выхода
        /// </summary>
        public List<NightStayPoint> AvailableNightStayPointsOnUndefinedTrackByOdd;

        /// <summary>
        /// Коллекция доступных точек ночной расстановки при чётной расстановке с неопределённым путём выхода
        /// </summary>
        public List<NightStayPoint> AvailableNightStayPointsOnUndefinedTrackByEven;


            //Коллекция доступных точек ночной расстановки при
            //четной расстановке по первому пути
            //public IList clPointNightChet1Put;
            //Коллекция доступных точек ночной расстановки при
            //четной расстановке по второму пути
            //public IList clPointNightChet2Put;
            //Коллекция доступных точек ночной расстановки при
            //четной расстановке с неопределенным путем выхода
            //public IList clPointNightChet0;
            //Коллекция доступных точек ночной расстановки при
            //нечетной расстановке по первому пути
            //public IList clPointNightNechet1Put;
            //Коллекция доступных точек ночной расстановки при
            //нечетной расстановке по второму пути
            //public IList clPointNightNechet2Put;
            //Коллекция доступных точек ночной расстановки при
            //нечетной расстановке с неопределенным путем выхода
            //public IList clPointNightNechet0;
            
                            //Коллекция точек снятия составов по первому пути
            //public IList clPointSnyatie1Put;
                             //Коллекция точек снятия составов по второму пути
            //public IList clPointSnyatie2Put;
                             //Последний снятый поезд
            //public TrainPath LastDeletePoezd;
                             //Первый снятый после вечернего пика поезд по 1 пути
            //public TrainPath FirstDeletePoezd1;
                             //Последний снятый после вечернего пика поезд по 1 пути
            //public TrainPath LastDeletePoezd1;
                             //Первый снятый после вечернего пика поезд по 2 пути
            //public TrainPath FirstDeletePoezd2;
                             //Последний снятый после вечернего пика поезд по 2 пути
            //public TrainPath LastDeletePoezd2;
                             //Количество фактически снятых поездов
            //public int ColFact;
            //public int ColFact1;
            //public int ColFact2;

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public clsRazmerDvizheniya(DataRow row)
        {
            colRoute = new List<Route>();

            code = Convert.ToInt32(row["Код"].ToString());
            beginTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Час_начала"].ToString()));
            endTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Час_конца"].ToString()));
            Parnost = Convert.ToInt32(row["Размер движения"].ToString());
            dInterval = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Коррекция интервала"].ToString()));
            Zahlest1 = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Захлест по 1 пути"].ToString()));
            Zahlest2 = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Захлест по 2 пути"].ToString()));
            flgRasstanovka = Convert.ToInt32(row["Признак_заполнения_тупиков"].ToString());
            RealNumberOfTrains = Convert.ToInt32(row["Количество_составов"].ToString());
            RequiredNumberOfTrains = Convert.ToInt32(row["Количество_вводимых_составов"].ToString());
            TimeOfCompleteMovement = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_полного_оборота"].ToString()));
            NPoezdov1 = 0;
            NPoezdov2 = 0;
            LastInt1 = TimeConverter.zeroSeconds;
            LastInt2 = TimeConverter.zeroSeconds;
            OtnZahlest1 = 0.4F;
            OtnZahlest2 = 0.4F;
        }

        /// <summary>
        /// Процедура инициализации текущего элемента класса "размер движения"
        /// </summary>
        /// <param name="Tab">Виртуальная таблица, аналог базы данных</param>
        /// <param name="id">Номер строки</param>
        public void Initialize(DataRow row)
        {

        }
        //--------------------------------------------------------------------------------------------

        public static Int32 GetTotalCapacity(List<NightStayPoint> collection)
        {
            return collection.Aggregate(0, (current, element) => current + element.Capacity);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
