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
        private int initialConfig = 1;
        
        public MainWindow()
        {
            InitializeComponent();
            grid = new VehicleGrid("../../../configurations.txt", initialConfig);
            configEntryBox.Text = initialConfig.ToString();
            SetGameGrid();
            //Panel.SetZIndex(solutionMoveButton, -1); // MAY BE USEFUL FOR VEHEICLES/BORDERS TO SIT ABOVE A GRID IMAGE
 
            // DRAGGABLE CONTROL EXPERIMENTATION
            //nextConfigButton.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(root_MouseLeftButtonDown);
            //nextConfigButton.MouseMove += new MouseEventHandler(root_MouseMove);
            //nextConfigButton.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(root_MouseLeftButtonUp); 
        }

        private void SetGameGrid()
        {
            vehicleIDs.Clear();
            borders.Clear();
            gameGrid.Children.Clear();

            // ONLY DO THESE IF NEW GRID HAS DIFFERENT DIMENSIONS THAN THE CURRENT GRID? ************************************
            gameGrid.RowDefinitions.Clear();
            gameGrid.ColumnDefinitions.Clear();

            // set gameGrid rows and columns according to the configuration
            for (int i = 0; i < grid.Rows; i++)
            {
                //RowDefinition rowDef = new RowDefinition();
                //rowDef.Height = new GridLength(1.0, GridUnitType.Star);
                //gameGrid.RowDefinitions.Add(rowDef);
                gameGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < grid.Columns; i++)
            {
                //ColumnDefinition colDef = new ColumnDefinition();
                //colDef.Width = new GridLength(100, GridUnitType.Star);
                //gameGrid.ColumnDefinitions.Add(colDef);
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // represent each Vehicle as a Border
            foreach (VehicleStruct vd in grid.GetVehicleStucts())
            {
                Border vehicleBorder = new Border();
                vehicleBorder.BorderThickness = new Thickness(8, 8, 8, 8);
                vehicleBorder.CornerRadius = new CornerRadius(15);
                
                if (vd.id.Equals("X"))
                    vehicleBorder.Background = Brushes.Red;
                else
                    vehicleBorder.Background = Brushes.Gray;

                if (vd.vertical)
                    vehicleBorder.SetValue(Grid.RowSpanProperty, vd.length);
                else
                    vehicleBorder.SetValue(Grid.ColumnSpanProperty, vd.length);

                gameGrid.Children.Add(vehicleBorder);
                vehicleBorder.Focusable = true;

                // OPTION 1 - SELECT BORDER WITH MouseLeftButtonDown EVENT
                vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_MouseLeftButtonDown);
                // THIS SHOULD BE EQUIVALENT TO STATEMENT ABOVE
                //vehicleBorder.AddHandler(Border.MouseLeftButtonDownEvent, new RoutedEventHandler(Border_MouseLeftButtonDown));              
                
                // OPTION 2 - SELECT BORDER BY GIVING IT KEYBOARD FOCUS
                //vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_PreviewMouseDown); // ALSO WORKS FOR PREVIEW EVENT
                //vehicleBorder.AddHandler(Border.GotKeyboardFocusEvent, new RoutedEventHandler(Border_GotFocus));

                // DRAGGABLE CONTROL EXPERIMENTATION
                //vehicleBorder.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(root_MouseLeftButtonDown);
                //vehicleBorder.MouseMove += new MouseEventHandler(root_MouseMove);
                //vehicleBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(root_MouseLeftButtonUp);   
                    
                
                vehicleBorder.KeyDown += new KeyEventHandler(border_KeyDown);

                Grid.SetRow(vehicleBorder, vd.row);
                Grid.SetColumn(vehicleBorder, vd.column);
                
                vehicleIDs.Add(vehicleBorder, vd.id);
                borders.Add(vd.id, vehicleBorder);
                solutionMoveButton.IsEnabled = true;
            }
        }

        // OPTION 1 (SEE SetGameGrid ABOVE)
        private void Border_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            // remove highlighting from selected
            if (selected != null)
                selected.BorderBrush = null;
            selected = (Border)sender;
            selected.BorderBrush = Brushes.Blue;
            selected.Focus();
        }

        // OPTION 2 (SEE SetGameGrid ABOVE)
        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(sender as Border);
        }

        // OPTION 2 (SEE SetGameGrid ABOVE)
        private void Border_GotFocus(object sender, RoutedEventArgs e)
        {
            // remove highlighting from selected
            if (selected != null)
                selected.BorderBrush = null;
            selected = (Border)sender;
            selected.BorderBrush = Brushes.Blue;
        }




        private void solutionMoveButton_Click(object sender, RoutedEventArgs e)
        {
            VehicleStruct? movedVehicle = grid.NextSolutionMove();
            //if (((Button)sender).Equals(solutionMoveButton))
            //    movedVehicle = grid.NextSolutionMove();
            //else
            //    movedVehicle = grid.UndoSolutionMove(); // GET RID OF? CAN'T THINK OF REASONABLE USE-CASE.

            // THIS IF SHOULDN'T BE NECESSARY->SOLUTION MOVES WILL ALWAYS WORK SO LONG AS THE BUTTON IS ENABLED
            if (movedVehicle.HasValue) // HANDLE NULL VehicleStruct (I.E. WHEN THE MOVE CAN'T BE MADE)
            {
                VehicleStruct mv = movedVehicle.Value;
                Border movedBorder = borders[mv.id];
                Grid.SetRow(movedBorder, mv.row);
                Grid.SetColumn(movedBorder, mv.column);

                // disable button if puzzle is solved
                if (grid.Solved)
                    solutionMoveButton.IsEnabled = false;
            }

            if (selected != null)
                selected.Focus();
        }

        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text); // THIS NEEDS TO BE VALIDATED, OR NON-NUMBERS SHOULD BE PROHIBITIED AT ENTRY
            grid.SetConfig(config);
            configEntryBox.Text = grid.CurrentConfig.ToString();
            SetGameGrid();            
        }

        private void configEntryBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int config = Int32.Parse(configEntryBox.Text); // THIS NEEDS TO BE VALIDATED, OR NON-NUMBERS SHOULD BE PROHIBITIED AT ENTRY
                grid.SetConfig(config);
                SetGameGrid();
            }
        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            grid.SetConfig(0);
            configEntryBox.Text = grid.CurrentConfig.ToString();
            SetGameGrid(); 
        }

        private void previousConfigButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text);
            if (config - 1 == 0)
                grid.SetConfig(grid.TotalConfigs);
            else
                grid.SetConfig(config - 1);
            configEntryBox.Text = grid.CurrentConfig.ToString();
            SetGameGrid();
        }

        private void nextConfigButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text);
            if (config + 1 > grid.TotalConfigs)
                grid.SetConfig(1);
            else
                grid.SetConfig(config + 1);
            configEntryBox.Text = grid.CurrentConfig.ToString();
            SetGameGrid();
        }


        private void border_KeyDown(object sender, KeyEventArgs e)
        {
            //if (selected == null)
            //    return;
            Border border = (Border)sender;
            bool vertical = Grid.GetRowSpan(border) > 1;

            // get ID of selected Vehicle
            string vID = vehicleIDs[border];

            if (e.Key == Key.Left && !vertical)
            {
                if (grid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetColumn(border) - 1;
                    Grid.SetColumn(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Right && !vertical)
            {
                if (grid.MoveVehicle(vID, 1))
                {
                    int destination = Grid.GetColumn(border) + 1;
                    Grid.SetColumn(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Up && vertical)
            {
                if (grid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetRow(border) - 1;
                    Grid.SetRow(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Down && vertical)
            {
                if (grid.MoveVehicle(vID, 1))
                {
                    int destination = Grid.GetRow(border) + 1;
                    Grid.SetRow(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            e.Handled = true;
        }




        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ DRAGGABLE CONTROL EXPERIMENTATION ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        Point _anchorPoint;
        Point _currentPoint;
        bool _isInDrag;

        private void root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            _anchorPoint = e.GetPosition(null);
            if (element != null) 
                element.CaptureMouse();
            _isInDrag = true;
            e.Handled = true;
        }

        private readonly TranslateTransform _transform = new TranslateTransform();
        private void root_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isInDrag)
            {
                var element = sender as FrameworkElement;
                _currentPoint = e.GetPosition(null);

                _transform.X += _currentPoint.X - _anchorPoint.X;
                _transform.Y += (_currentPoint.Y - _anchorPoint.Y);
                this.RenderTransform = _transform;
                _anchorPoint = _currentPoint;
            }
        }

        private void root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isInDrag)
            {
                var element = sender as FrameworkElement;
                if (element != null) element.ReleaseMouseCapture();
                _isInDrag = false;
                e.Handled = true;
            }
        }





    }









    //private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    //{
    //    if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
    //    {
    //        //configEntryBox.MoveFocus(gameGrid);
    //        e.Handled = true;
    //    }
    //}





    //private void mainWindow_KeyDown(object sender, KeyEventArgs e)
    //{
    //    if (selected == null)
    //        return;
    //    bool vertical = Grid.GetRowSpan(selected) > 1;

    //    // get ID of selected Vehicle
    //    string vID = vehicleIDs[selected];

    //    if (e.Key == Key.Left && !vertical)
    //    {
    //        e.Handled = true;
    //        if (grid.MoveVehicle(vID, -1))
    //        {
    //            int destination = Grid.GetColumn(selected) - 1;
    //            Grid.SetColumn(selected, destination);
    //        }
    //    }
    //    else if (e.Key == Key.Right && !vertical)
    //    {
    //        e.Handled = true;
    //        if (grid.MoveVehicle(vID, 1))
    //        {
    //            int destination = Grid.GetColumn(selected) + 1;
    //            Grid.SetColumn(selected, destination);
    //        }                    
    //    }
    //    else if (e.Key == Key.Up && vertical)
    //    {
    //        e.Handled = true;
    //        if (grid.MoveVehicle(vID, -1))
    //        {
    //            int destination = Grid.GetRow(selected) - 1;
    //            Grid.SetRow(selected, destination);
    //        }                    
    //    }
    //    else if (e.Key == Key.Down && vertical)
    //    {
    //        e.Handled = true;
    //        if (grid.MoveVehicle(vID, 1))
    //        {
    //            int destination = Grid.GetRow(selected) + 1;
    //            Grid.SetRow(selected, destination);
    //        }
    //    }            
    //}
}
