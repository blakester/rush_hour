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
        private Border _selected = null;
        private Dictionary<Border, string> _bordersToVIDs = new Dictionary<Border, string>(32);
        private Dictionary<string, Border> _vIDsToBorders = new Dictionary<string, Border>(32);
        private Border[,] _cellBorders;
        private VehicleGrid _grid;
        private int _initialConfig = 1;

        
        public MainWindow()
        {
            InitializeComponent();
            _grid = new VehicleGrid("../../../configurations.txt", _initialConfig);
            configEntryBox.Text = _initialConfig.ToString();
            SetGameGrid();
            //Panel.SetZIndex(solutionMoveButton, -1); // MAY BE USEFUL FOR VEHEICLES/BORDERS TO SIT ABOVE A GRID IMAGE
 
            // DRAGGABLE CONTROL EXPERIMENTATION
            //nextConfigButton.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(root_MouseLeftButtonDown);
            //nextConfigButton.MouseMove += new MouseEventHandler(root_MouseMove);
            //nextConfigButton.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(root_MouseLeftButtonUp); 
        }

        private void SetGameGrid()
        {
            _bordersToVIDs.Clear();
            _vIDsToBorders.Clear();
            gameGrid.Children.Clear();

            // ONLY DO THESE IF NEW GRID HAS DIFFERENT DIMENSIONS THAN THE CURRENT GRID? ************************************
            gameGrid.RowDefinitions.Clear();
            gameGrid.ColumnDefinitions.Clear();
            _cellBorders = new Border[_grid.Rows + 1, _grid.Columns + 1]; // plus one for hidden cells
            
            // set gameGrid rows and columns according to the configuration
            for (int i = 0; i <= _grid.Rows; i++)
            {
                //RowDefinition rowDef = new RowDefinition();
                //rowDef.Height = new GridLength(1.0, GridUnitType.Star);
                //gameGrid.RowDefinitions.Add(rowDef);
                gameGrid.RowDefinitions.Add(new RowDefinition());
            }            
            for (int i = 0; i <= _grid.Columns; i++)
            {
                //ColumnDefinition colDef = new ColumnDefinition();
                //colDef.Width = new GridLength(100, GridUnitType.Star);
                //gameGrid.ColumnDefinitions.Add(colDef);
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // hide the hidden cells
            gameGrid.RowDefinitions[_grid.Rows].Height = new GridLength(0);
            gameGrid.ColumnDefinitions[_grid.Columns].Width = new GridLength(0);

            // EXPERIMENTING
            //Border cellBorder1 = new Border();
            //cellBorder1.BorderThickness = new Thickness(10);
            //cellBorder1.BorderBrush = Brushes.Orange;
            //cellBorder1.Background = Brushes.Aqua;
            //gameGrid.Children.Add(cellBorder1);
            //Grid.SetRow(cellBorder1, 0);
            //Grid.SetColumn(cellBorder1, 0);

            //Border cellBorder2 = new Border();
            //cellBorder2.BorderThickness = new Thickness(10);
            //cellBorder2.BorderBrush = Brushes.Orange;
            //cellBorder2.Background = Brushes.Aqua;
            //gameGrid.Children.Add(cellBorder2);
            //Grid.SetRow(cellBorder2, 0);
            //Grid.SetColumn(cellBorder2, 2);

            //Point testing = gameGrid.TranslatePoint(new Point(0.0, 0.0), cellBorder2);
            //Point testing2 = setConfigGrid.TranslatePoint(new Point(0.0, 0.0), randomButton);
            //UIElement container = VisualTreeHelper.GetParent(positionLabel) as UIElement;
            // END EXPERIMENTING

            // DRAGGABLE VEHICLES EXPERIMENTATION
            // place a Border on each cell of the _grid
            for (int i = 0; i <= _grid.Rows; i++) // TODO: CHANGE BACK TO i = 0
            {
                for (int j = 0; j <= _grid.Columns; j++)
                {                     
                    // add the cell to the grid and cell borders array
                    Border cellBorder = new Border();
                    gameGrid.Children.Add(cellBorder);
                    Grid.SetRow(cellBorder, i);
                    Grid.SetColumn(cellBorder, j);
                    _cellBorders[i, j] = cellBorder;

                    // hide the hidden cells (though they should already be hidden)
                    if (i == _grid.Rows || j == _grid.Columns)
                    {
                        cellBorder.Visibility = Visibility.Hidden;
                        continue;
                    }

                    // stylize the cells
                    cellBorder.BorderThickness = new Thickness(4);
                    cellBorder.BorderBrush = Brushes.Black;
                    cellBorder.Background = Brushes.Aqua;

                    //cellBorder.AllowDrop = true;
                    //cellBorder.DragEnter += cellBorder_DragEnter;
                    //cellBorder.PreviewDragOver += cellBorder_PreviewDragOver;
                }
            }
            // END DRAGGABLE VEHICLES EXPERIMENTATION
            
            // represent each Vehicle as a Border
            foreach (VehicleStruct vd in _grid.GetVehicleStucts())
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

                // add the Border to the _grid
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
                vehicleBorder.MouseEnter += (s, e) => Mouse.OverrideCursor = Cursors.Hand;
                vehicleBorder.MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow;
                 
                vehicleBorder.KeyDown += new KeyEventHandler(border_KeyDown);
                // EXPERIMENTATION
                //vehicleBorder.Loaded += vehicleBorder_Loaded;
                // END EXPERIMENTATION

                // position the Border on the _grid
                Grid.SetRow(vehicleBorder, vd.row);
                Grid.SetColumn(vehicleBorder, vd.column);
                //Panel.SetZIndex(vehicleBorder, 0);
                
                // add the Border and Vehicle to lookup tables
                _bordersToVIDs.Add(vehicleBorder, vd.id);
                _vIDsToBorders.Add(vd.id, vehicleBorder);
                solutionMoveButton.IsEnabled = true;
            }            
        }

        // EXPERIMENTATION
        //void vehicleBorder_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Point testing2 = gameGrid.TranslatePoint(new Point(0.0, 0.0), sender as UIElement);
        //}
        // END EXPERIMENTATION

        // EXPERIMENTATION
        //private Point GetPosition(Visual element)
        //{
        //    var positionTransform = element.TransformToAncestor(setConfigGrid);
        //    var areaPosition = positionTransform.Transform(new Point(0, 0));

        //    return areaPosition;
        //}
        // END EXPERIMENTATION

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
        //    // remove highlighting from _selected
        //    if (_selected != null)
        //        _selected.BorderBrush = null;
        //    _selected = (Border)sender;
        //    _selected.BorderBrush = Brushes.Blue;
        //    _selected.Focus();
        //}

        //// OPTION 2 (SEE SetGameGrid ABOVE)
        //private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    Keyboard.Focus(sender as Border);
        //}

        // OPTION 2 (SEE SetGameGrid ABOVE)
        //private void Border_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    // remove highlighting from _selected
        //    if (_selected != null)
        //        _selected.BorderBrush = null;
        //    _selected = (Border)sender;
        //    _selected.BorderBrush = Brushes.Blue;
        //}






        private void solutionMoveButton_Click(object sender, RoutedEventArgs e)
        {
            VehicleStruct? movedVehicle = _grid.NextSolutionMove();
            //if (((Button)sender).Equals(solutionMoveButton))
            //    movedVehicle = _grid.NextSolutionMove();
            //else
            //    movedVehicle = _grid.UndoSolutionMove(); // GET RID OF? CAN'T THINK OF REASONABLE USE-CASE.

            // THIS IF SHOULDN'T BE NECESSARY->SOLUTION MOVES WILL ALWAYS WORK SO LONG AS THE BUTTON IS ENABLED
            if (movedVehicle.HasValue) // HANDLE NULL VehicleStruct (I.E. WHEN THE MOVE CAN'T BE MADE)
            {
                VehicleStruct mv = movedVehicle.Value;
                Border movedBorder = _vIDsToBorders[mv.id];
                Grid.SetRow(movedBorder, mv.row);
                Grid.SetColumn(movedBorder, mv.column);

                // disable button if puzzle is solved
                if (_grid.Solved)
                    solutionMoveButton.IsEnabled = false;
            }

            if (_selected != null)
                _selected.Focus();
        }

        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text); // THIS NEEDS TO BE VALIDATED, OR NON-NUMBERS SHOULD BE PROHIBITIED AT ENTRY
            _grid.SetConfig(config);
            configEntryBox.Text = _grid.CurrentConfig.ToString();
            SetGameGrid();            
        }

        private void configEntryBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int config = Int32.Parse(configEntryBox.Text); // THIS NEEDS TO BE VALIDATED, OR NON-NUMBERS SHOULD BE PROHIBITIED AT ENTRY
                _grid.SetConfig(config);
                SetGameGrid();
            }
        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            _grid.SetConfig(0);
            configEntryBox.Text = _grid.CurrentConfig.ToString();
            SetGameGrid(); 
        }

        private void previousConfigButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text);
            if (config - 1 == 0)
                _grid.SetConfig(_grid.TotalConfigs);
            else
                _grid.SetConfig(config - 1);
            configEntryBox.Text = _grid.CurrentConfig.ToString();
            SetGameGrid();
        }

        private void nextConfigButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text);
            if (config + 1 > _grid.TotalConfigs)
                _grid.SetConfig(1);
            else
                _grid.SetConfig(config + 1);
            configEntryBox.Text = _grid.CurrentConfig.ToString();
            SetGameGrid();
        }


        private void border_KeyDown(object sender, KeyEventArgs e)
        {
            //if (_selected == null)
            //    return;
            Border border = (Border)sender;
            bool vertical = Grid.GetRowSpan(border) > 1;

            // get ID of _selected Vehicle
            string vID = _bordersToVIDs[border];

            if (e.Key == Key.Left && !vertical)
            {
                if (_grid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetColumn(border) - 1;
                    Grid.SetColumn(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Right && !vertical)
            {
                if (_grid.MoveVehicle(vID, 1))
                {
                    int destination = Grid.GetColumn(border) + 1;
                    Grid.SetColumn(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Up && vertical)
            {
                if (_grid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetRow(border) - 1;
                    Grid.SetRow(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Down && vertical)
            {
                if (_grid.MoveVehicle(vID, 1))
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

        private Point _anchorPoint;
        private Point _currentPoint;
        private int openCellsBehind;
        private int openCellsAhead;
        private Border _furthestOpenCellBehind;
        private Border _nearestOccupiedCellAhead;
        private bool _isInDrag;
        //private double _spaceAhead;
        //private double _spaceBehind;
        //private readonly TranslateTransform _transform = new TranslateTransform(); // ORIGINAL
        //private TranslateTransform _transform = new TranslateTransform(); // BORDERS DISAPPEAR ON DRAG
        private TranslateTransform _transform; // BORDER SNAPS TO ORIGINAL POSITION ON SECOND DRAG

        private void root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // COPIED FROM Border_MouseLeftButtonDown
            // remove highlighting from _selected
            if (_selected != null)
                _selected.BorderBrush = null;
            _selected = (Border)sender;
            _selected.BorderBrush = Brushes.Blue;
            _selected.Focus(); // TODO: IS THIS EVEN NEEDED?
            // END COPY FROM Border_MouseLeftButtonDown
            
            _transform = new TranslateTransform(); // ADDED, NOT IN HERE ORIGINALLY, CAUSES BORDERS TO SHOOT OFF SCREEN VS BEING INSTANTIATED IN MOUSEMOVE WHERE IT WORKS AS DESIRED
            
            
            // ORIGINAL
            var element = sender as FrameworkElement;
            //_anchorPoint = e.GetPosition(null); // ORIGINAL
            //_anchorPoint = e.GetPosition(gameGrid);
            _anchorPoint = Mouse.GetPosition(gameGrid); // BORDER DRAGS WITH POINTER AS DESIRED (SAME BEHAVIOR AS ABOVE TWO?)
            //_anchorPoint.X = e.GetPosition(gameGrid).X - _transform.X;
            //_anchorPoint.Y = e.GetPosition(gameGrid).Y - _transform.Y;
            //_anchorPoint.X = e.GetPosition(gameGrid).X;// -_transform.X; 
            //_anchorPoint.Y = e.GetPosition(gameGrid).Y;// -_transform.Y; 
            anchorPositionActual.Content = string.Format("({0}, {1})", (int)_anchorPoint.X, (int)_anchorPoint.Y);

            // calculate how far the vehicle can move in either direction
            string vehicleID = _bordersToVIDs[_selected];
            //double cellSize = gameGrid.RowDefinitions[0].ActualHeight;//ALWAYS RETURNS 133 REGARDLESS OF CELL SIZE. WHY?
            //_spaceAhead = _grid.GetOpenCells(vehicleID, true) * cellSize;
            //_spaceBehind = _grid.GetOpenCells(vehicleID, false) * cellSize;
            //aheadActual.Content = string.Format("{0}", (int)_spaceAhead);
            //behindActual.Content = string.Format("{0}", (int)_spaceBehind);

            openCellsBehind = _grid.GetOpenCells(vehicleID, false);
            openCellsAhead = _grid.GetOpenCells(vehicleID, true);
            VehicleStruct v = _grid.GetVehicleStuct(vehicleID); 

            // determine the boundary cells between which the vehicle can move
            if (v.vertical)
            {
                // set _furthestOpenCellBehind
                if (openCellsBehind == 0)
                {
                    // set the boundary cell to the vehicle's cell so there's no space to move
                    _furthestOpenCellBehind = _cellBorders[v.row, v.column];
                }
                else
                {
                    // set the boundary cell to the furthest open cell spaceOnLeft (i.e. above)
                    _furthestOpenCellBehind = _cellBorders[v.row - openCellsBehind, v.column];
                }

                double distanceBehind = _selected.TranslatePoint(new Point(0, 0), _furthestOpenCellBehind).Y; // delete this
                double distanceAhead; // delete this

                // set _nearestOccupiedCellAhead
                if (openCellsAhead == 0)
                {
                    // set the boundary cell to the vehicle's cell so there's no space to move
                    _nearestOccupiedCellAhead = _cellBorders[v.row, v.column];
                    distanceAhead = 0; // delete this
                }
                else
                {
                    // set the boundary cell to the nearest open cell spaceOnRight (i.e. below); could be a wall/hidden cell
                    _nearestOccupiedCellAhead = _cellBorders[v.row + v.length + openCellsAhead, v.column];
                    distanceAhead = _selected.TranslatePoint(new Point(0, _selected.ActualHeight), _nearestOccupiedCellAhead).Y; // delete this
                }
                // delete this
                behindActual.Content = string.Format("{0}", (int)distanceBehind);
                aheadActual.Content = string.Format("{0}", (int)distanceAhead);
                // end delete this
            }
            // horizontal vehicle
            else
            {
                // set _furthestOpenCellBehind
                if (openCellsBehind == 0)
                {
                    // set the boundary cell to the vehicle's cell so there's no space to move
                    _furthestOpenCellBehind = _cellBorders[v.row, v.column];
                }
                else
                {
                    // set the boundary cell to the furthest open cell spaceOnLeft (i.e. to the left)
                    _furthestOpenCellBehind = _cellBorders[v.row, v.column - openCellsBehind];
                }

                double distanceBehind = _selected.TranslatePoint(new Point(0, 0), _furthestOpenCellBehind).X; // delete this
                double distanceAhead; // delete this

                // _nearestOccupiedCellAhead
                if (openCellsAhead == 0)
                {
                    // set the boundary cell to the vehicle's cell so there's no space to move
                    _nearestOccupiedCellAhead = _cellBorders[v.row, v.column];
                    distanceAhead = 0; // delete this
                }
                else
                {
                    _nearestOccupiedCellAhead = _cellBorders[v.row, v.column + v.length + openCellsAhead];
                    distanceAhead = _selected.TranslatePoint(new Point(_selected.ActualWidth, 0), _nearestOccupiedCellAhead).X; // delete this
                }

                behindActual.Content = string.Format("{0}", (int)distanceBehind); // delete this            
                aheadActual.Content = string.Format("{0}", (int)distanceAhead); // delete this 
            }

            if (element != null)
                element.CaptureMouse();

            _isInDrag = true;
            e.Handled = true;
        }

        // TODO: THE ANCHORPOINT NEEDS TO BE UPDATED WHEN DIRECTIONS CHANGE (E.G. LEFT TO RIGHT OR UP TO DOWN). OTHERWISE YOU HAVE TO WAIT
        // TO MOVE THE POINTER BACK TO, THEN BEYOND THE ORIGINAL ANCHOR POINT BEFORE THE VEHICLE STARTS MOVING AGAIN.
        // ACTUALLY, THIS SEEMS TO ONLY OCCUR WHEN YOU REACH A BOUNDARY (I.E. THE "space..." VARIABLE HITS ZERO).
        
        private void root_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isInDrag)
            {
                TranslateTransform _transform = new TranslateTransform(); // ORIGINAL
                //_transform = new TranslateTransform();
                
                //var element = sender as FrameworkElement;
                Border vehicleBorder = sender as Border;
                //_currentPoint = e.GetPosition(null);// ORIGINAL
                //_currentPoint = e.GetPosition(gameGrid);
                //_currentPoint = Mouse.GetPosition(null); // BORDER FALLS BEHIND POINTER DURING DRAG (SAME BEHAVIOR AS ABOVE?)
                _currentPoint = Mouse.GetPosition(gameGrid); // BORDER DRAGS WITH POINTER AS DESIRED
                currentPositionActual.Content = string.Format("({0}, {1})", (int)_currentPoint.X, (int)_currentPoint.Y); // delete this

                string vehicleID = _bordersToVIDs[vehicleBorder];
                VehicleStruct v = _grid.GetVehicleStuct(vehicleID);

                // TODO: WHAT ABOUT TRYING TO MOVE A HORIZONTAL VEHICLE VERTICALLY OR VICE VERSA? THEN, FOR EXAMPLE,
                // YOU END UP IN THE HORIZONTAL BLOCK BUT deltaX IS ZERO AND deltaY IS NON-ZERO
                if (v.vertical)
                {
                    double deltaY = _currentPoint.Y - _anchorPoint.Y; // WORKS PRETTY WELL, BUT BORDER CAN GO SLIGHTLY BEYOND BOUNDARY
                    deltaActual.Content = string.Format("{0}", (int)deltaY); // delete this

                    if (deltaY < 0)  // moving up
                    {
                        double spaceAbove = vehicleBorder.TranslatePoint(new Point(0, 0), _furthestOpenCellBehind).Y;
                        behindActual.Content = string.Format("{0}", (int)spaceAbove); // delete this

                        //if (_grid.IsCellOpen(v.row - 1, v.column))
                        //if (_spaceBehind > 0)
                        if (spaceAbove > 0)
                        {
                            _transform.Y += deltaY;
                            //_transform.Y += spaceOnLeft;
                            //_spaceAhead += deltaY / 133.0;
                            //_spaceBehind += deltaY / 133.0;
                            vehicleBorder.RenderTransform = _transform;
                        }
                        //else
                        //{
                        //    _anchorPoint = _currentPoint;
                        //}
                    }   
                    else if (deltaY > 0) // moving down
                    {
                        double spaceBelow;

                        if (openCellsAhead == 0)
                        {
                            spaceBelow = 0;
                        }
                        else
                        {
                            spaceBelow = vehicleBorder.TranslatePoint(new Point(0.0, vehicleBorder.ActualHeight), _nearestOccupiedCellAhead).Y;
                        }

                        aheadActual.Content = string.Format("{0}", (int)spaceBelow);

                        //if (_grid.IsCellOpen(v.row + v.length, v.column))
                        //if (_spaceAhead > 0)
                        if (spaceBelow < 0)
                        {
                            _transform.Y += deltaY;
                            //_spaceAhead -= deltaY / 133.0;
                            //_spaceBehind += deltaY / 133.0;
                            vehicleBorder.RenderTransform = _transform;
                        }
                        //else
                        //{
                        //    _anchorPoint = _currentPoint;
                        //}
                    }                                    
                }
                // horizontal
                else
                {
                    double deltaX = _currentPoint.X - _anchorPoint.X;
                    deltaActual.Content = string.Format("{0}", (int)deltaX); // delete this
                    
                    if (deltaX < 0) // moving left
                    {
                        double spaceOnLeft = vehicleBorder.TranslatePoint(new Point(0, 0), _furthestOpenCellBehind).X;
                        behindActual.Content = string.Format("{0}", (int)spaceOnLeft); // delete this

                        //if (_grid.IsCellOpen(v.row - 1, v.column))
                        //if (_spaceBehind > 0)
                        if (spaceOnLeft > 0)
                        {
                            _transform.X += deltaX;
                            //_transform.X += spaceOnLeft;
                            //_spaceAhead += deltaX / 133.0;
                            //_spaceBehind += deltaX / 133.0;
                            vehicleBorder.RenderTransform = _transform;
                        }
                        //else
                        //{
                        //    _anchorPoint = _currentPoint;
                        //}
                    }
                    else if (deltaX > 0) // moving right
                    {
                        double spaceOnRight;

                        // TODO: THIS WON'T WORK BECAUSE IT'S NOT DYNAMIC, ONCE YOU MOVE LEFT YOU SHOULD BE ABLE TO MOVE BACK RIGHT
                        if (openCellsAhead == 0)
                        {
                            spaceOnRight = 0;
                        }
                        else
                        {
                            spaceOnRight = vehicleBorder.TranslatePoint(new Point(vehicleBorder.ActualWidth, 0), _nearestOccupiedCellAhead).X;
                        }
                        aheadActual.Content = string.Format("{0}", (int)spaceOnRight);

                        
                        //if (_grid.IsCellOpen(v.row + v.length, v.column))
                        //if (_spaceAhead > 0)
                        if (spaceOnRight < 0)
                        {
                            _transform.X += deltaX;
                            //_spaceAhead -= deltaX / 133.0;
                            //_spaceBehind += deltaX / 133.0;
                            vehicleBorder.RenderTransform = _transform;
                        }
                        //else
                        //{
                        //    _anchorPoint = _currentPoint;
                        //}
                    }     
                }
                //this.RenderTransform = _transform; // ORIGINAL
                //vehicleBorder.RenderTransform = _transform; // SEEMS TO MAKE NO DIFFERENCE IF THIS IS HERE OR IN EACH IF/ELSE ABOVE
                //aheadActual.Content = string.Format("{0}", (int)_spaceAhead);
                //behindActual.Content = string.Format("{0}", (int)_spaceBehind);


                //Point testing2 = _cellBorders.TranslatePoint(new Point(0.0, 0.0), sender as UIElement);
                //fromOriginActual.Content = string.Format("({0},{1})", (int)testing2.X, (int)testing2.Y);


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

                // TODO: THIS MAY MAKE THINGS WORK: DETERMINE WHAT CELL TO SET THE BORDER TO THEN SET THE TRANSFORM TO NULL.
                // THIS MAY FIX THE SNAPPING BACK ISSUE.

                Border vehicleBorder = sender as Border; // ADDED THIS LINE                
                vehicleBorder.RenderTransform = null; // ADDED THIS LINE. ENSURES THE SETROW/COLUMN UPDATE DOESN'T INCLUDE THE MOVEMENT FROM THE DRAG, I.E. THE SETROW/COLUMN ISN'T RELATIVE TO THE FINAL DRAG POSITION.
                //Grid.SetRow(vehicleBorder, 2);
                vehicleBorder.RaiseEvent(new RoutedEventArgs(Frame.LoadedEvent, vehicleBorder));
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
    //    if (_selected == null)
    //        return;
    //    bool vertical = Grid.GetRowSpan(_selected) > 1;

    //    // get ID of _selected Vehicle
    //    string vID = _bordersToVIDs[_selected];

    //    if (e.Key == Key.Left && !vertical)
    //    {
    //        e.Handled = true;
    //        if (_grid.MoveVehicle(vID, -1))
    //        {
    //            int destination = Grid.GetColumn(_selected) - 1;
    //            Grid.SetColumn(_selected, destination);
    //        }
    //    }
    //    else if (e.Key == Key.Right && !vertical)
    //    {
    //        e.Handled = true;
    //        if (_grid.MoveVehicle(vID, 1))
    //        {
    //            int destination = Grid.GetColumn(_selected) + 1;
    //            Grid.SetColumn(_selected, destination);
    //        }                    
    //    }
    //    else if (e.Key == Key.Up && vertical)
    //    {
    //        e.Handled = true;
    //        if (_grid.MoveVehicle(vID, -1))
    //        {
    //            int destination = Grid.GetRow(_selected) - 1;
    //            Grid.SetRow(_selected, destination);
    //        }                    
    //    }
    //    else if (e.Key == Key.Down && vertical)
    //    {
    //        e.Handled = true;
    //        if (_grid.MoveVehicle(vID, 1))
    //        {
    //            int destination = Grid.GetRow(_selected) + 1;
    //            Grid.SetRow(_selected, destination);
    //        }
    //    }            
    //}
}
