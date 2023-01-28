using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Messages;

using View;

namespace Core
{
    public class TrainPath : Entity 
    {
        public View.TrainPath ViewTrainPath;
        /// <summary>
        /// (~!~)Этого поля в виде view надо сделать не фиктивной ссылку на образ или будет падать программа.
        /// </summary>

        public int code;
        /// <summary>
        /// Название объекта на листах
        /// </summary>
        public string trainNumber;

        public AbstractTail LogicalTail = null;

        //public clsNitkaView NitkaView;
		/// <summary>
		/// Ссылка на "нитку", по которой поезд должен пойти после того, как закончится эта.
		/// </summary>
        public TrainPath NextThread=null;


		/// <summary>
		/// Ссылка на "нитку", по которой поезд шел до этого.
		/// </summary>
        public TrainPath BackThread=null;

        /// <summary>
        /// Ссылка на соседнюю вперёд нитку (nextPoezd)
        /// </summary>
        public TrainPath RightThread;

        /// <summary>
        /// Ссылка на соседнюю назад нитку (prevPoezd)
        /// </summary>
        public TrainPath LeftThread;

		// Направление нитки:
		//  -1 - вниз
		//   1 - вверх
		public Direction direction;

		/// <summary>
		/// Флаг резервной "нитки".
		/// </summary>
		public Boolean FlgReserve;
	
		/// <summary>
		/// Признак "пик"/"непик"
		/// </summary>
		public Boolean flgPeak;

        /// Массив элементов расписания, принадлежащих нитке
        /// </summary>
        public List<clsElementOfSchedule> MasElRasp;
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //При этом, последний элемент расписания указывается необоротный
        /// <summary>
        /// Номер первого нефиктивного элемента расписания
        /// </summary>
        public byte NumFirst;
        /// <summary>
        /// Номер последнего нефиктивного элемента расписания
        /// </summary>
        public byte NumLast;
        //---------------------------------------------------------------
        /// <summary>
        /// Номер элемента расписания, на котором у нитки есть изгиб, 0 - изгиба нет
        /// </summary>
        public byte NumIzgib;

        public Boolean flgSovp;
        
        public ThreadState flgStart = ThreadState.NONE;
        
        public ThreadState flgEnd = ThreadState.NONE;

        /// <summary>
        /// Флаг учет нитки при расчете парности
        /// </summary>
        public Boolean flgParnost;
        /// <summary>
        /// Флаг пересчёта коэффициента наклона
        /// </summary>
        public Boolean flgNaklonKoeff;
        /// <summary>
        /// Время отправления с первой станции линии (а не нитки) в виртуальных еденицах
        /// </summary>
        public int DepartureTimeFromFirstStation;
        /// <summary>
        /// Время хода по линии в виртуальных еденицах (с момента отправления с первой станции линии до
        /// момента прибытия на последнюю станцию линии и исключая сверхрежимные выдержки)        /// 
        /// </summary>
        public int Txl;

        /// <summary>
        /// Ссылка на указатель ночного отстоя в начале
        /// </summary>
        public NightStayPoint pntNightStart = null;
        /// <summary>
        /// Ссылка на указатель ночного отстоя в в конце
        /// </summary>
        public NightStayPoint pntNightEnd = null;

        public TrainPath() { }

