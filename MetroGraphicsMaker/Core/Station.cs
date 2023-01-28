using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using System.Data;
using System.Windows;
using Messages;


namespace Core
{
    /// <summary>
    /// Направление станционных путей
    /// </summary>
    public enum StationTrackDirection
    { 
        /// <summary>
        /// Чётное направление (-1)
        /// </summary>
        EVEN = -1,

        /// <summary>
        /// Значение по умолчанию (0)
        /// </summary>
        NONE =  0,

        /// <summary>
        /// Нечётное направление (1)
        /// </summary>
        ODD  =  1
    }


    //Признак конечной станции:
    //  -1 - конечная вниз
    //   0 - неконечная
    //   1 - конечная вверх
    public enum EndStationDirection
    { 
        /// <summary>
        /// -1 - Конечная станция вниз (в чётном направлении)
        /// </summary>
        DOWN = -1,
        
        /// <summary>
        /// 0 - Значение по умолчанию (не конечная)
        /// </summary>
        NONE =  0,
        
        /// <summary>
        /// 1 - Конечная станция в нечётном направлении
        /// </summary>
        UP   =  1
    }

    //Координаты
    //Флаг начала станции
    //0 - Верхний - для ниток, идущих вверх, нижний - для ниток, идущих вниз
    //1 - Нижний - для ниток, идущих вверх, верхний - для ниток, идущих вниз
    //2 - Верхний для всех ниток
    //3 - Нижний для всех ниток
    public enum StationBegining
    { 
        /// <summary>
        /// 0 - Верхний - для ниток, идущих вверх, нижний - для ниток, идущих вниз
        /// </summary>
        SONAPR = 0,

        /// <summary>
        /// 1 - Нижний - для ниток, идущих вверх, верхний - для ниток, идущих вниз
        /// </summary>
        RAZNONAPR = 1,

        /// <summary>
        /// 2 - Верхний для всех ниток
        /// </summary>
        UP_TO_ALL = 2,

        /// <summary>
        /// 3 - Нижний для всех ниток
        /// </summary>
        DOWN_TO_ALL = 3
    }

	public class Station : Entity
	{
        
		//Расстояние от линий  до центра для
		//станций с путевым развитием, в мм
		private const int lnDouble = 2;
		//Половина высоты минутных линий в мм и пикселах
		private const int lnMinMM = 2;
		                                                    //private const int lnMinPx = lnMinMM * mdlData.VpixelINmm;
		//Высота 30-секундных линий в мм и пикселах
		private const int lnSecMM = 2;
		                                                    //private const int lnSecPx = lnSecMM * mdlData.VpixelINmm;
        

		//Информация из базы данных

        /// <summary>
        /// Код данной станции в базе данных.
        /// </summary>
		public UInt32 code;

        /// <summary>
        /// Название данной станции
        /// </summary>
        public String name;

        /// <summary>
        /// Короткое название данной станции.
        /// </summary>
		public String shortName; 
      
		/// <summary>
		/// Ссылка на линию, на которой находится данная станция.        
		/// </summary>
		public Line line;    
      
		/// <summary>
		/// Признак станции с путевым развитием
		/// </summary>
		public bool flgStation;
		/// <summary>
		/// Признак видимости путевого развития
		/// </summary>
		public bool flgVisStation;

		//Направление станционных путей
		public Direction Napr;
        /*
		//Код направления станционных путей
		private int NaprCode;
        */
		public int CoordinateInMm;            //Координата центра станции на листе ватмана (мм)
    
        /// <summary>
        /// Координата центра станции, отображаемой на экране, в пикселях.
        /// </summary>
		public Int32 CoordinateInPixels;
		
        /// <summary>
        /// Признак конечной станции
        /// </summary>
		public EndStationDirection flgEndStation;
        /*
		//Признак возможности заполнения точек ночной
		//расстановки станции до общей расстановки
		public bool flgRasstanovka;
		//Признак возможности использования точки для
		//регулировки
		//xxx1 - используется при ручном изменении длины нитки
		//xx1x - используется для отстоев
		//x1xx - используется для ночной расстановки
		public int flgRegulirovka;
          */

