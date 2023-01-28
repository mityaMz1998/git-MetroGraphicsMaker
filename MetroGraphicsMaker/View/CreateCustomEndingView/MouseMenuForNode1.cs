using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core;
using linii_graph;

namespace View.CreateCustomEndingView
{
    /// <summary>
    /// Context menu for <see cref="NightStayPoint"/> graphical editor.
    /// </summary>
    public class MouseMenuForNode1 : ContextMenu
    {
        /// <summary>
        /// Method which construct <see cref="MenuItem"/> by <see cref="NightStayPoint"/> object.
        /// </summary>
        /// <param name="point">Object from which will be taken information for <see cref="MenuItem.Header"/>.</param>
        /// <returns>New <see cref="MenuItem"/> object which sunscribed on event handler for click event.</returns>
        private MenuItem GetItemByPoint(NightStayPoint point)
        {
            var menuItem = new MenuItem
            {
                Header = "{point.code} -- {point.name}",
                Tag = point
            };
            menuItem.Click += PointersMenuItem_Click;
            return menuItem;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MouseMenuForNode1()
        {
            var menuItems = MouseMenuForStation.pointerCodesForSelectedStation
                .Select(GetItemByPoint)
                .ToList();

            var pointers = new MenuItem
            {
                Header = "Указатель",
                ItemsSource = menuItems
            };
            Items.Add(pointers);

            var mainPoint = new MenuItem { Header = "Главная точка" };
            Items.Add(mainPoint);
        }

        /// <summary>
        /// Method event handler which set <see cref="Node1.PointerName"/> for parent node.
        /// </summary>
        /// <param name="sender">Menu item which choose user.</param>
        /// <param name="e">Some event args (we don't use it).</param>
        private void PointersMenuItem_Click(Object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var node = PlacementTarget as Node1;
            if (node == null || item == null)
                return;
 
            node.PointerName = item.Header.ToString(); 
        }
    }
}