        public TrainPath(int departureTimeStartFirstStation, Direction directionTrainPath, RegimeOfMotion regimeOfMotioneg, ListTrainPaths Master)
        {
            DepartureTimeFromFirstStation = departureTimeStartFirstStation;
            direction = directionTrainPath;
            flgPeak = (regimeOfMotioneg == RegimeOfMotion.Peak);
            MasElRasp = new List<clsElementOfSchedule>();


            var localScheduleElements = new List<clsElementOfSchedule>();
            switch (direction.value)
            {
                case DirectionValue.EVEN:
                    switch (regimeOfMotioneg)
                    {
                        case RegimeOfMotion.Peak:
                            localScheduleElements.AddRange(Master.StandartTrainPathEVENPeak.LogicalTrainPath.MasElRasp);
                            break;
                        case RegimeOfMotion.NonPeak:
                            localScheduleElements.AddRange(Master.StandartTrainPathEVENNonpeak.LogicalTrainPath.MasElRasp);
                            break;
                        default:
                            throw new ArgumentException("Wrong RegimeOfMotion in ListTrainPaths class", "original");
                    }
                    break;
                case DirectionValue.ODD:
                    switch (regimeOfMotioneg)
                    {
                        case RegimeOfMotion.Peak:
                            localScheduleElements.AddRange(Master.StandartTrainPathODDPeak.LogicalTrainPath.MasElRasp);
                            break;
                        case RegimeOfMotion.NonPeak:
                            localScheduleElements.AddRange(Master.StandartTrainPathODDNonpeak.LogicalTrainPath.MasElRasp);
                            break;
                        default:
                            throw new ArgumentException("Wrong RegimeOfMotion in ListTrainPaths class", "original");
                    }
                    break;
                default:
                    throw new ArgumentException("Wrong direction value for this train path.");
            }

            foreach (var element in localScheduleElements)
                MasElRasp.Add(new clsElementOfSchedule(element, DepartureTimeFromFirstStation));

            NumFirst = 0;

            NumLast = (byte)(MasElRasp.Count - 1);


            var tmpTrainPath = direction.ColThreads.SingleOrDefault(
                tp =>
                    tp.DepartureTimeFromFirstStation < DepartureTimeFromFirstStation && tp.RightThread != null &&
                    tp.RightThread.DepartureTimeFromFirstStation > DepartureTimeFromFirstStation);

            //if (tmpTrainPath == null)
            //{
            //    direction.ColThreads.AddFirst(leftThread, this);
            //}
            //else
            //{
            //direction.ColThreads.AddAfter(leftThread, this);                
            //}
            //this.RightThread = leftThread.RightThread;
            //if (!this.RightThread.Equals( null)) 
            //    {this.RightThread.LeftThread = this;}

            //this.LeftThread = leftThread;
            //if (!this.LeftThread.Equals(null))
            //{leftThread.RightThread = this;}
            //ViewTrainPath = new View.TrainPath(this, Master);

            directionTrainPath.ColThreads.AddLast(this);
        }


		public TrainPath(DataRow row)
		{
            code = Convert.ToInt16(row["Код_Нитки"].ToString());

            trainNumber = row["Номер_Поезда"].ToString();
            
            try
            {
                flgParnost = Convert.ToBoolean(row["Учет_в_парности"].ToString());
            }
            catch
            {
                flgParnost = true;
            }


		}