        /// <summary>
        /// Ссылка на депо, с которым связана данная станция по 1-му пути.
        /// </summary>
		public Depot depotOnFirstWay;

        /// <summary>
        /// Ссылка на депо, с которым связана данная станция по 2-му пути.
        /// </summary>
		public Depot depotOnSecondWay;

        /// <summary>
        /// Ссылка на пункт осмотра, расположенный на станции
        /// </summary>
        public InspectionPoint inspectionPoint;

        /// <summary>
        /// Ссылка на пункт отстоя, расположенный на станции
        /// </summary>
        public NightStayPoint nightStayPoint;

        
		//++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		/// <summary>
        /// Коллекция заданий, которые начинаются на станции   
		/// </summary>
		public List<Task> colTasks = new List<Task>();
		//--------------------------------------------------------
		
        /// <summary>
        /// Номер задания, начинающегося на станции в стандартной нитке по первому пути
		/// </summary>
		public byte TaskNumberFirstTrack;

		/// <summary>
        /// Номер задания, начинающегося на станции в стандартной нитке по второму пути
		/// </summary>
		public byte TaskNumberSecondTrack;
        
		/// <summary>
        /// Коллекции точек ночной расстановки, расположенных на станции
		/// </summary>
		public IList colNightStayPoints;
        
        /// <summary>
        /// Коллекции точек ночной расстановки, расположенных на станции при нечетной расстановке
        /// </summary>
		public IList colNightStayPointsOdd;

        /// <summary>
        /// Коллекции точек ночной расстановки, расположенных на станции при четной расстановке
        /// </summary>
		public IList colNightStayPointsEven;


        /*
		//Private MasCardsStationEveningChet() As clsStrech
		//Private MasCardsStationEveningNeChet() As clsStrech
		//Private MasCardsStationMorningChet() As clsStrech
		//Private MasCardsStationMorningNeChet() As clsStrech
         */
		//Координаты линий станции,
		//отображаемой с путевым развититем
		public int CooUp;
		public int CooDown;
		//Вертикальная координата печати
		//названия станции на экране (пикселы)
		private int cooY0PrnPxl;
		private int cooY1PrnPxl;
         
		//Ссылка на вторую конечную станцию
		public Station TwoEnd;
        
		//Поля для лимитирующей станции
		//Лимитирующее направление
		public Direction LineDirection;
		public int BegLimTime;   //Начало лимитирующего интревала
		public int EndLimTime;   //Конец лимитирующего интревала
		public int NPoezdov;     //Число вводимых поездов
		//Источник вводимых поездов
		//public IstPoezdov As clsPointNight
		//Private MasArms() As TypArms
		//Флаг печати утренней или вечерней расстановки на этой станции
		//0 - не печатать
		//1 - утро
		//2 - вечер
		//3 - утро и вечер
		public int flgMourningEvening;
		//Длины маленькой и большой
		//вертикальных линий оборота в пикселах
		public int lenMinObPxS;
		public int lenMaxObPxS;
		public int lenRingObPxS;
		public int lenBigObPxS;
		public int lenExtrBigObPxS;
		//Длина вертикальных линий промежуточного
		//оборота пикселах
		public int lenObPxS;
		public int lenObbPxS;

         

        
		//Координаты
		//Флаг начала станции
		//0 - Верхний - для ниток, идущих вверх, нижний - для ниток, идущих вниз
		//1 - Нижний - для ниток, идущих вверх, верхний - для ниток, идущих вниз
		//2 - Верхний для всех ниток
		//3 - Нижний для всех ниток
		public int flgCooStart;
		//Флаг конца станции
		public int flgCooEnd;
		//Флаг СРВ
		public int flgCooSRV;
		//Координата станции начала при
		//движении поезда в направлении "1"
		public Single CooStartUp; 
		/// <summary>
		/// Координата станции начала при движении поезда в направлении "1" (планового графика)  1
		/// </summary>
		public Single CooStartUpPL;
		//Сдвиг координаты станции начала
		//относительно заданного в CooStartUp уровня
		//при движении поезда в направлении "1"
		public Single dCooStartUp;
		//Координата остановки на станции при
		//движении поезда в направлении "1"
		public Single CooStopUp;
		//Координата станции конца при
		//движении поезда в направлении "1" (планового графика)  1
		public Single CooStopUpPL;
         
