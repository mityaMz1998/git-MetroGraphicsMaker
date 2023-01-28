using System;
using Core;

namespace Converters
{
    /// <summary>
    /// Статический класс, хранящий временн ́ые константы и реализующих статические методы преобразования между астрономическим временем, количеством "метрополитеновских" секунд (числом  временнЫх интервалов) и строками.
    /// </summary>
    public static class TimeConverter
    {
        /* NOTE: Цитата из прежнего кода (Модуль временых констант -- mdlDataConstTime.bas) от 06.02.2002:
         *   'ЗА ЕДЕНИЦУ ВРЕМЕНИ ПРИНИМАЕМ 5 СЕКУНД.
         *   'РЕШЕНИЕ ОКОНЧАТЕЛЬНОЕ И ОБЖАЛОВАНИЮ НЕ ПОДЛЕЖИТ
         */
        /// <summary>
        /// Масштаб времени. Константа, обуславливающая размер временнОго интервала ("метрополитеноской" секунлы) из расчёта: временнОй интервал = 60 секунд / timeScale.
        /// </summary>
        public const Int32 timeScale = 5;

        /// <summary>
        /// Константа, означающая ноль в "метрополитеновских" секундах.
        /// </summary>
        public const Int32 zeroSeconds = 0;

        /// <summary>
        /// Количество "метрополитеновских" секунд (число временнЫх интервалов) в одной астрономической минуте.
        /// </summary>
        public const Int32 secondsInMinute = 60 / timeScale;

        /// <summary>
        /// Количество "метрополитеновских" секунд (число временнЫх интервалов) в одном астрономическом часе.
        /// </summary>
        public const Int32 secondsInHour = 60 * secondsInMinute;

        /// <summary>
        /// Количество "метрополитеновских" секунд (число временнЫх интервалов) в одних астрономических сутках.
        /// </summary>
        public const Int32 secondsInDay = 24 * secondsInHour;

        /// <summary>
        /// Коэффициент перехода от астрономических секунд к астрономическим суткам.
        /// </summary>
        public const Double koefSecondsToDays = 1.0 / 86400; // 86400 == 24 * 60 * 60