        public void Initialize(DataRow row, Boolean flgDouble = false)
        {
            Boolean flg = false;
            int np = 0,
            P = 0;
            byte StSt;



            //Коллекция "нечётных ниток" (ColN1)
            List<TrainPath> colOddThread = null; // new List<TrainPath>();

            // Коллекция "чётных ниток" (ColN_1)
            List<TrainPath> colEvenThread = null; // new List<TrainPath>();

            var lineCode = Convert.ToUInt32(row["Линия"].ToString());
            var localLine = MovementSchedule.colLine.SingleOrDefault(l => l.code == lineCode);
            if (localLine != null)
            {
 //               if (flgDouble)
 //               {
 ////                   colOddThread = localLine.oddDirection.colDoubleNitok;
 ////                   colEvenThread = localLine.evenDirection.colDoubleNitok;
 //               }
 //               else
 //               {
 //                   colOddThread = localLine.oddDirection.colThreads;
 //                   colEvenThread = localLine.evenDirection.colThreads;
 //               }

                if (!Convert.IsDBNull(row["Код_Следующей_Вперед"]))
                {
                    var threadCode = Convert.ToInt32(row["Код_Следующей_Вперед"].ToString());
                    switch (direction.value)
                    {
                        case DirectionValue.EVEN:
                            LeftThread = colEvenThread.ElementAt(threadCode);
                            break;
                        case DirectionValue.NONE:
                            // TODO: Обработка ошибочной ситуации.
                            break;
                        case DirectionValue.ODD:
                            LeftThread = colOddThread.ElementAt(threadCode);
                            break;
                    }
                    RightThread.LeftThread = this;
                }
            }
            else
            {
                Error.showErrorMessage(new Line {code = lineCode}, this);
            }


            FlgReserve = ((toCPoezd(ref flg) > MovementSchedule.dNumber) || (trainNumber == " "));

           

            if (!Convert.IsDBNull(row["Код_Следующей_Назад"]))
            {
                var threadCode = Convert.ToInt32(row["Код_Следующей_Назад"].ToString());
                switch (direction.value)
                {
                    case DirectionValue.EVEN:
                        RightThread = colEvenThread.ElementAt(threadCode);
                        break;
                    case DirectionValue.NONE:
                        // TODO: Обработка ошибочной ситуации.
                        break;
                    case DirectionValue.ODD:
                        RightThread = colOddThread.ElementAt(threadCode);
                        break;
                }
                LeftThread.LeftThread = this;
            }

            /*
             'Формируем коллекцию элементов расписания в нитке
             'Поскольку, у любой нитки должен быть, как минимум,
             'хоть один элемент расписания, не имеет смысла
             'делать проверку на удачный поиск первого из них
With RecElRasp
NumFirst = 0
.FindFirst "Код_Нитки = " & """" & Kod & """"
Do While .Fields("Код_Нитки") = Kod
    Set Vspom = New clsElementRasp
    Vspom.Init RecElRasp
    If Vspom.Zadanie.Direct = 0 Then
        CountElRasp = Napr.Line.MaxEl
    Else
        For StSt = 1 To Napr.Line.MaxEl
            If Vspom.Zadanie Is Napr.GetZadanie(StSt) Then
                CountElRasp = StSt
                If NumFirst = 0 Then NumFirst = StSt
                NumLast = CountElRasp
                Exit For
            End If
        Next StSt
    End If
    Set MasElRasp(CountElRasp) = Vspom
    Set Vspom = Nothing
    .MoveNext
    If .EOF Then
        Exit Do
    End If
Loop
End With
  
               'Корректируем элементы расписания (Только для Замоскворецкой линии)
If colLines(1).Nazv = "Замоскворецкая" Then
For StSt = NumFirst + 1 To NumLast
    If StSt = 1 Then Exit For
    With MasElRasp(StSt)
        If .TimeStop > 0 And StSt > 1 Then
            MasElRasp(StSt - 1).TimePrib = MasElRasp(StSt - 1).TimePrib - .TimeStop
        End If
    End With
Next StSt
End If
*/

            //"Достраиваем" фиктивные элементы расписания
            //Если у нитки нет первого нефиктивного элемента, то она непиковая
            flgPeak = (NumFirst > 0);
            //Определяем, пиковая нитка или нет
            //Предварительно она считается пиковой, иначе - она не имеет элементов расписания  


            //NumFirst - Задается значение в блоке, который необходимо на данном этапе пропустить


        }

        /// <summary>
        /// Инициализация объекта
        /// </summary>
        ///

        /*
        public void cnstrClsThread()
        {
            NitkaView = new clsNitkaView();
            NitkaView.cnstrClsNitkaView();
            NitkaView.thread = this;
            //Создаем массив элементов расписания
            MasElRasp = new clsElementOfSchedule[MovementSchedule.CurrentLine.MaxEl + 1];
            NumFirst = 1;
            NumLast = (byte)(MovementSchedule.CurrentLine.MaxEl - 1);
            for (int i = 1; i <= MovementSchedule.CurrentLine.MaxEl; i++)
                MasElRasp[i] = new clsElementOfSchedule();
            //this.NitkaText=...
        }*/

