using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Core;
using Converters;

namespace Forms
{
    /// <summary>
    /// Вкладка для отображения графика оборота.
    /// </summary>
    public sealed class MovementScheduleTab : Window
    {
        public ScrollViewer hsb;

        public ScrollViewer vsb;

        private Int32 headerHeight = 15;

        private Single lengthOfBlock;

        private Int32 numberCellWidth = 32;

        private Int32 pointNameCellWidth = 128;

        private Int32 rowHeight = 32;

        private Int32 timeGridHeight = 2;

        private Int32 timeGridStep = 5;

        private Int32 merge = 3;

        private Int32 padding = 2;


        private Grid layout;
        /// <summary>
        /// Цвет сопроводиловки
        /// </summary>
        private Brush otherColor = Brushes.WhiteSmoke;

        /// <summary>
        /// Цвет бланка, на котором отображается ГО
        /// </summary>
        private Brush blankColor = Brushes.White;

        private Image[] sideBoxes = new Image[2];

        private Image[] topSideBoxes = new Image[2];

        private Image[] timeLineBoxes = new Image[2];

        private Image center;

        private Image top;

        private Image bottom;


        /// <summary>
        /// Стандартное перо.
        /// </summary>
        public Pen StandardPen = new Pen(Brushes.Black, 1f);

        public FontFamily font;

        private windows.Point localStartPoint;

        /// <summary>
        /// 
        /// </summary>
        public StringFormat LegendFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.Word
        };


        public MovementScheduleTab()
        {
            font = new Font(FontFamily.GenericMonospace, 10 * MovementSchedule.scale, FontStyle.Regular);

            localStartPoint = new windows.Point();

            Text = Resources.MovementScheduleTabTitle;
            Location = new Point(4, 22);
            Name = "MSTab";
            Padding = new Padding(3);
            Size = new Size(931, 388);
            TabIndex = 1;
            UseVisualStyleBackColor = true;

            layout = new Grid
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Margin = new Padding(0),
                Padding = new Padding(0),
                ColumnCount = 4,
                RowCount = 3
            };

            top = new Image
            {
                BackColor = otherColor,
                Name = "top",
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            vsb = new VScrollBar
            {
                Width = 20,
                Dock = DockStyle.Fill,
                Name = "innerVScroll",
                Visible = true,
                Minimum = 0,
                Maximum = MovementSchedule.colRoute == null ? 0 : MovementSchedule.colRoute.Count,
                AutoSize = true,
                LargeChange = 1
            };
            vsb.Value = vsb.Minimum;
            vsb.Scroll += OnScroll;

            topSideBoxes[0] = new Image
            {
                BackColor = otherColor,
                Name = "topLeft",
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            topSideBoxes[1] = new Image
            {
                BackColor = otherColor,
                Name = "topRight",
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            sideBoxes[0] = new Image
            {
                BackColor = otherColor,
                Name = "left",
                Padding = new Padding(0),
                Margin = new Padding(0),
                Dock = DockStyle.Fill
            };

            center = new Image
            {
                BackColor = blankColor,
                Name = "centerBox",
                Padding = new Padding(0),
                Margin = new Padding(0),
                Dock = DockStyle.Fill
            };

            sideBoxes[1] = new Image
            {
                BackColor = otherColor,
                Name = "right",
                Padding = new Padding(0),
                Margin = new Padding(0),
                Dock = DockStyle.Fill
            };

            bottom = new Image
            {
                BackColor = blankColor,
                Name = "bottom",
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            SizeChanged += Resizing;
            Scroll += Scrolling;

            topSideBoxes[0].Paint += topSideBox_Paint;
            topSideBoxes[1].Paint += topSideBox_Paint;
            sideBoxes[0].Paint += sideBox_Paint;
            sideBoxes[1].Paint += sideBox_Paint;
            top.Paint += topBox_Paint;
            center.Paint += centerBox_Paint;
            bottom.Paint += bottomBox_Paint;

            layout.Children.Clear();
            layout.Children.Add(topSideBoxes[0], 0, 0);
            layout.Children.Add(top, 1, 0);
            layout.Children.Add(topSideBoxes[1], 2, 0);
            layout.Children.Add(sideBoxes[0], 0, 1);
            layout.Children.Add(center, 1, 1);
            layout.Children.Add(sideBoxes[1], 2, 1);
            layout.Children.Add(bottom, 0, 2);
            layout.SetColumnSpan(bottom, 3);

            layout.Children.Add(vsb, 3, 0);
            layout.SetRowSpan(vsb, 3);

            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));

            layout.ColumnStyles.Clear();
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));

