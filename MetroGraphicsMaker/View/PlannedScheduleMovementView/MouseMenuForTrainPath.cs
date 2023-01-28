using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using Actions;
using Core;
using linii_graph;

using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Converters;

using Exceptions.Actions;
using WpfApplication1.Forms.EditorWindows.CreateCustomEndingWindows;
using WpfApplication1.Forms.EditorWindows.GraphCore;

namespace View
{ 
    //Логика меню мыши для ниток.
    class MouseMenuForTrainPath : ContextMenu
    {
        List<Point> LowEnd = new List<Point> { new Point(0, 0), new Point(0, 10) };
        List<Point> HingEnd = new List<Point> { new Point(0, 0), new Point(0, 20) };

        Core.TrainPath ActivatedTrainPath;
        ListTrainPaths MasterTrainPath;

        LoaderCustomEndings StreamCustomEnding;
        MenuItem MenuItemsForCustomEndings;


        public MouseMenuForTrainPath(Core.TrainPath _ActivatedTrainPath, ListTrainPaths _MasterTrainPath)
        {
            ActivatedTrainPath = _ActivatedTrainPath;
            MasterTrainPath = _MasterTrainPath;

            List<MenuItem> NewMenuItemsForSubMenu = new List<MenuItem>();

            MenuItem NewMenuItem = new MenuItem();
            NewMenuItem.Header = "Малый конец";
            NewMenuItem.Click += MouseClickLowEnd;
            NewMenuItemsForSubMenu.Add(NewMenuItem);

            NewMenuItem = new MenuItem();
            NewMenuItem.Header = "Большой конец";
            NewMenuItem.Click += MouseClickHingEnd;
            NewMenuItemsForSubMenu.Add(NewMenuItem);

            MenuItem NewSubMenu = new MenuItem();
            NewSubMenu.Header = "Добавить стандартное окончание";
            NewSubMenu.ItemsSource = NewMenuItemsForSubMenu;
            Items.Add(NewSubMenu);

            MenuItemsForCustomEndings = new MenuItem();
            MenuItemsForCustomEndings.Header = "Добавить произвольное окончание";
            StreamCustomEnding = new LoaderCustomEndings(EnjectionCustomEnding);

            NewMenuItem = new MenuItem();
            NewMenuItem.Header = "Удалить хвост";
            if (ActivatedTrainPath.LogicalTail != null) 
            {
                NewSubMenu.IsEnabled = false; 
                MenuItemsForCustomEndings.IsEnabled = false; 
                NewMenuItem.IsEnabled = true;
                NewMenuItem.Click += MouseClickDeleteLastOrConnectionTail;   
            }
            else
            {
                NewMenuItem.IsEnabled = false; 
            }

            Items.Add(MenuItemsForCustomEndings);
            Items.Add(NewMenuItem);

            List<NightStayPoint> nightStayPointers;
            switch (_ActivatedTrainPath.direction.value)
            {
                case DirectionValue.EVEN:
                    nightStayPointers = MovementSchedule.giveStationPointers(MovementSchedule.colStation[_ActivatedTrainPath.NumLast]);
                    break;
                case DirectionValue.ODD:
                    nightStayPointers = MovementSchedule.giveStationPointers(MovementSchedule.colStation[MovementSchedule.colStation.Count-1-_ActivatedTrainPath.NumLast]);
                    break;
                default:
                    nightStayPointers = new List<NightStayPoint>();
                    MessageBox.Show("Unknown  RegimeOfMotion in ListTrainPaths class");
                    break;
            }

            NewMenuItemsForSubMenu = new List<MenuItem>();

            foreach (NightStayPoint thNightStayPoint in nightStayPointers)
            {
                NewMenuItem = new MenuItem();
                NewMenuItem.Header = thNightStayPoint.name;
                NewMenuItem.Tag = thNightStayPoint;
                NewMenuItem.Click += MouseClickNightEnd;
                NewMenuItemsForSubMenu.Add(NewMenuItem);
            }
                 
            NewSubMenu = new MenuItem();
            NewSubMenu.Header = "Привязать указатель";
            NewSubMenu.ItemsSource = NewMenuItemsForSubMenu;
            if (NewMenuItemsForSubMenu.Count == 0)
            {
                NewSubMenu.IsEnabled = false;
            }
            Items.Add(NewSubMenu);

            NewMenuItem = new MenuItem();
            NewMenuItem.Header = "Привязать указатели";
            NewMenuItem.IsEnabled = false;
            Items.Add(NewMenuItem);

            Items.Add("2222");
            Items.Add("3333");
            Items.Add("4444");
        }
    