        /// <summary>
        /// Расчет времени хода по определенному количеству заданий
        /// </summary>
        /// <param name="Nf"></param>
        /// <param name="Nl"></param>
        /// <returns></returns>
        public Int32 toTxlUchastok(Byte Nf, Byte Nl)
        {
            Int32 result = MasElRasp[Nl - 1].arrivalTime - MasElRasp[Nf].departureTime;
            for (int i = Nf + 1; i < Nl; i++)
                result -= MasElRasp[i].GetTimeStop();
            return result;
        }

        /// <summary>
        /// Коррекция названия нитки
        /// </summary>
        /// <param name="d"></param>
        public void toKorrectPoezd(int d)
        {
            var bis = true;
            trainNumber = Convert.ToString(toCPoezd(ref bis) + d);
            if (bis)
                trainNumber += "бис";
        }

        /// <summary>
        /// Возврат времени выхода на линию
        /// </summary>
        /// <param name="flgTotal"></param>
        /// <returns></returns>
        public int toTimeVihod(Boolean flgTotal)
        {
            var index = flgTotal ? 1 : NumFirst;
            var elementOfSchedule = MasElRasp[index]; 
            var result = elementOfSchedule.departureTime;

            if (elementOfSchedule.GetTimeStop() != 0)
                result -= elementOfSchedule.task.toTStat(flgPeak) + elementOfSchedule.GetTimeStop();

            return result;
        }

        /// <summary>
        /// Возврат времени ухода с линии
        /// </summary>
        /// <param name="flgTotal"></param>
        /// <returns></returns>
        public int toTimeUhod(Boolean flgTotal)
        {
            int result = 0;
            //В VB скрыт
            //  If flgTotal Then
            //    If NumLast <> Napr.Line.MaxEl And NumLast > 0 Then
            //      TimeUhod = MasElRasp(NumLast).TimePrib
            //    Else: TimeUhod = MasElRasp(Napr.Line.MaxEl).TimeOtpr
            //    End If
            //  Else: TimeUhod = MasElRasp(Napr.Line.MaxEl).TimeOtpr
            //  End If

            if (flgTotal)
            {
                if (NumLast > 0 && NumLast != direction.Line.MaxEl - 1)
                    result = MasElRasp[direction.Line.MaxEl - 1].arrivalTime;
                else
                    result = MasElRasp[direction.Line.MaxEl].departureTime;
            }
            else
            {
                result = MasElRasp[direction.Line.MaxEl].departureTime;
            }
            /*
                if (NumLast == direction.line.MaxEl - 1)
                    result = MasElRasp[direction.line.MaxEl].TimeOtprav;
                else if (NumLast > 0)
                    result = MasElRasp[direction.line.MaxEl - 1].TimePribyt ?? 0;
                else
                    result = MasElRasp[direction.line.MaxEl].TimeOtprav;
             */
            return result;
        }

        /// <summary>
        /// Возврат числовой составляющей названия нитки
        /// </summary>
        /// <param name="bis">Обязательный параметр</param>
        /// <returns></returns>
        public Int32 toCPoezd(ref Boolean bis)
        {
            /*
            string s = trainNumber;
            if (trainNumber.IndexOf("бис") > 0)
            {
                while (s.IndexOf("бис") > 0)
                {
                    s.Trim();
                    s.Remove(s.Length - 3);
                    bis = true;
                }
                bis = true;
            }
            else
                bis = false;
            return Convert.ToInt32(s);
            */
            var localTrainNumber = trainNumber;
            var index = trainNumber.IndexOf("бис");
            if (bis = index > 0)
                localTrainNumber = trainNumber.Remove(index);
            return Convert.ToInt32(localTrainNumber);
        }