		//Координата станции конца при
		//движении поезда в направлении "1"
		public Single CooEndUp;
		//Координата станции конца при
		//движении поезда в направлении "1" (планового графика)  1
		public Single CooEndUpPL;
		//Координата станции начала при
		//движении поезда в направлении "-1"
		public Single CooStartDown;
		//Координата станции начала при
		//движении поезда в направлении "-1" (планового графика) 1
		public Single CooStartDownPL;
		//Сдвиг координаты станции начала
		//относительно заданного в CooStartDown уровня
		//при движении поезда в направлении "-1"
		public Single dCooStartDown;
		//Координата остановки на станции при
		//движении поезда в направлении "-1"
		public Single CooStopDown;
		//Координата остановки на станции при
		//движении поезда в направлении "-1" (планового графика) 1
		public Single CooStopDownPL;
		//Координата станции конца при
		//движении поезда в направлении "-1"
		public Single CooEndDown;
		//Координата станции конца при
		//движении поезда в направлении "-1" (планового графика) 1
		public Single CooEndDownPL;
        
		//Сдвиг координаты станции конца
		//относительно заданного в CooStartUp уровня
		//при движении поезда в направлении "1"
		public Single dCooEndUp;
		//Сдвиг координаты станции конца
		//относительно заданного в CooStartDown уровня
		//при движении поезда в направлении "-1"
		public Single dCooEndDown;


		//Информация для работы с поездами
		//Коллекция поездов, занимающих
		//координату захода на станционные пути
		private IList PoezdaIn;
		//Коллекция поездов, занимающих координату
		//ухода со станционных путей
		private IList PoezdaFrom;
		//Желаемые координаты
		private Single GcooY1;
		private Single GcooY2;
		//Фактические координаты
		private Single FcooY1;
		private Single FcooY2;

		public IList colPoezdOborot;

		public IList colOtprPoezd1Way;
		public IList colOtprPoezd2Way;

		//Координаты точек отстоя для станций по 1,2,3,4 пути
		public int PointOtstoy1;
		public int PointOtstoy2;
		public int PointOtstoy3;
		public int PointOtstoy4;
        