            Controls.Add(layout);
            // Controls.Add(vsb);
        }

        private void OnScroll(object sender, ScrollEventArgs scrollEventArgs)
        {
            var scroll = (sender as VScrollBar);
            if (scroll != null)
            {
                var value = scrollEventArgs.NewValue - scrollEventArgs.OldValue;
                MovementSchedule.initialPixelsV += value;
                //Logger.Output(String.Format("[V]scroll.OldValue = {0}; [V]scroll.NewValue = {1}", scrollEventArgs.OldValue, scrollEventArgs.NewValue), "MSTab.OnScroll");
                //MessageBox.Show(scroll.Name + "; my Value = " + Value);
            }
            Refresh();
            Update();
        }

        /// <summary>
        /// Метод задания и пересчёта размеров компонентов ГО.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Resizing(object sender, EventArgs args)
        {
            var topBorder = 5;
            var sideBorder = 5;

            //var sideWidth = 1650;

            var totalW = (Width - 2 * (numberCellWidth + pointNameCellWidth)) * MovementSchedule.scale;

            var w = (int)(totalW / (10 * TimeConverter.secondsInMinute));
            timeGridStep = 1;
            var centerWidth = (timeGridStep * (TimeConverter.timeDay + TimeConverter.secondsInHour / 2)) / (10 * TimeConverter.secondsInMinute);
            var topHeight = 70;

            var sideWidth = numberCellWidth + pointNameCellWidth;
            // Resize
            topSideBoxes[0].Size = new Size(sideWidth, topHeight);
            top.Size = new Size(layout.Width - 2 * topSideBoxes[0].Width, topHeight);
            topSideBoxes[1].Size = topSideBoxes[0].Size;
            sideBoxes[0].Size = new Size(topSideBoxes[0].Width, Height - 2 * topHeight);
            sideBoxes[1].Size = sideBoxes[0].Size;
            center.Size = new Size(top.Width, sideBoxes[0].Height);
            bottom.Size = new Size(layout.Width, topHeight);

            lengthOfBlock = center.Width * 10.0f * TimeConverter.secondsInMinute / (TimeConverter.timeDay + 30.0f * TimeConverter.secondsInMinute);

        }

        public void Scrolling(object sender, ScrollEventArgs scrollEventArgs)
        {
            if (!Visible)
                return;

            ScrollBar scrollbar;
            if ((scrollbar = (sender as VScrollBar)) != null)
            {
                //MessageBox.Show(scrollbar.GetType().FullName + ": СКР-СКР!");
                MovementSchedule.initialPixelsV += scrollEventArgs.NewValue - scrollEventArgs.OldValue;
                localStartPoint.Y = scrollEventArgs.NewValue;
            }
            else if ((scrollbar = (sender as HScrollBar)) != null)
            {
                //MessageBox.Show(scrollbar.GetType().FullName + ": Вжих-Вжих");
                MovementSchedule.initialPixelsH2 += scrollEventArgs.NewValue - scrollEventArgs.OldValue;
                localStartPoint.X = scrollEventArgs.NewValue;
            }

            MessageBox.Show(String.Format("new Y Value = {0}", MovementSchedule.initialPixelsV));
            Refresh();
            Update();
        }

        private void topSideBox_Paint(object sender, PaintEventArgs args)
        {
            var graph = args.Graphics;
            var pb = sender as PictureBox;
            if (pb == null)
                return;

            var width = pb.Width - 1;
            var height = pb.Height - 1;
            var xCoordinates = new Int32[3];
            var text = "empty";
            if (pb.Name.Equals("topLeft"))
            {
                xCoordinates[0] = 0;
                xCoordinates[1] = numberCellWidth;
                xCoordinates[2] = width;
                text = "Выход составов";
            }
            else if (pb.Name.Equals("topRight"))
            {
                xCoordinates[0] = width - numberCellWidth;
                xCoordinates[1] = 0;
                xCoordinates[2] = width;
                text = "Заход составов";
            }

            // решётка
            var startY = 0;
            var startX = 0;
            graph.DrawLine(StandardPen, startX, startY, width, startY);
            graph.DrawLine(StandardPen, startX, height, width, height);
            foreach (var x in xCoordinates)
                graph.DrawLine(StandardPen, x, startY, x, height);

            // вывод текста
            var number = "№";
            var delta = 2 * MovementSchedule.scale;
            var yCoordinate = 10f;
            graph.DrawString(number, font, Brushes.Black, xCoordinates[0] + delta, yCoordinate);
            graph.DrawString(text, font, Brushes.Black, xCoordinates[1] + delta, yCoordinate);

        }

        private void topBox_Paint(object sender, PaintEventArgs args)
        {
            var graph = args.Graphics;
            var pb = sender as PictureBox;
            if (pb == null)
                return;

            var width = pb.Width - 1;
            var height = pb.Height - 1;

            // решётка
            var startY = 0;
            var startX = 0;
            var lineH = Convert.ToInt32(0.8 * height);
            var lineW = 10 * mdlData.scale;
            graph.DrawLine(StandardPen, startX, startY, width, startY);
            // временная линия
            graph.DrawLine(StandardPen, startX, height, width, height);
            graph.DrawLine(StandardPen, startX, lineH - padding, width, lineH - padding);

            // временные линии с часами
            for (var currentTime = TimeConverter.MovementTime.begin;
                currentTime <= TimeConverter.MovementTime.end + TimeConverter.secondsInHour / 2;
                currentTime += TimeConverter.secondsInHour)
            {
                var localTime = (currentTime / TimeConverter.secondsInHour + 1) % 24;
                var currentX = ConvertTimeToPixelsWithScale(currentTime, true) - lineW * (localTime > 9 ? 1 : 0.5f);
                graph.DrawString(localTime.ToString("D"), font, Brushes.Black, currentX, lineH);
                // graph.DrawLine(StandardPen, currentX, lineH, currentX, pb.Height);
            }
        }

        private Route routeByOrdinate(Double ordinate)
        {
            return MovementSchedule.colRoute[Convert.ToInt32(Math.Floor(ordinate / rowHeight))];
        }

        private Int32 getMaxVisibleNumber(Int32 height)
        {
            return Convert.ToInt32(Math.Ceiling(height / (rowHeight * MovementSchedule.scale)) + vsb.Value);
        }

        private void sideBox_Paint(object sender, PaintEventArgs args)
        {
            var graph = args.Graphics;
            var pb = sender as PictureBox;
            if (pb == null)
                return;

            //var vsb = VerticalScroll.Value;


            // Положение бегунков у полос прокрутки окна графика 
            //          и их максимальное значение (mdlData.bas)
            // Public ValueHScr As Long, 
            //        ValueVScr As Long
            // Public MaxHScr As Integer, 
            //        MaxVScr As Integer
            //
            // __________________ Переменные масштаба и размеров ____________________________
            // Public PxInTw As Single      'Отношение пикселов к твипам
            // Public HeightSys As Single   'Высота системной шапки
            // public lenminintr as long    'длина минимального интервала
            // public lenminotprpr as long  'разница между прибытием и отправлением
            // public lenminstop as long    'длина минимальной стоянки
            // public lenminvisstop as long 'длина минимально-отображаемой сверхрежимной выдержки
            // public scalex as single      'масштаб по оси х (для принтера)
            // public scaley as single      'масштаб по оси y (для принтера)
            //
            //                             'длина суток (для констант слишком велика,
            //                             'поэтому записываем в переменные,
            //                             'а потом будем инициализировать)
            // public lenday as long

            var width = pb.Width - 1;
            var height = pb.Height - 1;
            var xCoordinates = new Int32[3];
            if (pb.Name.Equals("left"))
            {
                xCoordinates[0] = 0;
                xCoordinates[1] = numberCellWidth;
                xCoordinates[2] = width;
            }
            else if (pb.Name.Equals("right"))
            {
                xCoordinates[0] = width;
                xCoordinates[1] = xCoordinates[0] - numberCellWidth;
                xCoordinates[2] = 0;
            }

            var startY = 0;
            var startX = 0;
            //graph.DrawLine(StandardPen, startX, startY, width, startY);
            //graph.DrawLine(StandardPen, startX, height, width, height);
            foreach (var x in xCoordinates)
                graph.DrawLine(StandardPen, x, startY, x, height);

            // отрисовка станций и линий ...
            var movementPen = Pens.Black;
            var brush = new SolidBrush(Color.Black);
            //var str = route.number.ToString();
            // var graph = e.Graphics;
            // var rect = new RectangleF(cellBounds[0], ordinate - font.Height - 1f, cellBounds[1], ordinate);
            //LegendFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, str.Length) });
            //var reg = graph.MeasureCharacterRanges(str, font, rect, LegendFormat);
            //var rect1 = reg.First().GetBounds(graph);
            //graph.DrawString(str, font, brush, rect1, LegendFormat);

            try
            {
                var ordinate = startY + rowHeight * MovementSchedule.scale;

                Logger.Output(localStartPoint.Y.ToString("C"));
                for (var y = 0f; y <= width; y += rowHeight * MovementSchedule.scale)
                {
                    var r = routeByOrdinate(localStartPoint.Y + y);
                    Logger.Output(r.PrintToHtml());
                }

                if (vsb.Maximum == 0 && MovementSchedule.colRoute != null)
                    vsb.Maximum = MovementSchedule.colRoute.Count;

                // @TODO: Выводить только видимые!!!
                for (var i = vsb.Value; i <= getMaxVisibleNumber(height) && i < MovementSchedule.colRoute.Count; i++)
                {
                    var route = MovementSchedule.colRoute[i];
                    var numberStr = route.Number.ToString("D");


                    var numberRect = new RectangleF(xCoordinates[0] + padding * MovementSchedule.scale, ordinate - (rowHeight - padding) * MovementSchedule.scale, numberCellWidth, (rowHeight - padding) * MovementSchedule.scale);
                    var pointRect = new RectangleF(xCoordinates[1], ordinate - (rowHeight - padding) * MovementSchedule.scale, width - numberCellWidth, (rowHeight - padding) * MovementSchedule.scale);

                    NightStayPoint point = null;
                    if (pb.Name.Equals("left"))
                    {
                        point = route.prevRoute.nightStayPoint;
                    }
                    else if (pb.Name.Equals("right"))
                    {
                        point = route.nightStayPoint;
                        numberRect.X = xCoordinates[1] + padding * MovementSchedule.scale;
                        pointRect.X = 0 + padding * MovementSchedule.scale;
                    }

                    var pointStr = "";
                    if (point == null)
                        pointStr = "Линия";
                    else if (point.depot == null)
                        pointStr = String.Format("{0} {1}", point.station.shortName, point.name);
                    else
                        pointStr = String.Format("Депо \u00ab{0}\u00bb", point.depot.name); // кавычки


                    LegendFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, numberStr.Length) });


                    /*
                    var rect1 = graph.MeasureCharacterRanges(numberStr, font, numberRect, LegendFormat).First().GetBounds(graph);
                    

                    var rect2 = graph.MeasureCharacterRanges(pointStr,  font, pointRect,  LegendFormat).First().GetBounds(graph);

                    graph.DrawString(numberStr, font, brush, rect1, LegendFormat);
                    graph.DrawString(pointStr, font, brush, rect2, LegendFormat);
                    */

                    graph.DrawString(numberStr, font, brush, numberRect);
                    graph.DrawString(pointStr, font, brush, pointRect);

                    graph.DrawLine(StandardPen, 0, ordinate, width, ordinate);
                    //graph.DrawLine(StandardPen, 0, ordinate - timeGridHeight * mdlData.scale, width, ordinate - timeGridHeight * mdlData.scale);


                    ordinate += (rowHeight + merge) * MovementSchedule.scale;
                }
            }
            catch
            {
            }

        }

        private Single ConvertTimeToPixelsWithScale(Int32 value, Boolean flag = false)
        {
            var tmp1 = ((TimeConverter.MovementTime.begin / TimeConverter.secondsInHour + 1) * TimeConverter.secondsInHour -
                        TimeConverter.MovementTime.begin);
            // var tmp = TimeConverter.secondsInMinute*40;
            var time = flag ? tmp1 : 0;
            var secondsInPixel = 1.3;
            return Convert.ToSingle(MovementSchedule.scale * (value + time - TimeConverter.MovementTime.begin) / (secondsInPixel * TimeConverter.secondsInMinute));
        }

        private void makeHourLines(Graphics graph, PictureBox pb)
        {
            var height = pb.Height - 1;
            for (var currentTime = TimeConverter.MovementTime.begin;
                currentTime <= TimeConverter.MovementTime.end;
                currentTime += TimeConverter.secondsInHour)
            {
                var currentX = ConvertTimeToPixelsWithScale(currentTime, true);
                graph.DrawLine(StandardPen, currentX, 0, currentX, height);
            }
        }

        private void centerBox_Paint(object sender, PaintEventArgs args)
        {
            var graph = args.Graphics;
            var pb = sender as PictureBox;
            if (pb == null)
                return;

            //var size = pb.Parent.Parent.Size();

            var width = pb.Width - 1;
            var height = pb.Height - 1;

            //var contRect = new Rectangle(0, 0, width, height);
            // graph.FillRectangle(Brushes.White, contRect);
            //graph.DrawRectangle(StandardPen, contRect);

            var movementElementHeight = 2;
            var movementPen = new Pen(Color.Blue, mdlData.scale < 1 ? movementElementHeight : movementElementHeight * mdlData.scale);

            makeHourLines(graph, pb);
            var rNumber = "";
            try
            {
                var ordinate = rowHeight * MovementSchedule.scale;
                var timeLine = timeGridHeight * MovementSchedule.scale;

                var routes =
                    MovementSchedule.colRoute;//.Where(r => r.depot.name.ToLowerInvariant().Equals("Свиблово".ToLowerInvariant())).ToList();

                for (var i = vsb.Value; i < getMaxVisibleNumber(height) && i < MovementSchedule.colRoute.Count; ++i)
                {
                    var route = routes[i];
                    rNumber = "Маршрут №" + route.Number.ToString("000");
                    // горизонтальные линии сетки маршрута
                    graph.DrawLine(StandardPen, 0, ordinate, width, ordinate);
                    graph.DrawLine(StandardPen, 0, ordinate - timeLine, width, ordinate - timeLine);
                    // вертикальная решётка с шагом сетки
                    for (var currentTime = TimeConverter.MovementTime.begin;
                        currentTime <= TimeConverter.MovementTime.end + TimeConverter.secondsInHour / 2;
                        currentTime += 10 * TimeConverter.secondsInMinute)
                    {
                        var currentX = ConvertTimeToPixelsWithScale(currentTime);
                        graph.DrawLine(StandardPen, currentX, ordinate, currentX, ordinate - timeLine);
                    }
                    /*
                    var x = timeGridStep*mdlData.scale;
                    while (x < width)
                    {
                        var currentX = ConvertTimeToPixelsWithScale(x);
                        graph.DrawLine(StandardPen, currentX, ordinate, currentX, ordinate - timeGridHeight*mdlData.scale);
                        currentX += timeGridStep*mdlData.scale;
                    }

                    */
                    // отрисовка моментов, когда ездим
                    var row = rowHeight * MovementSchedule.scale;
                    if (route.prevRoute.nightStayPoint != null && route.prevRoute.nightStayPoint.depot != null)
                    {
                        if (route.nightStayPoint != null && route.nightStayPoint.depot != null)
                            movementPen.Color = Color.ForestGreen;
                        else
                            movementPen.Color = Color.YellowGreen;
                    }
                    else
                    {
                        movementPen.Color = Color.Blue;
                    }
                    /*
                    var fakeRepairs = route.Repairs.Where(r => r.IsFake);
                    foreach (var fakeRepair in fakeRepairs)
                    {
                        var begin = ConvertTimeToPixelsWithScale(fakeRepair.beginTime);
                        var end = ConvertTimeToPixelsWithScale(fakeRepair.endTime);

                        graph.DrawLine(new Pen(Color.Red), begin, ordinate-5, end, ordinate-5);
                       
                        
                    }
                    */



                    // высота риски-флажка указывающего время (минуты) начала/конеца эл-та ГО
                    // TODO: Рисуем только то, что видим!!!
                    var hLine = ordinate - 0.5f * row;
                    foreach (var el in route.ElementsOfMovementSchedule)
                    {
                        var begin = ConvertTimeToPixelsWithScale(el.beginTime);
                        var end = ConvertTimeToPixelsWithScale(el.endTime);

                        graph.DrawLine(movementPen, begin, ordinate, end, ordinate);
                        graph.DrawLine(StandardPen, begin, ordinate, begin, hLine);
                        graph.DrawLine(StandardPen, end, ordinate, end, hLine);
                        // нужен собственный тип времени с часами, минутами и проч.
                        var bTime = (el.beginTime % TimeConverter.secondsInHour) / TimeConverter.secondsInMinute;
                        var eTime = (el.endTime % TimeConverter.secondsInHour) / TimeConverter.secondsInMinute;
                        graph.DrawString(bTime.ToString("00"), font, Brushes.Black, new PointF(begin, hLine));
                        graph.DrawString(eTime.ToString("00"), font, Brushes.Black, new PointF(end, hLine));
                    }

                    var repairPen = new Pen(Color.Chartreuse, 2);
                    foreach (var repair in route.Repairs.Where(r => !r.IsFake))
                    {
                        var bTime = ConvertTimeToPixelsWithScale(repair.beginTime);
                        var eTime = ConvertTimeToPixelsWithScale(repair.endTime);
                        graph.DrawLine(repairPen, new PointF(bTime, hLine), new PointF(eTime, hLine));
                        graph.DrawLine(repairPen, new PointF(bTime, hLine), new PointF(bTime, ordinate));
                        graph.DrawLine(repairPen, new PointF(eTime, hLine), new PointF(eTime, ordinate));
                    }

                    var tmpColor = StandardPen.Color;
                    StandardPen.Color = Color.DarkGreen;

                    foreach (var variant in route.Times)
                    {
                        var min = ConvertTimeToPixelsWithScale(variant.Tmin);
                        var des = ConvertTimeToPixelsWithScale(variant.Tdes);
                        var max = ConvertTimeToPixelsWithScale(variant.Tmax);
                        graph.DrawLine(StandardPen, min, ordinate, des, hLine);
                        graph.DrawLine(StandardPen, max, ordinate, des, hLine);
                        graph.DrawLine(StandardPen, min, ordinate, min, hLine);
                        graph.DrawLine(StandardPen, min, hLine, min + 10, hLine);

                        graph.DrawLine(StandardPen, des, ordinate, des, hLine + 2);
                        graph.DrawLine(StandardPen, des, hLine + 2, des + 10, hLine + 2);
                        graph.DrawLine(StandardPen, des, ordinate, des + 10, hLine + 2);

                        graph.DrawLine(StandardPen, max, ordinate, max, hLine);
                        graph.DrawLine(StandardPen, max, hLine, max - 10, hLine);
                    }

                    StandardPen.Color = tmpColor;
                    foreach (var repair in route.Repairs.Where(r => !r.IsFake))
                    {
                        var informationString = String.Format("{0}, {1}",
                            (repair.type != null ? repair.type.Name : "NULL"),
                            (repair.inspectionPoint != null ? repair.inspectionPoint.name : "NULL"));
                        graph.DrawString(informationString, font, Brushes.DarkBlue, new PointF(ConvertTimeToPixelsWithScale((repair.beginTime + repair.endTime) / 2), hLine - 0.4f * row));

                        //graph.DrawLine(new Pen(Color.BlueViolet, 2), new PointF(ConvertTimeToPixelsWithScale(repair.beginTime ), hLine - 0.4f * row), new PointF(ConvertTimeToPixelsWithScale(repair.endTime), hLine - 0.4f*row));
#if LOGER_ON
                        //  Logger.Output(String.Format("Маршрут №{0:D3} --> {1}", route.number, informationString), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
                    }
                    ordinate += (rowHeight + merge) * MovementSchedule.scale;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), rNumber);
            }
        }

        private void bottomBox_Paint(object sender, PaintEventArgs args)
        {
            var graph = args.Graphics;
            var pb = sender as PictureBox;
            if (pb == null)
                return;

            var width = pb.Width - 1;
            var height = pb.Height - 1;
            var startX = 0;
            var startY = 0;
            graph.DrawLine(StandardPen, startX, startY, width, startY);
            graph.DrawLine(StandardPen, width, startY, width, height);
        }
    }
}