        /// <summary>
        /// Возврат окончания числовой составляющей названия нитки
        /// </summary>
        /// <param name="bis">Параметр используется внутри функции</param>
        /// <returns></returns>
        public int toCPoezdEnd(ref Boolean bis)
        {
            var index = trainNumber.IndexOf("бис");
            // строки неизменяемы, так что вызов метода не приведёт к изменению строки trainNumber, а лишь к созданию безымянной строки содержащей результат применения данного метода. 
            var result = Convert.ToInt32( (bis = index > 0) ? trainNumber.Remove(index) : trainNumber );
            if (result > MovementSchedule.dNumber)
                result -= MovementSchedule.dNumber + 100 * Convert.ToInt32(direction.Line.number);
            else if (LeftThread != null && LeftThread.FlgReserve && FlgReserve && pntNightStart != null && pntNightStart.depot != null)
                result -= MovementSchedule.dNumberBis;
            return result;
            /*
            int otvet = 0;
            string s = trainNumber;
            if (trainNumber.IndexOf("бис") > 0)
            {
                s.Trim();
                otvet = Convert.ToInt32(s.Remove(s.Length - 3));
                bis = true;
            }
            else
                otvet = Convert.ToInt32(trainNumber);

            if (otvet > mdlData.dNumber)
                otvet -= (int)(mdlData.dNumber + direction.line.number * 100);
            else if (leftThread != null)// If PrevPoezd.flgRezerv And flgRezerv And Not pntNightStart Is Nothing Then
                if ((leftThread.flgReserve) && (flgReserve) && (pntNightStart != null))
                {
                    //If Not pntNightStart.Depo Is Nothing Then CPoezdEnd = CPoezdEnd - dNomerBis
                }
            return otvet;
             */
        }
        // Julietta-0@yandex.ru

        /// <summary>
        /// Возврат времени отправления с первой станции
        /// </summary>
        /// <param name="flgTotal"></param>
        /// <returns></returns>
        public int toTimeStartThread(Boolean flgTotal)
        {
            int result = 0;
            if (flgTotal)
                result = MasElRasp[1].departureTime;
            else if (NumFirst == 0)
                result = MasElRasp[direction.Line.MaxEl].departureTime;
            else
                result = MasElRasp[NumFirst].departureTime;
            return result;
        }

        /// <summary>
        /// Возврат массива элементов расписания
        /// </summary>
        /// <param name="msElRasp"></param>
        /// <param name="NFirst"></param>
        /// <param name="NLast"></param>
        /// <param name="flgStEnd"></param>
        public void toGetMasElRasp(ref clsElementOfSchedule[] msElRasp, ref byte NFirst, ref byte NLast, Boolean flgStEnd = true)
        {
            Byte IMax = direction != null ? direction.Line.MaxEl : MovementSchedule.CurrentLine.MaxEl;

            msElRasp = new clsElementOfSchedule[IMax + 1];
            for (int i = 1; i <= IMax; i++)
                msElRasp[i] = MasElRasp[i];

            NFirst = NumFirst;
            NLast = NumLast;
            if (!flgStEnd)
            {
                if (flgStart == ThreadState.FROM_NIGHTSTAY && NFirst != 0)
                    NFirst++;
                if (flgEnd == ThreadState.TO_NIGHTSTAY)
                    NLast--;
            }
        }
        /// <summary>
        /// Найти поезд, который движется позже текущего на i ниток
        /// </summary>
        /// <param name="i"></param>
        /// <param name="flgRez"></param>
        /// <returns></returns>
        public TrainPath toGetRightPoezd(int i, Direction.FlgRez flgRez)
        {
            var currentThread = this;
            while (i > 0 && currentThread.RightThread != null)
            {
                i--;
                currentThread = currentThread.RightThread;
            }
            if (i > 0 && currentThread.RightThread == null)
            {
                currentThread = null;
            }
            return currentThread;
        }