        /*
               <table name="Станции">
		        <field name="Видимость" type="Да/Нет (bit|bool)" isPK="false"/>
		        <field name="Высота большого" type="Целое (int)" isPK="false"/>
		        <field name="Высота большого на промежуточной" type="Целое (int)" isPK="false"/>
		        <field name="Высота высокого оборота" type="Целое (int)" isPK="false"/>
		        <field name="Высота малого" type="Целое (int)" isPK="false"/>
		        <field name="Высота малого на промежуточной" type="Целое (int)" isPK="false"/>
		        <field name="Высота оборота на кольцевой линии" type="Целое (int)" isPK="false"/>
		        <field name="Высота очень высокого оборота" type="Длинное целое (long)" isPK="false"/>
		      * <field name="Код" type="Длинное целое (long)" isPK="true"/>
		        <field name="Конец 2 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Конец_лимитирующего времени" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Конец1 4 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Конец1 6 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Конец2 2 ручек" type="Целое (int)" isPK="false"/>
		        <field name="Конец2 4 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Конец2 6 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Конечная" type="Целое (int)" isPK="false"/>
		        <field name="Конечная_2" type="Целое (int)" isPK="false"/>
		        <field name="Координата" type="Целое (int)" isPK="false"/>
		        <field name="Координата конца" type="Целое (int)" isPK="false"/>
		        <field name="Координата начала" type="Целое (int)" isPK="false"/>
		        <field name="Координата СРВ" type="Целое (int)" isPK="false"/>
		      * <field name="Коротко" type="Текст (text)" isPK="false"/>
		        <field name="Лимитирующее_направление" type="Целое (int)" isPK="false"/>
		      * <field name="Линия" type="Длинное целое (long)" isPK="false"/>
		      * <field name="Название" type="Текст (text)" isPK="false"/>
		        <field name="Направление" type="Целое (int)" isPK="false"/>
		        <field name="Начало 2 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Начало_лимитирующего времени" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Начало1 4 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Начало1 6 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Начало2 2 ручек" type="Целое (int)" isPK="false"/>
		        <field name="Начало2 4 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Начало2 6 ручек" type="Дата и время (datetime)" isPK="false"/>
		        <field name="Откуда_вводим" type="Длинное целое (long)" isPK="false"/>
		        <field name="Плоский_Оборот" type="Да/Нет (bit|bool)" isPK="false"/>
		        <field name="Признак_возможности_использования_для_регулировки" type="Да/Нет (bit|bool)" isPK="false"/>
		        <field name="Признак_заполнения_до_ночной_расстановки" type="Да/Нет (bit|bool)" isPK="false"/>
		        <field name="Путь_смены_головы" type="Длинное целое (long)" isPK="false"/>
		        <field name="Расстановка" type="Текст (text)" isPK="false"/>
		        <field name="Ручки1" type="Целое (int)" isPK="false"/>
		        <field name="Ручки2" type="Целое (int)" isPK="false"/>
		        <field name="Ручки3" type="Целое (int)" isPK="false"/>
		        <field name="Сдвиг координаты конца вверх" type="Целое (int)" isPK="false"/>
		        <field name="Сдвиг координаты конца вниз" type="Целое (int)" isPK="false"/>
		        <field name="Сдвиг координаты начала вверх" type="Целое (int)" isPK="false"/>
		        <field name="Сдвиг координаты начала вниз" type="Целое (int)" isPK="false"/>
		        <field name="Секунды" type="Да/Нет (bit|bool)" isPK="false"/>
		        <field name="Станция" type="Да/Нет (bit|bool)" isPK="false"/>
		        <field name="Число_вводимых_поездов" type="Целое (int)" isPK="false"/>
	        </table>
         */

        public Station() { }

        /// <summary>
        /// Конструктор инициирующий не ссылочные поля.
        /// </summary>
        /// <param name="row"></param>
        public Station(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            name = row["Название"].ToString();

            shortName = row["Коротко"].ToString();

            //Станция
            flgStation = Convert.ToBoolean(row["Станция"].ToString());
            //Видимость
            flgVisStation = Convert.ToBoolean(row["Видимость"].ToString());

            //Координата середины станции
            CoordinateInMm = Convert.ToInt32(row["Координата"].ToString());
                                                                //CoordinateInPixels = CoordinateInMm * mdlData.VpixelINmm;

            //Конечная
            flgEndStation = (EndStationDirection)Convert.ToInt32(row["Конечная"].ToString());
        }

