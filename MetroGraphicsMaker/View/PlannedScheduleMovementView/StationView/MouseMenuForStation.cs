using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using Actions;
using Core;
using linii_graph;
using CreateCustomEnding = WpfApplication1.Forms.EditorWindows.GraphCore.CreateCustomEnding;
using CreateCustomEndingNew = WpfApplication1.Forms.EditorWindows.GraphCore.CreateCustomEndingNew;

namespace View
{
    //Описывет меню мыши для станций.
    class MouseMenuForStation: ContextMenu
    {
        private UInt32 selectedStationCode;
        private String selectedstationName;
        public static List<NightStayPoint> pointerCodesForSelectedStation = new List<NightStayPoint>();
        public static String stationName;
        public MouseMenuForStation(string _name,UInt32 _code)
        {
            selectedStationCode = _code;
            selectedstationName = _name;
            stationName = _name;
            var NewMenuItem = new MenuItem {Header = "Построение произвольного окончания" };
            NewMenuItem.Click += MouseClickLowEnd;
            Items.Add(NewMenuItem);

            var CreateTree = new MenuItem();
            CreateTree.Header = "Размещение указателей ночной расстановки";
            CreateTree.Click += MouseClick_CreateTree;
            Items.Add(CreateTree);
        }

        public static string getStationName()
        {
            string stName = stationName;
            return stName;
        }
        void MouseClickLowEnd(object sender, RoutedEventArgs e)
        {
            CreateCustomEnding TasksOutputOnTable = new CreateCustomEnding();
            //CreateCustomEndingNew TasksOutputOnTable = new CreateCustomEndingNew();
        }

        private void MouseClick_CreateTree(object sender, RoutedEventArgs e)
        {
           
            pointerCodesForSelectedStation= MovementSchedule.giveStationPointers(MovementSchedule.colStation[Convert.ToInt32(selectedStationCode-1)]);
            if (pointerCodesForSelectedStation.Count != 0)
            {
                CreateCustomEndingNew createCustomEndingNewWindow = new CreateCustomEndingNew();
            }
            else
            {
                MessageBox.Show("Указатели отсутствуют!", "No Pointers!", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            
        }
        /*
        private void MouseClickHingEnd(object sender, RoutedEventArgs e)
        {
            CreateLastTailEditor NewCreateLastTailEditor = new CreateLastTailEditor(ActivatedTrainPath, AbstractTailGiver.NamesTails.HingEnd, MasterTrainPath);
            if (NewCreateLastTailEditor.Check()) NewCreateLastTailEditor.Do();
        }*/
    }
}
