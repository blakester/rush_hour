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
using System.Threading;

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
                //vehicleBorder.MouseLeave += vehicleBorder_MouseLeave;

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

        //void vehicleBorder_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (_isInDrag)
        //        throw new NotImplementedException();
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
            //if (_isInDrag)
            if (Mouse.LeftButton == MouseButtonState.Pressed) // ALTERNATIVE TO ABOVE
            {
                e.Handled = true;
                return;
            }

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

        // ANOTHER APPROACH WHICH MAY BE LESS BUGGY (THOUGH MAYBE NOT AS COOL) WOULD BE TO SIMPLY HIGHTLIGHT CELLS BASED ON THE MOUSE
        // POSITION SHOWING WHERE THE VEHICLE *WOULD* MOVE TO UPON RELEASING THE MOUSE BUTTON, RATHER THAN ACTUALLY RENDERING THE VEHICLE
        // WITH EACH MOUSE MOVEMENT.

        // NOW THAT I HAVE A BETTER UNDERSTANDING OF THINGS, IS IT POSSIBLE TO DITCH THE TRANSLATEPOINT TECHNIQUE IN MOUSEMOVE?
        // MAYBE I COULD GO BACK TO MY OLD IDEA AND BASICALLY JUST USE THE distanceFromAnchor AND/OR changeInDistanceFromAnchor
        // TO DETERMINE BOUNDARIES. THE POSSIBLE BENEFIT I'D SEE TO THIS IS IT COULD BE FASTER DOING SOME ARITHMETIC RATHER THAN
        // HAVING TO CALL TRANSLATEPOINT, AND ITS POSSIBLE THAT THE spaceAhead/Behind VALUES IT RETURNS ARE PART OF THE PROBLEM.
        // I'D STILL USE TRANSLATEPOINT, HOWEVER, IN THE MOUSE DOWN TO DETERMINE THE INITIAL DISTANCES.

        private Point _anchorMousePoint;
        private Point _currentMousePoint;
        private Point _lastMousePoint;
        private double _spaceBehind; // space to left/top-most boundary (greater than or equal to zero)
        private double _spaceAhead;  // space to right/bottom-most boundary (less than or equal to zero)
        private bool _isInDrag; // TODO: IS THIS NEEDED? IT SEEMS SIMPLY CHECKING IF MOUSELEFTBUTTON == PRESSED IN MOUSEMOVE IS SUFFICIENT
        private readonly TranslateTransform _transform = new TranslateTransform(); // ORIGINAL
        //private TranslateTransform _transform = new TranslateTransform(); // BORDERS DISAPPEAR ON DRAG
        //private TranslateTransform _transform; // BORDER SNAPS TO ORIGINAL POSITION ON SECOND DRAG

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

            //_transform = new TranslateTransform(); // ADDED, NOT IN HERE ORIGINALLY, CAUSES BORDERS TO SHOOT OFF SCREEN VS BEING INSTANTIATED IN MOUSEMOVE WHERE IT WORKS AS DESIRED

            // ORIGINAL
            var element = sender as FrameworkElement;
            _anchorMousePoint = e.GetPosition(gameGrid); // BORDER DRAGS WITH POINTER AS DESIRED (SAME BEHAVIOR AS ABOVE TWO?)
            _lastMousePoint = e.GetPosition(gameGrid);
            anchorPositionActual.Content = string.Format("({0}, {1})", (int)_anchorMousePoint.X, (int)_anchorMousePoint.Y); // delete

            string vehicleID = _bordersToVIDs[_selected];
            int openCellsBehind = _grid.GetOpenCells(vehicleID, false);
            int openCellsAhead = _grid.GetOpenCells(vehicleID, true);
            VehicleStruct v = _grid.GetVehicleStuct(vehicleID);

            Border furthestOpenCellBehind;
            Border nearestOccupiedCellAhead;

            // determine the boundary cells between which the vehicle can move
            if (v.vertical)
            {
                // set furthestOpenCellBehind
                if (openCellsBehind == 0)
                {
                    _spaceBehind = 0;
                }
                else
                {
                    // set the boundary cell to the furthest open cell spaceOnLeft (i.e. above)
                    furthestOpenCellBehind = _cellBorders[v.row - openCellsBehind, v.column];
                    _spaceBehind = _selected.TranslatePoint(new Point(0, 0), furthestOpenCellBehind).Y;
                }

                // set nearestOccupiedCellAhead
                if (openCellsAhead == 0)
                {
                    _spaceAhead = 0; // delete this
                }
                else
                {
                    // set the boundary cell to the nearest open cell spaceOnRight (i.e. below); could be a wall/hidden cell
                    nearestOccupiedCellAhead = _cellBorders[v.row + v.length + openCellsAhead, v.column];
                    _spaceAhead = _selected.TranslatePoint(new Point(0, _selected.ActualHeight), nearestOccupiedCellAhead).Y; // delete this
                }
            }
            // horizontal vehicle
            else
            {
                // set furthestOpenCellBehind
                if (openCellsBehind == 0)
                {
                    _spaceBehind = 0;
                }
                else
                {
                    // set the boundary cell to the furthest open cell spaceOnLeft (i.e. to the left)
                    furthestOpenCellBehind = _cellBorders[v.row, v.column - openCellsBehind];
                    _spaceBehind = _selected.TranslatePoint(new Point(0, 0), furthestOpenCellBehind).X;
                }

                // set nearestOccupiedCellAhead
                if (openCellsAhead == 0)
                {
                    _spaceAhead = 0;
                }
                else
                {
                    nearestOccupiedCellAhead = _cellBorders[v.row, v.column + v.length + openCellsAhead];
                    _spaceAhead = _selected.TranslatePoint(new Point(_selected.ActualWidth, 0), nearestOccupiedCellAhead).X;
                }
            }

            behindActual.Content = string.Format("{0}", (int)_spaceBehind); // delete this            
            aheadActual.Content = string.Format("{0}", (int)_spaceAhead); // delete this 

            if (element != null)
                element.CaptureMouse();

            //_isInDrag = true;
            e.Handled = true;
        }



        private void root_MouseMove(object sender, MouseEventArgs e)
        {
            //if (_isInDrag)
            if (Mouse.LeftButton == MouseButtonState.Pressed) // FIX 1 (NO FIX 2 NEEDED)
            {               
                // TODO: *FIX FOR BELOW FOR BUG*: RELEASING A DRAG ON WITH THE CURSOR OVER A VEHICLE OTHER THAN
                // THE DRAGGED VEHICLE TRIGGERS THIS HANDLER AND APPLIES THE TRANSFORM TO SAID OTHER VEHICLE.
                Border vehicleBorder = sender as Border;
                //if (!vehicleBorder.IsMouseCaptured) // FIX 2 (ALTERNATIVE TO FIX 1)
                //{
                //    e.Handled = true;
                //    return;
                //}
                
                _currentMousePoint = e.GetPosition(gameGrid);
                bool vertical = Grid.GetRowSpan(vehicleBorder) > 1;
                double mousePointDelta;
                double mouseRelativeToVehicle;
                double vehicleExtent;

                if (vertical)
                {
                    mousePointDelta = _currentMousePoint.Y - _lastMousePoint.Y;
                    mouseRelativeToVehicle = e.GetPosition(vehicleBorder).Y;
                    vehicleExtent = vehicleBorder.ActualHeight;
                }
                else
                {
                    mousePointDelta = _currentMousePoint.X - _lastMousePoint.X;
                    mouseRelativeToVehicle = e.GetPosition(vehicleBorder).X;
                    vehicleExtent = vehicleBorder.ActualWidth;
                }

                _lastMousePoint = _currentMousePoint;
                bool mouseOutsideRange = mouseRelativeToVehicle < 0 || mouseRelativeToVehicle > vehicleExtent;

                relativePosActual.Content = string.Format("{0}", (int)mouseRelativeToVehicle); // delete this

                // ignore drags where the mouse isn't over the vehicle
                if (mouseOutsideRange && (_spaceBehind == 0 || _spaceAhead == 0))
                {
                    //e.Handled = true;
                    return;
                }

                // moving behind (up/left)
                if (mousePointDelta < 0)
                {
                    // only render the vehicle if the amount it will move is within the available space
                    // (_spaceBehind is always >= 0, so check if the added (negative) delta is in that range)
                    if (_spaceBehind + mousePointDelta >= 0)
                    {
                        _transform.X += (vertical ? 0 : mousePointDelta);
                        _transform.Y += (vertical ? mousePointDelta : 0);
                        _spaceBehind += mousePointDelta;
                        _spaceAhead += mousePointDelta;
                    }
                    // the pending delta is too far; use all the space and place vehicle at the back boundary
                    else
                    {
                        _transform.X -= (vertical ? 0 : _spaceBehind);
                        _transform.Y -= (vertical ? _spaceBehind : 0);
                        _spaceBehind = 0;
                        _spaceAhead -= _spaceBehind;
                    }
                }

                // moving ahead (down/right)
                else
                {
                    // only render the vehicle if the amount it will move is within the available space
                    // (_spaceAhead is always <= 0, so check if the added delta is in that range)
                    if (_spaceAhead + mousePointDelta <= 0)
                    {
                        _transform.X += (vertical ? 0 : mousePointDelta);
                        _transform.Y += (vertical ? mousePointDelta : 0);
                        _spaceBehind += mousePointDelta;
                        _spaceAhead += mousePointDelta;
                    }
                    // the pending delta is too far; use all the space and place vehicle at the forward boundary
                    else
                    {
                        _transform.X -= (vertical ? 0 : _spaceAhead);
                        _transform.Y -= (vertical ? _spaceAhead : 0);
                        _spaceBehind -= _spaceAhead;
                        _spaceAhead = 0;
                    }
                }
                vehicleBorder.RenderTransform = _transform;
                behindActual.Content = string.Format("{0}", (int)_spaceBehind); // delete this            
                aheadActual.Content = string.Format("{0}", (int)_spaceAhead); // delete this
            }
        }


        private void root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (_isInDrag)
            //{
                var element = sender as FrameworkElement;

                if (element != null)
                    element.ReleaseMouseCapture();

                _isInDrag = false;
                e.Handled = true;

                //DragDrop.DoDragDrop(element, element, DragDropEffects.Move); // ADDED THIS LINE

                // determine the cell to place vehicle
                Border vehicleBorder = sender as Border; // ADDED THIS LINE                  
                vehicleBorder.RenderTransform = null; // only used during drag, we don't want to apply it after setting new position
                int vehicleRow = Grid.GetRow(vehicleBorder);
                int vehicleColumn = Grid.GetColumn(vehicleBorder);
                double cellSize = _cellBorders[0, 0].ActualHeight;
                bool wasVehicleMoved;

                // TODO: UNLESS I CAN ABSOLUTELY STOP VEHICLES FROM MOVING PAST BOUNDARIES, THIS CODE
                // WILL NEED TO MAKE SURE THE VEHICLE PLACEMENT IS VALID. EITHER SET TO NEAREST VALID
                // OR RESET COMPLETELY. I'VE BEEN ABLE TO TRIGGER EXCEPTIONS IN HERE BUT I CANT REMEMBER
                // EXACTLY WHICH ONES, INDEX OUT OF BOUNDS MAYBE?
                if (Grid.GetRowSpan(vehicleBorder) > 1) // vertical vehicle
                {
                    Point pointFromTopMostWall = vehicleBorder.TranslatePoint(new Point(0, 0), _cellBorders[0, vehicleColumn]);
                    int nearestRow = (int)Math.Round(pointFromTopMostWall.Y / cellSize, 0);
                    int spacesMoved = (nearestRow - vehicleRow);
                    Grid.SetRow(vehicleBorder, nearestRow);
                    wasVehicleMoved = _grid.MoveVehicle(_bordersToVIDs[vehicleBorder], spacesMoved);
                }
                else // horizontal vehicle
                {
                    Point pointFromLeftMostWall = vehicleBorder.TranslatePoint(new Point(0, 0), _cellBorders[vehicleRow, 0]);
                    int nearestColumn = (int)Math.Round(pointFromLeftMostWall.X / cellSize, 0);
                    int spacesMoved = (nearestColumn - vehicleColumn);
                    Grid.SetColumn(vehicleBorder, nearestColumn);
                    wasVehicleMoved = _grid.MoveVehicle(_bordersToVIDs[vehicleBorder], spacesMoved);
                }

                // TODO: I THINK THIS HAS BUGS
                solutionMoveButton.IsEnabled = !wasVehicleMoved;

                // reset the transform (EITHER IN HERE OR IN MOUSEDOWN)
                _transform.X = 0;
                _transform.Y = 0;

                if (_grid.Solved)
                    vehicleBorder.Background = Brushes.Green;

                //vehicleBorder.RaiseEvent(new RoutedEventArgs(Frame.LoadedEvent, vehicleBorder));
            //}
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