        /// <summary>
        /// Инициализатор ссылочных полей.
        /// </summary>
        /// <param name="row"></param>
		public void Initialize(DataRow row)
		{	    
            var lineCode = Convert.ToUInt32(row["Линия"].ToString());
			line = MovementSchedule.colLine.SingleOrDefault(l => l.code == lineCode);

            if (line != null)
            {
                line.colStations.Add(this);

                if (flgEndStation != EndStationDirection.NONE && !Convert.IsDBNull(row["Конечная_2"]))
                {
                    var stationCode = Convert.ToUInt32(row["Конечная_2"].ToString());
                    TwoEnd = MovementSchedule.colStation.SingleOrDefault(s => s.code == stationCode);
                    if (TwoEnd != null)
                    {
                        switch (flgEndStation)
                        {
                            case EndStationDirection.DOWN:
                                line.EndStationEven = this;
                                break;
                            case EndStationDirection.UP:
                                line.EndStationOdd = this;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        Error.showErrorMessage(new Station() {code = stationCode}, this);
                    }
                }
            }  
            else
            {
                Error.showErrorMessage(new Line() {code = lineCode}, this);
            }


            


            /**
             * TODO: Стоит обратить внимание на LINQ. Например, так:
             *  mdlData.colTask.Where(t => t.departureStation == this).Select(t => { colTasks.Add(t); return t; });
             */
            foreach (var task in MovementSchedule.colTask)
				if (task.departureStation == this)
					colTasks.Add(task);

            //Направление
            //mdlData.colStation[index - 1].LineCode = Convert.ToInt32(tabStation.Rows[index-1]["Направление"].ToString());
            var st = MovementSchedule.colStation.SingleOrDefault(s => s.code == code);
            //if (st != null)
            //    st.LineCode = Convert.ToInt32(row["Направление"].ToString());

			
			// TODO: Рефакторинг
			//Считываем направление
            Napr = null;
			if (!Convert.IsDBNull(row["Направление"]))
			{	
				var STDirection = (StationTrackDirection)Convert.ToInt32(row["Направление"].ToString());
                switch (STDirection)
                { 
                    case StationTrackDirection.EVEN:
                        Napr = line.evenDirection;
                        break;
                    case StationTrackDirection.NONE:
                        Error.showErrorMessage(new Direction(), this, "Отсутствует выбранное направление");
                        break;
                    case StationTrackDirection.ODD:
                        Napr = line.oddDirection;
                        break;
                }
			}

			/*
			toCalculateStartCoo();
			
			//    //Лимитирующее_направление
			//    //Obj.flgDop = Convert.ToBoolean(Tab1.Rows[ID]["Флаг_добавочной_линии"].ToString());
             */
		}

        /*
		/// <summary>
		/// процедура инициализации текущего элемента класса "Станции"
		/// </summary>
		/// <param name="Tab"></param>
		/// <param name="id"></param>
		public void Initialize(DataTable Tab, int id)
		{            
			//Код
			this.code = Convert.ToInt32(Tab.Rows[id]["Код"].ToString());
			//Название
			this.name = Tab.Rows[id]["Название"].ToString();
			//Коротко
			this.ShtName = Tab.Rows[id]["Коротко"].ToString();
			//Линия
			this.Line = mdlData.colLine[Convert.ToInt32(Tab.Rows[id]["Линия"].ToString()) - 1];
			//if (this.Line == md)
			//this.Line.colStations.Add(this);
			//Станция
			this.flgStation = Convert.ToBoolean(Tab.Rows[id]["Станция"].ToString());
			//Видимость
			this.flgVisStation = Convert.ToBoolean(Tab.Rows[id]["Видимость"].ToString());
           
			//Координата
			this.CoordinateInMm = Convert.ToInt32(Tab.Rows[id]["Координата"].ToString());
			this.CoordinateInPixels = this.CoordinateInMm * mdlData.VpixelINmm;
			//Конечная
			this.flgEndStation = Convert.ToInt32(Tab.Rows[id]["Конечная"].ToString());
			//Конечная_2
			if (this.flgEndStation != 0)
			{
				this.TwoEnd = mdlData.colStation[Convert.ToInt32(Tab.Rows[id]["Конечная_2"].ToString()) - 1];
				if (this.flgEndStation == 1)
					Line.EndStationOdd = this;
				else
					Line.EndStationEven = this;
			}
			foreach (Task task in mdlData.colTask)
				if (task.StanOtpr == this)
					colTasks.Add(task);

			//Направление
			//mdlData.colStation[index - 1].LineCode = Convert.ToInt32(tabStation.Rows[index-1]["Направление"].ToString());

			// TODO: Рефакторинг
			//Считываем направление
			if (!(Tab.Rows[id]["Направление"] is DBNull))
			{
                
				int STDirection = Convert.ToInt32(Tab.Rows[id]["Направление"].ToString());
				if (STDirection == 1)
					this.Napr = this.Line.Napr1;
				else if (STDirection == -1)
						this.Napr = this.Line.Napr_1;
					else
						this.Napr = null;
			}
			else
				this.Napr = null;

			toCalculateStartCoo();

			//    //Лимитирующее_направление
			//    //Obj.flgDop = Convert.ToBoolean(Tab1.Rows[ID]["Флаг_добавочной_линии"].ToString());
		}

        */
		/// <summary>
		/// Расчет начальных координат
		/// </summary>
		public void toCalculateStartCoo()
		{
			if (this.flgVisStation)
			{//Для станции с развитием координаты стоянки и отправления вверх и вниз совпадают                             
				                                                    //this.CooUp = this.CoordinateInPixels - lnDouble * mdlData.VpixelINmm;
				                                                    //this.CooDown = this.CoordinateInPixels + lnDouble * mdlData.VpixelINmm;
				this.CooStopUp = this.CooUp;
				this.CooStopDown = this.CooDown;
			}
			else
			{//Для станций без развития координаты стоянки и отправления различаются на 3 пиксела в каждую сторону
				this.CooUp = this.CoordinateInPixels;
				this.CooDown = this.CoordinateInPixels;
				                                                    //this.CooStopUp = this.CooUp - lnDouble * mdlData.VpixelINmm / 2F;
				                                                    //this.CooStopDown = this.CooUp + lnDouble * mdlData.VpixelINmm / 2F;
			}
			this.CooStartUp = this.CooUp;
			this.CooStartDown = this.CooDown;

			switch (this.flgCooStart)
			{
				case 0://Верхний - для ниток, идущих вверх, нижний - для ниток, идущих вниз
					this.CooStartUpPL = this.CooUp + this.dCooStartUp;
					this.CooStartDownPL = this.CooDown + this.dCooStartDown;
					break;
				case 1://Нижний - для ниток, идущих вверх, верхний - для ниток, идущих вниз
					this.CooStartUpPL = this.CooDown + this.dCooStartUp;
					this.CooStartDownPL = this.CooUp + this.dCooStartDown;
					break;
				case 2://Верхний для всех ниток
					this.CooStartUpPL = this.CooUp + this.dCooStartUp;
					this.CooStartDownPL = this.CooUp + this.dCooStartDown;
					break;
				case 3://Нижний для всех ниток
					this.CooStartUpPL = this.CooDown + this.dCooStartUp;
					this.CooStopDownPL = this.CooDown + this.dCooStartDown;
					break;
			}

			switch (this.flgCooEnd)
			{
				case 0:
					this.CooEndUpPL = this.CooDown + this.dCooEndUp;
					this.CooEndDownPL = this.CooUp + this.dCooEndDown;
					break;
				case 1:
					this.CooEndUpPL = this.CooUp + this.dCooEndUp;
					this.CooEndDownPL = this.CooDown + this.dCooEndDown;
					break;
				case 2:
					this.CooEndUpPL = this.CooUp + this.dCooEndUp;
					this.CooEndDownPL = this.CooUp + this.dCooEndDown;
					break;
				case 3:
					this.CooEndUpPL = this.CooDown + this.dCooEndUp;
					this.CooEndDownPL = this.CooDown + this.dCooEndDown;
					break;
			}

			switch (flgCooSRV)
			{
				case 0:
					this.CooStopUpPL = this.CooUp;
					this.CooStopDownPL = this.CooDown;
					break;
				case 1:
					this.CooStopUpPL = this.CooDown;
					this.CooStopDownPL = this.CooUp;
					break;
				case 2:
					this.CooStopUpPL = this.CooUp;
					this.CooStopDownPL = this.CooUp;
					break;
				case 3:
					this.CooStopUpPL = this.CooDown;
					this.CooStopDownPL = this.CooDown;
					break;                    
			}

		}

	}
}