        /// <summary>
        /// Расчет пиксельной координаты по временной. Переводит время в пиксели
        /// </summary>
        /// <param name="time">Время</param>
        /// <param name="isLocalPixels">true - перевести время в координаты канвы рисования; false - перевести время в координаты всей временной оси</param>
        /// <returns>Число пикселей.</returns>
        public static Int32 SecondsToPixels(Int32 time, Boolean isLocalPixels = false)
        {
            if (!isLocalPixels)
                time -= MovementSchedule.MovementTime.begin;
            /*
             -                time -= MovementTime.begin;
             +                time -= MovementSchedule.MovementTime.begin;
             */
            return time * len05sec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="isLocalPixels"></param>
        /// <returns></returns>
        public static Int32 PixelsToSeconds(Int32 pixels, Boolean isLocalPixels = false)
        {
            if (!isLocalPixels)
                pixels += MovementSchedule.MovementTime.begin * len05sec;
            return pixels / len05sec;
            /*
             -                pixels += MovementTime.begin*len05sec;
             -            return pixels/len05sec;
             +                pixels += MovementSchedule.MovementTime.begin * len05sec;
             +            return pixels / len05sec;
            */
        }

        /// <summary>
        /// Приведение метрополитеновского времени к реальному времени в секундах.
        /// </summary>
        /// <param name="metroTime"></param>
        /// <returns></returns>
        public static Int32 GetRealTime(Int32 metroTime)
        {
            return metroTime * timeScale;
        }

        /// <summary>
        /// Метод, получающий из числа временнЫх интервалов астрономические часы, минуты и секунды.
        /// </summary>
        /// <param name="timeInSeconds">Количество "метрополитеновских" секунд (число временнЫх интервалов).</param>
        /// <returns>Астрономическое время (часы, минуты, секунды).</returns>
        private static DateTime getTimeFromSeconds(Int32 timeInSeconds)
        {
            // TODO: убрать багу secondsInDay % secondsInDay == 0
            timeInSeconds %= secondsInDay;

            var hours = timeInSeconds / secondsInHour;
            timeInSeconds %= secondsInHour;

            var minutes = timeInSeconds / secondsInMinute;
            timeInSeconds %= secondsInMinute;

            var seconds = timeInSeconds * timeScale;
            /*
            var time = baseAODay;
            time.AddHours(hours);
            time.AddMinutes(minutes);
            time.AddSeconds(seconds);
            return time;
            */
            return new DateTime(1899, 12, 30, hours, minutes, seconds);
        }

        /// <summary>
        /// Метод, переводящий астрономические дату и время в колличество "метрополитеновских" секунд (число временнЫх интервалов). Таким образом, в 1 (астрономической) минуте 12 ("метрополитеновских") секунд, согласно масштабу времени.
        /// </summary>
        /// <param name="time">Астрономичесие дата и время.</param>
        /// <returns>Колличество "метрополитеновсикх" секунд (число временнЫх интервалов).</returns>
        public static Int32 TimeToSeconds(DateTime time)
        {
            // День месяца для time (тип int, от 1 до 31) - 30 (таким образом от -29 до 1);
            // TODO: Зачем вычитаем 30 дней? Почему 31 число должно иметь 1 в сутках, а остальные числа - 0?

            /* NOTE: Так как метрополитен работает с 5:20 утра до 2:00 ночи, то мы залезаем в следующие сутки, что кодируется следующим образом -- пусть у нас есть переменная DateTime time, для которой мы задали только время равное 23:59:59, если мы прибавим к time 2 секунды, то получим значение timе равное 31.12.1899 00:00:01. В нормальном состоянии число дней равно 0.*/

            /* NOTE: [http://msdn.microsoft.com/ru-ru/library/system.datetime.tooadate.aspx]
             * Значение даты OLE-автоматизации реализовано как число с плавающей запятой, целая часть которого равна количеству дней, прошедших до или после полуночи 30 декабря 1899 г., а дробная часть которого представляет время дня (день разделен на 24 части).
             * Например, полночь 31-ого декабря 1899 года представлена как 1.0; а 6 часов утра 1-ого января 1900 года представлено как 2.25; полночь 29-ого декабря 1899 года представлена -1.0; и 6 часов утра 29-ого декабря 1899 года представлено как -1.25.
             * Базовой датой OLE-автоматизации является полночь 30 декабря 1899 года. Минимальная дата автоматизации OLE: полночь 1 января 0100 года. Максимальная дата OLE-автоматизации совпадает с DateTime.MaxValue — последним моментом 31 декабря 9999 года. */

            var dayOfMonth = (time.Day == 31) ? 1 : 0;
            var seconds = dayOfMonth * secondsInDay
                          + time.Hour * secondsInHour
                          + time.Minute * secondsInMinute
                          + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(time.Second) / timeScale));

            return seconds;
        }

        /// <summary>
        /// Метод, переводящий количество "метрополитеновских" секунд (число временнЫх интервалов) в астрономические дату и время. Таким образом, в 12 ("метрополитеновских") секунд равны одной (астрономической) минуте.
        /// </summary>
        /// <param name="timeInSeconds">Колличество "метрополитеновсикх" секунд (число временнЫх интервалов).</param>
        /// <returns>Астрономичесие дата и время.</returns>
        public static DateTime SecondsToTime(Int32 timeInSeconds)
        {
            // TODO: Никто нигде никому не обещает, что timeInSeconds >= 0
            var days = timeInSeconds / secondsInDay;
            var time = new DateTime(1899, 12, 30);

            /* NOTE: Мысли вслух. Переменная timeInSeconds имеет тип Int32 (целое число), константа mdlData.secondsInDay имеет тип Int32 (целое число), переменная days также имеет тип Int32 (целое число). Переменная days может иметь значение отличное от {0, 1} только в случае, если оно принадлежит множеству {2, 3, 4, ...}. Иными словами, когда переменная timeInSeconds охватывает временной промежуток больше либо равный 2 суткам.
             Данное предположение вступает в конфликт с логикой предметной области и возможно лишь в случае появления даты 31 декабря 1899 года. Но, чтобы её адекватно представить, в этом случае достаточно присвоить переменной days значение 1. */
            if (days > 1)
            {
                // BUGFIX: days = 1;
                days = 30;
            }
            else
            {
                time = getTimeFromSeconds(timeInSeconds);
            }
            // TODO: В случае если days > 1 имеем 30 + 30 дней. ПОЧЕМУ?
            // TODO: День 30 декабря 1899 года стоит сделать константой с объяснением 
            // какой сакральный смысл она несёт и почему выбрана именно эта дата.
            // TODO: Необходимо придумать что-то более элегантное, чем
            // return new DateTime(baseAODay.Year, baseAODay.Month, baseAODay.Day + days, hours, minutes, seconds);
            // к тому же не доконца ясна концепция с 30 днями и вообще чудесной датой 30 декабря 1899 года.
            // Объект класса (структуры) DateTime инстанцированный конструктором по умолчанию содержит дату и время 01/01/0001 12:00:00 AM
            // Вероятнее всего данная проблема прищла из MS Access.
            return time.AddDays(days);
        }

