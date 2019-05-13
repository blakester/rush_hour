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
        
        public MainWindow()
        {
            InitializeComponent();

            // CAN USE THESE TO ADD DYNAMIC NUMBER OF ROWS/COLUMNS
            //uiGrid.RowDefinitions.Add(new RowDefinition());
            //uiGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Initialize cars in loop?
            //for (int i = 0; i < 3; i++)
            //{
                Border car = new Border();
                car.BorderThickness = new Thickness(10, 10, 10, 10);
                car.Background = Brushes.Red;
                car.SetValue(Grid.ColumnSpanProperty, 2);
                uiGrid.Children.Add(car);
                //car.MouseLeftButtonDown += new MouseButtonEventHandler(cell1_1_MouseLeftButtonDown);
                car.AddHandler(Border.MouseLeftButtonDownEvent, new RoutedEventHandler(cell1_1_MouseLeftButtonDown));
                Grid.SetRow(car, 0);
                Grid.SetColumn(car, 1);

                Border car2 = new Border();
                car2.BorderThickness = new Thickness(10, 10, 10, 10);
                car2.Background = Brushes.Red;
                car2.SetValue(Grid.RowSpanProperty, 3);
                uiGrid.Children.Add(car2);
                //car.MouseLeftButtonDown += new MouseButtonEventHandler(cell1_1_MouseLeftButtonDown);
                car2.AddHandler(Border.MouseLeftButtonDownEvent, new RoutedEventHandler(cell1_1_MouseLeftButtonDown));
                Grid.SetRow(car2, 0);
                Grid.SetColumn(car2, 4);
            //}

        }


        private void cell1_1_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            // deselect selected
            if (selected != null)
                selected.BorderBrush = null;
            selected = (Border)sender;
            selected.BorderBrush = Brushes.Blue;
            //Grid.SetColumn(lastSelected, Grid.GetColumn(lastSelected) + 1);
        }

        private void uiGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.Title = "Clicked";
        }

        private void uiGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right) // The Arrow-Down key
            {
                Grid.SetColumn(selected, Grid.GetColumn(selected) + 1);
            }
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (selected == null)
                return;
            bool vertical = Grid.GetRowSpan(selected) > 1;

            if (e.Key == Key.Left && !vertical)
            {
                int destination = Grid.GetColumn(selected) - 1; // BOUNDS CAN BE CHECKED BY THE MODEL TOO
                if (destination >= 0)
                    Grid.SetColumn(selected, destination);
            }
            else if (e.Key == Key.Right && !vertical)
            {
                int destination = Grid.GetColumn(selected) + 1;
                if (destination < 6)
                    Grid.SetColumn(selected, destination);
            }
            else if (e.Key == Key.Up && vertical)
            {
                int destination = Grid.GetRow(selected) - 1;
                if (destination >= 0)
                    Grid.SetRow(selected, destination);
            }
            else if (e.Key == Key.Down && vertical)
            {
                int destination = Grid.GetRow(selected) + 1;
                if (destination < 6)
                    Grid.SetRow(selected, destination);
            }
        }

    }
}