        protected void EnjectionCustomEnding(Button CustomEnding)
        {
            MenuItemsForCustomEndings.Items.Add(CustomEnding);
            CustomEnding.Click += MouseClickCustomEnding;
            //CustomEnding.tag
        }
        
        public void MouseClickCustomEnding(object sender, RoutedEventArgs e)
        {
            Button ButtonCustomEnding = (Button)sender;
            CreationTrainPathLastTail NewCreateLastTailEditor = new CreationTrainPathLastTail(ActivatedTrainPath, AbstractTailGiver.NamesTails.SingleTailCustom, ButtonCustomEnding.Tag.ToString(), MasterTrainPath);
            if (NewCreateLastTailEditor.Check()) NewCreateLastTailEditor.Do();
        }
        
        private void MouseClickLowEnd(object sender, RoutedEventArgs e)
        {
            CreationTrainPathLastTail NewCreateLastTailEditor = new CreationTrainPathLastTail(ActivatedTrainPath, AbstractTailGiver.NamesTails.SingleTail, "LowEnd", MasterTrainPath);
            if (NewCreateLastTailEditor.Check()) NewCreateLastTailEditor.Do();
        }

        private void MouseClickHingEnd(object sender, RoutedEventArgs e)
        {
            CreationTrainPathLastTail NewCreateLastTailEditor = new CreationTrainPathLastTail(ActivatedTrainPath, AbstractTailGiver.NamesTails.SingleTail, "HingEnd", MasterTrainPath);
            if (NewCreateLastTailEditor.Check()) NewCreateLastTailEditor.Do();
        }

        private void MouseClickNightEnd(object sender, RoutedEventArgs e)
        {
            CreationTrainPathLastTail NewCreateLastTailEditor = new CreationTrainPathLastTail(ActivatedTrainPath, AbstractTailGiver.NamesTails.NightTail, "NightEnd", MasterTrainPath);
            if (NewCreateLastTailEditor.Check())
            {
                NewCreateLastTailEditor.Do();
                
                NightTail Night = (NightTail)NewCreateLastTailEditor.CTrainPath.LogicalTail;

                Night.StayPoint=(NightStayPoint)((MenuItem)sender).Tag;
                Night.isExistText = true;
            }
        }

        private void MouseClickDeleteLastOrConnectionTail(object sender, RoutedEventArgs e)
        {
            DeletionTrainPathLastTail NewDeleteLastTailEditor;
            DisconnectionTrainPaths NewDisconnectionTrainPathsEditor;
            switch (ActivatedTrainPath.LogicalTail.NameTail)
            {
                case AbstractTailGiver.NamesTails.LinkedTail:
                    try
                    {
                        NewDisconnectionTrainPathsEditor = new DisconnectionTrainPaths(ActivatedTrainPath, ActivatedTrainPath.NextThread, ActivatedTrainPath.LogicalTail.NameTail, MasterTrainPath, true);
                        NewDisconnectionTrainPathsEditor.Do();
                    }
                    catch (TheOperationIsNotFeasible ane)
                    {
                        MessageBox.Show(String.Format("State = {0}", ane.State), "Привет, Вася!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case AbstractTailGiver.NamesTails.LinkedTailCustom:
                    try
                    {
                        NewDisconnectionTrainPathsEditor = new DisconnectionTrainPaths(ActivatedTrainPath, ActivatedTrainPath.NextThread, ActivatedTrainPath.LogicalTail.NameTail, MasterTrainPath, true);
                        NewDisconnectionTrainPathsEditor.Do();
                    }
                    catch (TheOperationIsNotFeasible ane)
                    {
                        MessageBox.Show(String.Format("State = {0}", ane.State), "Привет, Вася!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case AbstractTailGiver.NamesTails.SingleTail:
                    NewDeleteLastTailEditor = new DeletionTrainPathLastTail(ActivatedTrainPath.ViewTrainPath.TailTrainPath, true);
                    if (NewDeleteLastTailEditor.Check()) NewDeleteLastTailEditor.Do();
                    break;
                case AbstractTailGiver.NamesTails.SingleTailCustom:
                    NewDeleteLastTailEditor = new DeletionTrainPathLastTail(ActivatedTrainPath.ViewTrainPath.TailTrainPath, true);
                    if (NewDeleteLastTailEditor.Check()) NewDeleteLastTailEditor.Do();
                    break;
            }

        }
    }
}
