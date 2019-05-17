using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RushHourModel;

namespace RushHour
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Border selected = null;
        private Dictionary<Border, int> vehicleIDs; // IS A DICTIONARY NECESSARY?
        //private Dictionary<int, Border> vehicleIDs;
        private VehicleGrid grid;
        
        public MainWindow()
        {
            InitializeComponent();
            grid = new VehicleGrid("../../../configurations.txt", 2);
            vehicleIDs = new Dictionary<Border, int>(grid.vehicles.Count);

            // set uiGrid rows and columns according to the configuration
            for (int i = 0; i < grid.Rows; i++)
                uiGrid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < grid.Columns; i++)
                uiGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // THESE CAN BE USED TO CLEAR THE ROWS AND COLUMNS
            //uiGrid.RowDefinitions.Clear();
            //uiGrid.RowDefinitions.Clear();

            //foreach (KeyValuePair<int, Vehicle> kv in grid.vehicles)
            //{
            //    Border vehicleBorder = new Border();
            //    vehicleBorder.BorderThickness = new Thickness(10, 10, 10, 10);

            //    if (kv.Key == 1)
            //        vehicleBorder.Background = Brushes.Red;
            //    else
            //        vehicleBorder.Background = Brushes.Gray;

            //    if (kv.Value.Vertical)
            //        vehicleBorder.SetValue(Grid.RowSpanProperty, kv.Value.Length);
            //    else
            //        vehicleBorder.SetValue(Grid.ColumnSpanProperty, kv.Value.Length);

            //    uiGrid.Children.Add(vehicleBorder);
            //    vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_MouseLeftButtonDown);
                
            //    Grid.SetRow(vehicleBorder, kv.Value.BackRow);
            //    Grid.SetColumn(vehicleBorder, kv.Value.BackCol);

            //    vehicleIDs.Add(vehicleBorder, kv.Key);
            //}

            int vehicleID = 1;
            foreach (Vehicle v in grid.vehicles)
            {
                Border vehicleBorder = new Border();
                vehicleBorder.BorderThickness = new Thickness(10, 10, 10, 10);

                if (vehicleID == 1)
                    vehicleBorder.Background = Brushes.Red;
                else
                    vehicleBorder.Background = Brushes.Gray;

                if (v.Vertical)
                    vehicleBorder.SetValue(Grid.RowSpanProperty, v.Length);
                else
                    vehicleBorder.SetValue(Grid.ColumnSpanProperty, v.Length);

                uiGrid.Children.Add(vehicleBorder);
                vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_MouseLeftButtonDown);
                //vehicleBorder.AddHandler(Border.MouseLeftButtonDownEvent, new RoutedEventHandler(Border_MouseLeftButtonDown));

                //vehicleBorder.Focusable = true;
                //vehicleBorder.Focus();
                //vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_MouseLeftButtonDown);
                //vehicleBorder.AddHandler(Border.GotKeyboardFocusEvent, new RoutedEventHandler(Border_GotFocus));

                Grid.SetRow(vehicleBorder, v.BackRow);
                Grid.SetColumn(vehicleBorder, v.BackCol);

                vehicleIDs.Add(vehicleBorder, vehicleID++);
            }
        }


        private void Border_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            // deselect selected
            if (selected != null)
                selected.BorderBrush = null;
            selected = (Border)sender;
            selected.BorderBrush = Brushes.Blue;
        }

        //private void Border_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    // deselect selected
        //    if (selected != null)
        //        selected.BorderBrush = null;
        //    selected = (Border)sender;
        //    selected.BorderBrush = Brushes.Blue;
        //}


        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (selected == null)
                return;
            bool vertical = Grid.GetRowSpan(selected) > 1;

            // get ID of selected Vehicle
            int vID = vehicleIDs[selected];

            if (e.Key == Key.Left && !vertical)
            {
                if (grid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetColumn(selected) - 1;
                    Grid.SetColumn(selected, destination);
                }
            }
            else if (e.Key == Key.Right && !vertical)
            {
                if (grid.MoveVehicle(vID, 1))
                {
                    int destination = Grid.GetColumn(selected) + 1;
                    Grid.SetColumn(selected, destination);
                }                    
            }
            else if (e.Key == Key.Up && vertical)
            {
                if (grid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetRow(selected) - 1;
                    Grid.SetRow(selected, destination);
                }                    
            }
            else if (e.Key == Key.Down && vertical)
            {
                if (grid.MoveVehicle(vID, 1))
                {
                    int destination = Grid.GetRow(selected) + 1;
                    Grid.SetRow(selected, destination);
                }
            }
        }

    }
}