        /// <summary>
        /// Метод, переводящий количество "метрополитеновских" секунд (число временнЫх интервалов) в обычные секунды. Таким образом, в 12 ("метрополитеновских") секунд равны одной (астрономической) минуте.
        /// </summary>
        /// <param name="timeInSeconds">Колличество "метрополитеновсикх" секунд (число временнЫх интервалов).</param>
        /// <returns>Астрономичесие дата и время.</returns>
        public static Int32 SecondsMetroToPeopleSeconds(Int32 timeInSeconds)
        {
            return timeInSeconds * 5;
        }

        /// <summary>
        /// Метод, переводящий количество "метрополитеновских" секунд (число временнЫх интервалов) в строку вида д:ч:мм:сс, где д - число дней (один знак), ч - число часов (один знак), мм - число минут (два знака), сс - число секунд (два знака).
        /// </summary>
        /// <param name="timeInSeconds">Колличество "метрополитеновсикх" секунд (число временнЫх интервалов).</param>
        /// <returns>Строка вида день:час:минуты:секунды.</returns>
        public static String SecondsToString(Int32 timeInSeconds)
        {
            var resultString = "0:0:00:00";
            var days = timeInSeconds / secondsInDay;

            if (days <= 1)
            {
                var time = getTimeFromSeconds(timeInSeconds);
                resultString = String.Format("{0}:{1}", days, time.ToString("H:mm:ss"));
                //resultString = String.Format("{0}:{1}:{2:D2}:{3:D2}", days, time.Hour, time.Minute, time.Second);
            }
            return resultString;
        }