        /// <summary>
        /// Найти поезд, который движется раньше текушего на i ниток
        /// </summary>
        /// <param name="i"></param>
        /// <param name="flgRez"></param>
        /// <returns></returns>
        public TrainPath toGetLeftPoezd(int i, Boolean flgRez = true)
        {
            var currentThread = this;
            while (i>0 && currentThread.LeftThread!=null)
            {
                i--;
                currentThread = currentThread.LeftThread;
            }
            if (i>0 && currentThread.LeftThread== null)
            {
                currentThread = null;
            }
            return currentThread;
        }
        /// <summary>
        /// Найти поезд, который прибывает на заданную станцию или отправляется с нее раньше текущего на i ниток
        /// </summary>
        /// <param name="i"></param>
        /// <param name="station"></param>
        /// <param name="flgPribOtpr"></param>
        /// <param name="flgRez"></param>
        /// <returns></returns>
        public TrainPath toGetLeftPoezdOnStation(ref int i, Station station, Boolean flgPribOtpr = true, Boolean flgRez = true)
        {
            /*
            TrainPath N = this;
            var e = new clsElementOfSchedule();

            //Вспомогательная переменная, для "по умолчанию" = 0
            byte vsp = 0;
            while ((i > 0) && (N != null))
            {
                N = N.leftThread;
                if (N != null)
                {
                    if (flgPribOtpr)
                        e = N.getElementOfScheduleByArrivalStation(station, ref vsp);
                    else
                        e = N.getElementOfScheduleByDepartureStation(station, ref vsp);
                    if (
             * (e != null) && (
             *  (!N.flgReserve) || (!flgRez)
             *  ) && (
             *  (flgPribOtpr) || (
             *      (!flgPribOtpr) &&
                        (N.NumLast >= ((N.direction.value == DirectionValue.ODD) ? station.TaskNumberFirstTrack : station.TaskNumberSecondTrack)))))
                        i--;
                }
            }
            return N;
            */

            /*
             // Код написанный по оригиналу на VB6 -- clsNitka.cls.
            var currentThread = this;
            while (i > 0 && currentThread.leftThread != null)
            {
                var element = currentThread.getElementOfScheduleByArrivalStation(station);
                if (element != null && currentThread.code < mdlData.dNumber)
                    i--;
                currentThread = currentThread.leftThread;
            }
            */

            Byte vsp = 0;
            var currentThread = this;
            while (i > 0 && currentThread.LeftThread != null)
            {
                var element = flgPribOtpr ? 
                    currentThread.getElementOfScheduleByArrivalStation(station, ref vsp) :
                    currentThread.getElementOfScheduleByDepartureStation(station, ref vsp);
                Byte taskNumber = 0;
                switch (currentThread.direction.value)
                { 
                    case DirectionValue.EVEN:
                        taskNumber = station.TaskNumberSecondTrack;
                        break;
                    case DirectionValue.NONE:
                        // TODO: Обработка ошибочной ситуации.
                        break;
                    case DirectionValue.ODD:
                        taskNumber = station.TaskNumberFirstTrack;
                        break;
                }

                if (element != null && (!currentThread.FlgReserve || !flgRez) && (flgPribOtpr || !flgPribOtpr && currentThread.NumLast >= taskNumber))
                    i--;

                currentThread = currentThread.LeftThread;
            }
             
            return currentThread;
        }   


        /// <summary>
        /// Возврат элемента расписания по станции прибытия (nullable). Прежнее название toGetElRaspStationPrib.
        /// </summary>
        /// <param name="station"></param>
        /// <param name="index">для "по умлочанию" присвоить 0</param>
        /// <returns></returns>
        public clsElementOfSchedule getElementOfScheduleByArrivalStation(Station station, ref byte index)
        {
            //Искать имеет смысл, если нитка проходит через искомую станцию          
            clsElementOfSchedule result = null;
            
            index = (direction.Equals(direction.Line.oddDirection) ? station.TaskNumberFirstTrack : station.TaskNumberSecondTrack);
            index--;
            
            if (index > 0)
            {
                result = GetElementOfSchedule(index);
                if (result == null)
                    index = 0;
            }

            return result;
        }

