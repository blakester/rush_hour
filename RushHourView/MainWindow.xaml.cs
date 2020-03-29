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

            // DRAGGABLE VEHICLES EXPERIMENTATION
            // place a Border on each cell of the grid
            for (int i = 0; i < grid.Rows; i++)
            {
                for (int j = 0; j < grid.Columns; j++)
                {
                    Border cellBorder = new Border();
                    cellBorder.BorderThickness = new Thickness(4);
                    cellBorder.BorderBrush = Brushes.Black;
                    cellBorder.Background = Brushes.Aqua;
                    gameGrid.Children.Add(cellBorder);
                    Grid.SetRow(cellBorder, i);
                    Grid.SetColumn(cellBorder, j);

                    //cellBorder.AllowDrop = true;
                    //cellBorder.DragEnter += cellBorder_DragEnter;
                    //cellBorder.PreviewDragOver += cellBorder_PreviewDragOver;
                }
            }
            // END DRAGGABLE VEHICLES EXPERIMENTATION
            
            // represent each Vehicle as a Border
            foreach (VehicleStruct vd in grid.GetVehicleStucts())
            {
                Border vehicleBorder = new Border();
                vehicleBorder.BorderThickness = new Thickness(8);
                vehicleBorder.CornerRadius = new CornerRadius(15);
                
                // the target "X" vehicle is red, all others are gray
                if (vd.id.Equals("X"))
                    vehicleBorder.Background = Brushes.Red;
                else
                    vehicleBorder.Background = Brushes.Gray;

                // set the orientation of the Border according to its Vehicle
                if (vd.vertical)
                    vehicleBorder.SetValue(Grid.RowSpanProperty, vd.length);
                else
                    vehicleBorder.SetValue(Grid.ColumnSpanProperty, vd.length);

                // add the Border to the grid
                gameGrid.Children.Add(vehicleBorder);

                // set up event handlers for the Border
                vehicleBorder.Focusable = true;
                

                // OPTION 1 - SELECT BORDER WITH MouseLeftButtonDown EVENT
                //vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_MouseLeftButtonDown);
                // THIS SHOULD BE EQUIVALENT TO STATEMENT ABOVE
                //vehicleBorder.AddHandler(Border.MouseLeftButtonDownEvent, new RoutedEventHandler(Border_MouseLeftButtonDown));              
                
                // OPTION 2 - SELECT BORDER BY GIVING IT KEYBOARD FOCUS
                //vehicleBorder.MouseLeftButtonDown += new MouseButtonEventHandler(Border_PreviewMouseDown); // ALSO WORKS FOR PREVIEW EVENT
                //vehicleBorder.AddHandler(Border.GotKeyboardFocusEvent, new RoutedEventHandler(Border_GotFocus));

                // DRAGGABLE BORDER EXPERIMENTATION
                vehicleBorder.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(root_MouseLeftButtonDown);
                vehicleBorder.MouseMove += new MouseEventHandler(root_MouseMove);
                vehicleBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(root_MouseLeftButtonUp);   
                 
                
                vehicleBorder.KeyDown += new KeyEventHandler(border_KeyDown);

                // position the Border on the grid
                Grid.SetRow(vehicleBorder, vd.row);
                Grid.SetColumn(vehicleBorder, vd.column);
                //Panel.SetZIndex(vehicleBorder, 0);
                
                // add the Border and Vehicle to lookup tables
                vehicleIDs.Add(vehicleBorder, vd.id);
                borders.Add(vd.id, vehicleBorder);
                solutionMoveButton.IsEnabled = true;
            }
        }

        //void cellBorder_PreviewDragOver(object sender, DragEventArgs e)
        //{
        //    Border cellBorder = sender as Border;
        //    cellBorder.BorderBrush = Brushes.Yellow;
        //}

        //void cellBorder_DragEnter(object sender, DragEventArgs e)
        //{
        //    Border cellBorder = sender as Border;
        //    cellBorder.BorderBrush = Brushes.Yellow;
        //}

        // OPTION 1 (SEE SetGameGrid ABOVE)
        //private void Border_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        //{
        //    // remove highlighting from selected
        //    if (selected != null)
        //        selected.BorderBrush = null;
        //    selected = (Border)sender;
        //    selected.BorderBrush = Brushes.Blue;
        //    selected.Focus();
        //}

        //// OPTION 2 (SEE SetGameGrid ABOVE)
        //private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    Keyboard.Focus(sender as Border);
        //}

        // OPTION 2 (SEE SetGameGrid ABOVE)
        //private void Border_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    // remove highlighting from selected
        //    if (selected != null)
        //        selected.BorderBrush = null;
        //    selected = (Border)sender;
        //    selected.BorderBrush = Brushes.Blue;
        //}




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

        // TODO: DRAGGING BORDERS IS SOMEWHAT WORKING (THOUGH THERE VERY WELL BE SOME MORE BUGS THAT NEED TO BE WORKED OUT). BUT THE NEXT
        // MAJOR CHALLENGE IS TO DYNAMICALLY UPDATE A BORDER'S POSITION IN THE UNDERLYING VEHICLE GRID AS IT IS DRAGGED. I COULD SIMPLY
        // TRY TO CHECK IT ON THE MOUSEUP EVENT WHEN THE DRAG IS FINISHED, BUT THIS WOULD ALLOW USERS TO DRAG BORDERS OFF THE GRID OR INTO
        // OTHER VEHICLES BEFORE LETTING UP THE MOUSE BUTTON.

        // I'M THINKING TO DETERMINE, UPON MOUSELEFTBUTTONDOWN, HOW FAR IN EITHER DIRECTION THE BORDER CAN TRAVEL BASED ON THE WIDTH/HEIGHT OF
        // A CELL AND ADDING/SUBTRACTING TO SAID DIRECTIONS IN MOUSEMOVE. THEN IN MOUSEUP, DIVIDE THE DISTANCE MOVED BY THE CELL WIDTH/HEIGHT
        // AND IF THE REMAINDER OF THE RESULT IS GREATER THAN HALF THE CELL'S WIDTH/HEIGHT, SNAP THE BORDER/VEHICLE TO THE CELL OF THE CEILING
        // OF SAID RESULT, OTHERWISE SNAP IT TO THE CELL OF THE FLOOR OF SAID RESULT. SOMETHING LIKE THIS.

        Point _anchorPoint;
        Point _currentPoint;
        bool _isInDrag;
        //private readonly TranslateTransform _transform = new TranslateTransform(); // ORIGINAL
        private TranslateTransform _transform;// = new TranslateTransform();

        private void root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // COPIED FROM Border_MouseLeftButtonDown
            // remove highlighting from selected
            if (selected != null)
                selected.BorderBrush = null;
            selected = (Border)sender;
            selected.BorderBrush = Brushes.Blue;
            selected.Focus();
            // END COPY FROM Border_MouseLeftButtonDown          

            
            
            //_transform = new TranslateTransform(); // ADDED, NOT IN HERE ORIGINALLY

            // TESTING
            //Point relativeLocation = selected.TranslatePoint(new Point(0, 0), selected.Parent as UIElement);
            // END TESTING
            
            // ORIGINAL
            var element = sender as FrameworkElement;
            //_anchorPoint = e.GetPosition(null); // ORIGINAL
            //_anchorPoint = e.GetPosition(gameGrid);
            _anchorPoint = Mouse.GetPosition(gameGrid);
            anchorPositionActual.Content = string.Format("({0}, {1})", (int)_anchorPoint.X, (int)_anchorPoint.Y);
            //_anchorPoint.X = e.GetPosition(gameGrid).X - _transform.X;
            //_anchorPoint.Y = e.GetPosition(gameGrid).Y - _transform.Y;
            
            if (element != null)
                element.CaptureMouse();


            _isInDrag = true;
            e.Handled = true;
        }

        
        private void root_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isInDrag)
            {
                //TranslateTransform _transform = new TranslateTransform(); // ORIGINAL
                _transform = new TranslateTransform();
                
                //var element = sender as FrameworkElement;
                Border vehicleBorder = sender as Border;
                //_currentPoint = e.GetPosition(null);// ORIGINAL
                //_currentPoint = Mouse.GetPosition(null);
                _currentPoint = Mouse.GetPosition(gameGrid);
                currentPositionActual.Content = string.Format("({0}, {1})", (int)_currentPoint.X, (int)_currentPoint.Y);

                string vehicleID = vehicleIDs[vehicleBorder];
                VehicleStruct v = grid.GetVehicleStuct(vehicleID);
                //double cellSize;
                
                if (v.vertical)
                {
                    double deltaY = _currentPoint.Y - _anchorPoint.Y;
                    //cellSize = gameGrid.RowDefinitions[0].ActualHeight;//ALWAYS RETURNS 133 REGARDLESS OF CELL SIZE. WHY?

                    if (deltaY > 0) // moving down
                    {
                        // only move if cell below is open
                        if (grid.IsCellOpen(v.row + v.length, v.column))
                        {
                            _transform.Y += deltaY;
                            //Grid.SetColumn(vehicleBorder, v.row + 1);
                        }

                    }
                    else // moving up
                    {
                        // only move if cell above is open
                        if (grid.IsCellOpen(v.row - 1, v.column))
                            _transform.Y += deltaY;
                    }                                        
                }
                else
                {
                    double deltaX = _currentPoint.X - _anchorPoint.X;
                    
                    if (deltaX < 0) // moving left
                    {
                        // only move if cell to left is open
                        if (grid.IsCellOpen(v.row, v.column - 1))
                            _transform.X += deltaX;
                    }
                    else // moving right
                    {
                        // only move if cell to right is open
                        if (grid.IsCellOpen(v.row, v.column + v.length))
                            _transform.X += deltaX;
                    }     
                }
                //this.RenderTransform = _transform; // ORIGINAL
                vehicleBorder.RenderTransform = _transform;

                //DragDrop.DoDragDrop(element, element, DragDropEffects.Move); // ADDED THIS LINE
                
            }
        }

        private void root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isInDrag)
            {
                var element = sender as FrameworkElement;
                
                if (element != null) 
                    element.ReleaseMouseCapture();

                _isInDrag = false;
                e.Handled = true;

                //DragDrop.DoDragDrop(element, element, DragDropEffects.Move); // ADDED THIS LINE
                //Border vehicleBorder = sender as Border; // ADDED THIS LINE
                //vehicleBorder.RenderTransform = _transform; // ADDED THIS LINE
            }
        }


        // DELETE ME WHEN FINISHED
        private void gameGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(gameGrid);
            int x = (int)mousePosition.X;
            int y = (int)mousePosition.Y;
            positionActual.Content = string.Format("({0}, {1})", x, y);
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
