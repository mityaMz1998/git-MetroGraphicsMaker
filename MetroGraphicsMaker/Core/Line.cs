using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace Core
{
public class Line : Entity
    {

        //перечень переменных класса "Линии"
        //локальные
        //Расстояние от линий  до центра для
        //станций с путевым развитием, в мм
        private const int lnDouble = 2;
        //Половина высоты минутных линий в мм и пикселах
        private const int lnMinMM = 2;
        private const int lnMinPx = lnMinMM * MovementSchedule.VpixelINmm;
        //Высота 30-секундных линий в мм и пикселах
        private const int lnSecMM = 2;
        private const int lnSecPx = lnSecMM * MovementSchedule.VpixelINmm;

        //глобальные

        /// <summary>  
        /// Код (идентификатор записи в БД) данной линии.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Номер данной линии.
        /// </summary>
        public UInt32 number;

        /// <summary>
        /// Название данной линии.
        /// </summary>
        public String name;

        // TODO: переименовать, например в name in dative case
        public string NameDP;  //Название в дательном падеже

        /// <summary>
        /// Станция связи с другой линией.
        /// </summary>
        public Station crossStation;

        //Флаг добавочной линии
        // TODO: что это, переименовать для понятности
        public bool flgDop;
        //Флаг направления путей вверх
        //True - 1 путь
        //False - 2 путь
        // TODO: если два варианта, почему не бул, переименовать для понятности Put -> direction of way
        // лучше, если way go up или 
        public byte flgTrack;
        //++++++++++++++++++++++++++++++++
        //Коллекция станций линии
        // NOTE: Почему IList? Интерфейс может измениться?
        public IList<Station> colStations;
        //--------------------------------

        /// <summary>
        /// Конечная станция в нечётном направлении.
        /// </summary>
        public Station EndStationOdd;

        /// <summary>
        /// Конечная станция в чётном направлении.
        /// </summary>
        public Station EndStationEven;

        /// <summary>
        /// Нечётное направление данной линии (Napr1).
        /// </summary>
        public Direction oddDirection = new Direction();

        /// <summary>
        /// Чётное направление данной линии (Napr_1).
        /// </summary>
        public Direction evenDirection = new Direction();

        /// <summary>
        /// Направление данной линии для оборотных заданий
        /// </summary>
        public Direction noneDirection = new Direction();

        //Максимальное потребное количество составов
        //public int MaxPoezd;
        /// <summary>
        /// Максимальное количество элементов в массиве элементов расписания
        /// </summary>
        public byte MaxEl;
        //Вертикальные координаты вывода основного и
        //дополнительного текста
        public int yNMOsn;
        public int yNMDop;

        public int CooUp;
        public int CooDown;

        public Line() { }

        /// <summary>
        /// Конструктор инициализирующий не ссылочные поля.
        /// </summary>
        /// <param name="row"></param>
        public Line(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            name = row["Название"].ToString();

            NameDP = row["Название_1"].ToString();

            number = Convert.ToUInt32(row["Код_линии_метрополитена"].ToString());

            flgDop = Convert.ToBoolean(row["Флаг_добавочной_линии"].ToString());

            try
            {
                flgTrack = Convert.ToByte(row["Направление_вверх"].ToString());
            }
            catch
            {
                flgTrack = 1; // TODO: Стоит убрать хардкод и ввести константу для значения по умолчанию
            }

            colStations = new List<Station>();
        }

        /// <summary>
        /// Инициализатор устанавливающий ссылки.
        /// </summary>
        /// <param name="row"></param>
        public void Initialize(DataRow row)
        {
            if (!Convert.IsDBNull(row["Код_станции_связи"]))
            {
                var stationCode = Convert.ToInt32(row["Код_станции_связи"].ToString());
                crossStation = MovementSchedule.colStation.SingleOrDefault(s => s.code == stationCode);
            }

            // colStations = mdlData.colStation.Where(s => s.line.code == code).ToList<Station>();
        }

		public override string ToString()
		{
			return name;
		}
	}
}