        /// <summary>
        /// Возврат элемента расписания по станции отправления (nullable). Прежнее название toGetElRaspStationOtpr
        /// </summary>
        /// <param name="station"></param>
        /// <param name="index">для "по умолчанию" присвоить 0"</param>
        /// <param name="flgOb"></param>
        /// <param name="flgTotal"></param>
        /// <returns></returns>
        public clsElementOfSchedule getElementOfScheduleByDepartureStation(Station station, ref Byte index, Boolean flgOb = true, Boolean flgTotal = false)
        {
            //Искать имеет смысл, если нитка проходит через искомую станцию
            switch (direction.value)
            { 
                case DirectionValue.EVEN:
                    index = station.TaskNumberSecondTrack;
                    break;
                case DirectionValue.NONE:
                    // TODO: Обработка ошибочной ситуации.
                    break;
                case DirectionValue.ODD:
                    index = station.TaskNumberFirstTrack;
                    break;
            }

            // clsElementOfSchedule result = null;
            if (index == direction.Line.MaxEl) 
                return null;
            var result = GetElementOfSchedule(index, flgTotal);
            if ((result != null) || (!flgOb)) 
                return result;
            index = 0;
            if (!MasElRasp[direction.Line.MaxEl].task.departureStation.Equals(station)) 
                return null;
            index = direction.Line.MaxEl;
            result = MasElRasp[index];
            return result;
        }

        /// <summary>
        /// Возврат элемента расписания по индексу. Прежнее название toGetElRasp
        /// </summary>
        /// <param name="index">индекс элемента</param>
        /// <param name="flgTotal"></param>
        /// <returns></returns>
        public clsElementOfSchedule GetElementOfSchedule(Int32 index, Boolean flgTotal = false)
        {
            /*
            // TODO: Проверить на идентичность.
            if (!flgTotal && NumFirst > 0 && NumLast > 0 && (NumFirst > index || NumLast < index) && index != direction.line.MaxEl)
                return null;
            return MasElRasp[index];
            */
            byte i;
            clsElementOfSchedule result = null;
            if (flgTotal)
            {
                result = MasElRasp[index];
            }
            else if (NumFirst > -1 && NumLast > -1)
            {
                if (NumFirst <= index && index <= NumLast || index == direction.Line.MaxEl)
                    result = MasElRasp[index];
                /*
                if ((index < NumFirst) || ((index > NumLast) && (index != direction.line.MaxEl)))
                {
                    return null;
                }
                result = MasElRasp[index];
                */
            }
            else
            {
                //'    If Napr Is Napr.Line.Napr1 Then
                //'      i = MasElRasp(Napr.Line.MaxEl).Zadanie.StanOtpr.NZadaniya1
                //'    Else
                //'      i = MasElRasp(Napr.Line.MaxEl).Zadanie.StanOtpr.NZadaniya_1
                //'    End If
                //'    If i = index Then Set GetElRasp = MasElRasp(index)
                if (index == 0)
                {
                    result = MasElRasp[index];
                }
            }
            return result;
        }


        //Добавил Петров Александр 03.06.2015 Begin
        public List<TrainPath> CreateListAll_clsThread(DirectionOfReading_clsThread DirectionOfReading)
        {
            List<TrainPath> AllLogicalTrainPaths = new List<TrainPath>();
            TrainPath th_clsThread = this;

            switch (DirectionOfReading)
            {
                case DirectionOfReading_clsThread.AllBack:
                    while (th_clsThread.BackThread != null)
                    {
                        th_clsThread = th_clsThread.BackThread;

                        AllLogicalTrainPaths.Add(th_clsThread);
                    }
                    break;
                case DirectionOfReading_clsThread.AllNext:
                    while (th_clsThread.NextThread != null)
                    {
                        th_clsThread = th_clsThread.NextThread;

                        AllLogicalTrainPaths.Add(th_clsThread);
                    }
                    break;
                default:
                    throw new System.ArgumentException("Unknown DirectionOfReading_clsThread (Неизвестное направление считывания) in TrainPath class (CreateListAll_clsThread)", "original");
            }

            return AllLogicalTrainPaths;
        }

        public enum DirectionOfReading_clsThread
        {
            /// <summary>
            /// Считывать все нитки находящиеся ДО текущей.
            /// </summary>
            AllBack,

            /// <summary>
            /// Считывать все нитки находящиеся ПОСЛЕ текущей.
            /// </summary>
            AllNext
        }
        //Добавил Петров Александр 03.06.2015 End
    }
}