        /// <summary>
        /// Метод, преобразующий строку строку вида д:ч:мм:сс, где д - число дней (один знак), ч - число часов (один знак), мм - число минут (два знака), сс - число секунд (два знака) в количство "метрополитеновских" секунд (число временнЫх интервалов).
        /// </summary>
        /// <param name="str">Строка вида день:час:минуты:секунды.</param>
        /// <returns>Количство "метрополитеновских" секунд (число временнЫх интервалов).</returns>
        public static Int32 StringToSeconds(String str)
        {
            var time = str.Split(':');
            // TODO: Стоит продумать механизм оповещения о том, что-то пошло не так, если не удасться локализовать
            // проблему на этом уровне. Например, возвращать отрицательное число. Или пробросить exception наверх.
            var result = 0;

            try
            {
                result = Convert.ToInt32(time[0]) * secondsInDay
                       + Convert.ToInt32(time[1]) * secondsInHour
                       + Convert.ToInt32(time[2]) * secondsInMinute
                       + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(time[3]) / timeScale));
            }
            catch (IndexOutOfRangeException exception)
            {
                // TODO: Добавить обработку исключения System.IndexOutOfRangeException
                Console.WriteLine(exception);
            }
            catch (FormatException exception)
            {
                // TODO: Добавить обработку исключения System.FormatException
                Console.WriteLine(exception);
            }

            return result;
        }


        /// <summary>
        /// Округление времени до RoundPeak, RoundNonPeak
        /// </summary>
        /// <param name="valueToRound">Округляемая величина.</param>
        /// <param name="baseOfRounding">Основание округления, то есть величина, до кратной которой будет производится округление.</param>
        /// <returns>Значение округлённое до кратного основанию округления.</returns>
        public static Int32 RoundingToMultipleOfBase(Int32 valueToRound, Int32 baseOfRounding)
        {
            if (baseOfRounding == 1)
                return valueToRound;

            var modulo = valueToRound % baseOfRounding;
            if (modulo == 0)
                return valueToRound;

            valueToRound -= modulo;
            if (modulo >= baseOfRounding / 2 + 1)
                valueToRound += baseOfRounding;
            return valueToRound;
        }


        /* TODO: Где должны находиться данные временнЫе константы? */


        // Временной диапазон построения графика оборота составов (с точностью до 10 минут)
        // public const DateTime TimeStartOborot = DateTime.Parse("5:20:00 AM");
        // public const DateTime TimeFinishOborot = TimeStartOborot.Add("8:50:00 PM");
        // public const DateTime TimeStartOborotS = 5 * HourTime + 20 * MinuteTime;
        // public const DateTime TimeFinishOborotS = TimeStartOborotS + 20 * HourTime + 50 * MinuteTime;

        /// <summary>
        /// Время начала оборота подвижного состава (0 дней 5 часов 20 минут 0 секунд). Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени.
        /// </summary>
        public const Int32 timeStartMovement = 5 * secondsInHour + 20 * secondsInMinute;

        /// <summary>
        /// Время окончания оборота подвижного состава (1 день 2 часа 0 минут 0 секунд). Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени.
        /// </summary>
        public const Int32 timeFinishMovement = secondsInDay + 2 * secondsInHour;

        /// <summary>
        /// Константа, равная суткам (0 дней 20 часов 40 минут 0 секунд). Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени.
        /// </summary>
        public const Int32 timeDay = timeFinishMovement - timeStartMovement;
        // Public Const TimeDay = #11:59:59 PM# + #12:00:01 AM#

        /// <summary>
        /// Начало утреннего (первого) часа-пик в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        public const Int32 beginTimePeak1 = 7 * secondsInHour; //#7:00:00 AM#

        /// <summary>
        /// Окончание утреннего (первого) часа-пик в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        public const Int32 endTimePeak1 = 9 * secondsInHour; //#9:00:00 AM#

        /// <summary>
        /// Начало вечернего (второго) часа-пик в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        public const Int32 beginTimePeak2 = 17 * secondsInHour; //#5:00:00 PM#

        /// <summary>
        /// Окончание вечернего (второго) часа-пик в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        public const Int32 endTimePeak2 = 19 * secondsInHour; //#7:00:00 PM#


        // -----------------------

        /// <summary>
        /// Временной интервал движения по линии (0:05:20 -- 1:02:00)
        /// </summary>
        public static TimeInterval MovementTime = new TimeInterval(5 * secondsInHour + 20 * secondsInMinute, secondsInDay + 2 * secondsInHour);

        /// <summary>
        /// Временной интервал утреннего пика   
        /// </summary>
        public static TimeInterval MorningPeak = new TimeInterval(7 * secondsInHour, 9 * secondsInHour);

        /// <summary>
        /// Временной интервал вечернего пика
        /// </summary>
        public static TimeInterval EveaningPeak = new TimeInterval(17 * secondsInHour, 19 * secondsInHour);

        // ------------------------


        // Дополнительная временная зона к диапазону построения графика оборота (с точностью до 10 минут)
        // public const DateTime dTimeZonaGraphic = DateTime.Parse("12:10:00 AM");
        // public DateTime dTimeZonaGraphic;

        // Время прибытия первого поезда на предпоследнюю станцию в секундах
        // public const DateTime TimePribToPredPoslStation = HourTime * 6; //#6:00:00 AM#

        // Временная константа - 1 час. 3 мин. ночи в секундах
        // public const int TimeHourAM = DayTime + HourTime + MinuteTime * 3; //#11:59:59 PM# + #1:03:01 AM#
        public const Int32 TimeHourAM = secondsInDay + secondsInHour + secondsInMinute * 3; //#11:59:59 PM# + #1:03:01 AM#

        public static int TimeLastPassTrain;

        // Время подачи напряжения на линии в секундах
        // public Const TimeNapr = 5 * TimeHour + 25 * TimeMinute //#5:25:00 AM#

        // Время минимальной проверки составов на линии после подачи напряжения в секундах
        // Public Const MinTest = 15 * TimeMinute '#12:15:00 AM#

        // Время минимальной стоянки в секундах
        // Public Const TimeMinStop = 4 '#12:00:20 AM#

        // Время минимально-отображаемой сверхрежимной выдержки в секундах
        // Public Const MinVisStop = 2 '#12:00:10 AM#

        // Минимальный интервал между поездами в секундах
        // Public Const TimeMinIntr = TimeMinute + 3 '#12:01:15 AM#

        // Минимальное время между отправлением и прибытием поездов на одну платформу  в секундах
        // Public Const TimeMinOtprPr = TimeMinIntr - TimeMinStop

        // Время перехода от непиковых режимов к пиковым в секундах
        // Public TimePerehodPikNepik As Integer

        // Шаг перехода от непиковых режимов к пиковым в секундах
        // Public Const dTimePerehodPikNepik = 3 '#12:00:15 AM#

        // Шаг изменения времени хода при переходе от непиковых режимов к пиковым в секундах
        // Public Const dTimePerehod = 1 '#12:00:05 AM#

        // Время начала перехода от пиковых режимов к непиковым по 1 пути утром в секундах
        // Public Const TPerehodPikNepikUtro1 = 8 * TimeHour + 45 * TimeMinute '#8:45:00 AM#

        // Время начала перехода от пиковых режимов к непиковым по 2 пути утром в секундах
        // Public Const TPerehodPikNepikUtro2 = 8 * TimeHour + 45 * TimeMinute '#8:45:00 AM#

        // Время начала перехода от пиковых режимов к непиковым по 1 пути вечером в секундах
        // Public Const TPerehodPikNepikVecher1 = 19 * TimeHour '#7:00:00 PM#

        // Время начала перехода от пиковых режимов к непиковым по 2 пути вечером в секундах
        // Public Const TPerehodPikNepikVecher2 = 19 * TimeHour '#7:00:00 PM#

        // Величина сверхрежимной стоянки на первой станции для поездов, выходящих из тупика коленками назад в секундах
        //                             '6 * Time5Second '#12:00:30 AM#
        // Public Const TsFirstStation = 6 * Time5Second '2 * TimeMinute '#12:02:00 AM#

        // Шаг округления в час-пик в секундах
        // Public Const dTimeRoundPik = 1 '#12:00:05 AM#

        /// <summary>
        /// Шаг округления для часа-пик.
        /// </summary>
        public const Int32 STEP_OF_ROUNDING_ON_PEAK_TIME = 1;

        // Шаг округления в час-непик в секундах
        //                             'Делаем переменным
        // Public Const dTimeRoundNePik = 3 '#12:00:15 AM#

        /// <summary>
        /// Шаг округления для часа-непик.
        /// </summary>
        public const Int32 STEP_OF_ROUNDING_ON_NON_PEAK_TIME = 3;


        // Public dTimeRoundNePik As Integer

        // Шаг округления в час-пик, с
        // Public Const RoundPik = 1

        // Шаг округления в час-непик, c
        // Public Const RoundNePik = 3

        /// <summary>
        /// Константа, означающая, что ресурс по времени ничем не ограничен
        /// </summary>
        public const int TimeFull = 32676;

        // Время от занятия станционного индикатора до остановки 15 секунд
        // Public Const TimeStation = 3 * Time5Second

        // Время, после которого считается, что поезд ушел в отстой (10 минут)
        // Public Const TimeOtstoy = 10 * TimeMinute  

        //---------------------------


        /// <summary>
        /// Длина 5 секунд
        /// </summary>
        public const int len05sec = 2;

        /// <summary>
        /// Длина 15 секунд
        /// </summary>
        public const int len15sec = len05sec * 3;

        /// <summary>
        /// Длина 30 секунд
        /// </summary> 
        public const int len30sec = len15sec * 2;



        /// <summary>
        /// Расчет пиксельной координаты по временной. Переводит время в пиксели
        /// </summary>
        /// <param name="time">Время</param>
        /// <param name="localPixels">true - перевести время в координаты канвы рисования; false - перевести время в координаты всей временной оси</param>
        /// <returns></returns>
        public static Int32 TimeToPixels(Int32 time, Boolean localPixels = false)
        {
            // @TODO: Здесь какая-то ерунда!!!
            var result = 0;
            if (!localPixels)
            {
                //result = time * len05sec;
                time -= timeStartMovement;
            }
            else
            {
                // result = time * len05sec;
            }
            result = time * len05sec;
            return result;

            // return len05sec * (localPixels ? time : time - timeStartMovement);
        }
    }
}
