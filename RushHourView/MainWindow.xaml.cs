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
        private Dictionary<Border, string> vehicleIDs = new Dictionary<Border, string>(32);
        private Dictionary<string, Border> borders = new Dictionary<string, Border>(32);
        private VehicleGrid grid;
        
        public MainWindow()
        {
            InitializeComponent();
            grid = new VehicleGrid("../../../configurations.txt", 1);
            SetGameGrid();
        }

        private void SetGameGrid()
        {
            vehicleIDs.Clear();
            borders.Clear();
            gameGrid.Children.Clear();
            gameGrid.RowDefinitions.Clear();
            gameGrid.ColumnDefinitions.Clear();

            // set gameGrid rows and columns according to the configuration
            for (int i = 0; i < grid.Rows; i++)
                gameGrid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < grid.Columns; i++)
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());

            foreach (KeyValuePair<string, Vehicle> kv in grid.vehicles)
            {
                string vID = kv.Key;
                Vehicle v = kv.Value;
                Border vehicleBorder = new Border();
                vehicleBorder.BorderThickness = new Thickness(10, 10, 10, 10);

                if (vID.Equals("X"))
                    vehicleBorder.Background = Brushes.Red;
                else
                    vehicleBorder.Background = Brushes.Gray;

                if (v.Vertical)
                    vehicleBorder.SetValue(Grid.RowSpanProperty, v.Length);
                else
                    vehicleBorder.SetValue(Grid.ColumnSpanProperty, v.Length);

                gameGrid.Children.Add(vehicleBorder);
                vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_MouseLeftButtonDown);
                //vehicleBorder.AddHandler(Border.MouseLeftButtonDownEvent, new RoutedEventHandler(Border_MouseLeftButtonDown));

                //vehicleBorder.Focusable = true;
                //vehicleBorder.Focus();
                //vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_MouseLeftButtonDown);
                //vehicleBorder.AddHandler(Border.GotKeyboardFocusEvent, new RoutedEventHandler(Border_GotFocus));

                Grid.SetRow(vehicleBorder, v.BackRow);
                Grid.SetColumn(vehicleBorder, v.BackCol);

                vehicleIDs.Add(vehicleBorder, vID);
                borders.Add(vID, vehicleBorder);
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
            string vID = vehicleIDs[selected];

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

        private void solutionMoveButton_Click(object sender, RoutedEventArgs e)
        {
            string movedVehicleID = grid.SolutionNextMove();
            if (movedVehicleID == null)
                return;

            Vehicle movedVehicle = grid.vehicles[movedVehicleID];
            Border movedBorder = borders[movedVehicleID];
            Grid.SetRow(movedBorder, movedVehicle.BackRow);
            Grid.SetColumn(movedBorder, movedVehicle.BackCol);
        }

        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text);
            grid.SetConfig(config);
            SetGameGrid();            
        }
    }
}
