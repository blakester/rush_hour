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
        private Border _selectedBorder = null;
        private Dictionary<Border, string> _bordersToVIDs = new Dictionary<Border, string>(32);
        private Dictionary<string, Border> _vIDsToBorders = new Dictionary<string, Border>(32);
        private Border[,] _cellBorders;
        private VehicleGrid _vehicleGrid;
        private int _initialConfig = 1;

        // fields used for dragging _vehicles
        private Point _anchorMousePoint;  // initial position of mouse on left button click
        private Point _currentMousePoint; // current position of mouse during a left button click (i.e. a drag)
        private Point _lastMousePoint;    // last position of mouse in most recent mouse move event
        private double _spaceBehind;      // space to left/top-most boundary (greater than or equal to zero)
        private double _spaceAhead;       // space to right/bottom-most boundary (less than or equal to zero)
        private bool _isInDrag;           // is the mouse left button down
        private readonly TranslateTransform _transform = new TranslateTransform(); // transform to render the dragged vehicle
        //private  TranslateTransform _transform; // transform to render the dragged vehicle // TODO: STACKOVERFLOW SEEMS TO RECOMMEND REINITIALIZING THIS EACH TIME. BUT DOES IT MAKE THE OVERLAPPING VEHICLES BUG MORE COMMON? MY GUESS IS NO.


        public MainWindow()
        {
            InitializeComponent();

            try
            {
                _vehicleGrid = new VehicleGrid("../../../configurations.txt", _initialConfig);
            }
            catch (Exception ex)
            {
                // TODO: HOW TO HANDLE BAD CONFIG FILES?
            }

            configEntryBox.Text = _initialConfig.ToString();
            configEntryBox.Maximum = _vehicleGrid.TotalConfigs;
            SetGameGrid();
            //Panel.SetZIndex(solutionMoveButton, -1); // MAY BE USEFUL FOR VEHEICLES/BORDERS TO SIT ABOVE A GRID IMAGE
        }

        private void SetGameGrid()
        {
            _bordersToVIDs.Clear();
            _vIDsToBorders.Clear();
            gameGrid.Children.Clear();

            // ONLY DO THESE IF NEW GRID HAS DIFFERENT DIMENSIONS THAN THE CURRENT GRID? ************************************
            gameGrid.RowDefinitions.Clear();
            gameGrid.ColumnDefinitions.Clear();
            _cellBorders = new Border[_vehicleGrid.Rows + 1, _vehicleGrid.Columns + 1]; // plus one for hidden cells

            // set gameGrid rows and columns according to the configuration
            for (int i = 0; i <= _vehicleGrid.Rows; i++)
            {
                gameGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i <= _vehicleGrid.Columns; i++)
            {
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // hide the hidden cells used to calculate movable space
            gameGrid.RowDefinitions[_vehicleGrid.Rows].Height = new GridLength(0);
            gameGrid.ColumnDefinitions[_vehicleGrid.Columns].Width = new GridLength(0);

            // place a Border on each cell of the _vehicleGrid
            for (int i = 0; i <= _vehicleGrid.Rows; i++)
            {
                for (int j = 0; j <= _vehicleGrid.Columns; j++)
                {
                    // add the cell to the _grid and cell borders array
                    Border cellBorder = new Border();
                    gameGrid.Children.Add(cellBorder);
                    Grid.SetRow(cellBorder, i);
                    Grid.SetColumn(cellBorder, j);
                    _cellBorders[i, j] = cellBorder;

                    // hide the hidden cells (though they should already be hidden)
                    if (i == _vehicleGrid.Rows || j == _vehicleGrid.Columns)
                    {
                        cellBorder.Visibility = Visibility.Hidden;
                        continue;
                    }

                    // stylize the cells
                    cellBorder.BorderThickness = new Thickness(4);
                    cellBorder.BorderBrush = Brushes.Black;
                    cellBorder.Background = Brushes.Aqua;
                }
            }

            // represent each Vehicle as a Border
            foreach (VehicleStruct vd in _vehicleGrid.GetVehicleStucts())
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

                // add the Border to the _vehicleGrid
                gameGrid.Children.Add(vehicleBorder);

                // set up event handlers for the Border
                vehicleBorder.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(VehicleBorder_MouseLeftButtonDown);
                vehicleBorder.MouseMove += new MouseEventHandler(VehicleBorder_MouseMove);
                vehicleBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(VehicleBorder_MouseLeftButtonUp);
                vehicleBorder.MouseEnter += (s, e) => Mouse.OverrideCursor = Cursors.Hand;  // cursor is hand icon over vehicle
                vehicleBorder.MouseLeave += (s, e) => Mouse.OverrideCursor = Cursors.Arrow; // cursor is regular not over vehicle
                vehicleBorder.KeyDown += new KeyEventHandler(border_KeyDown);
                vehicleBorder.Focusable = true;

                // position the Border on the _vehicleGrid
                Grid.SetRow(vehicleBorder, vd.row);
                Grid.SetColumn(vehicleBorder, vd.column);
                //Panel.SetZIndex(vehicleBorder, 0);

                // add the Border and Vehicle to lookup tables
                _bordersToVIDs.Add(vehicleBorder, vd.id);
                _vIDsToBorders.Add(vd.id, vehicleBorder);
                solutionMoveButton.IsEnabled = true;
            }
        }

        // ANOTHER APPROACH WHICH MAY BE LESS BUGGY (THOUGH MAYBE NOT AS COOL) WOULD BE TO SIMPLY HIGHTLIGHT CELLS BASED ON THE MOUSE
        // POSITION SHOWING WHERE THE VEHICLE *WOULD* MOVE TO UPON RELEASING THE MOUSE BUTTON, RATHER THAN ACTUALLY RENDERING THE VEHICLE
        // WITH EACH MOUSE MOVEMENT.

        // TODO: APPARENTLY IT'S STILL POSSIBLE TO DRAG A VEHICLE THROUGH ANOTHER. SEEMS TO HAPPEN ON FORWARD MOVEMENTS.
        // THIS IS RARE AND IT'S HARD TO RECREATE, BUT IT SEEMS IT PERHAPS HAPPENS SOMETIMES IF YOU QUICKLY AND REPEATEDLY
        // CLICK BETWEEN DIFFERENT VEHICLES THEN HURRY AND DRAG ONE TO THE FORWARD BOUNDARY. THIS BUG SEEMS UNLIKELY TO OCCUR
        // DURING PRACTICAL USE, BUT IS THERE ANYTHING I CAN DO? OR IS IT SIMPLY A TIMING ISSUE AND THE CALCS CAN'T KEEP UP
        // WITH THE EVENTS?


        private void VehicleBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // remove highlighting from previously _selectedBorder
            if (_selectedBorder != null)
                _selectedBorder.BorderBrush = null;
            
            Border vehicleBorder = (Border)sender;
            //_transform = new TranslateTransform();

            // update _selectedBorder
            _selectedBorder = vehicleBorder;
            _selectedBorder.BorderBrush = Brushes.Blue;
            vehicleBorder.Focus(); // give focus so arrow key movements will be recognized

            // update mouse positions
            _anchorMousePoint = e.GetPosition(gameGrid);
            _lastMousePoint = e.GetPosition(gameGrid);
            //anchorPositionActual.Content = string.Format("({0}, {1})", (int)_anchorMousePoint.X, (int)_anchorMousePoint.Y); // delete

            string vehicleID = _bordersToVIDs[vehicleBorder];
            int openCellsBehind = _vehicleGrid.GetOpenCellsBehind(vehicleID);
            int openCellsAhead = _vehicleGrid.GetOpenCellsAhead(vehicleID);
            VehicleStruct v = _vehicleGrid.GetVehicleStuct(vehicleID);
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
                    // set the boundary cell to the furthest open cell behind (i.e. above)
                    furthestOpenCellBehind = _cellBorders[v.row - openCellsBehind, v.column];
                    _spaceBehind = vehicleBorder.TranslatePoint(new Point(0, 0), furthestOpenCellBehind).Y;
                }

                // set nearestOccupiedCellAhead
                if (openCellsAhead == 0)
                {
                    _spaceAhead = 0;
                }
                else
                {
                    // set the boundary cell to the nearest occupied cell ahead (i.e. below); could be a wall/hidden cell
                    nearestOccupiedCellAhead = _cellBorders[v.row + v.length + openCellsAhead, v.column];
                    _spaceAhead = vehicleBorder.TranslatePoint(new Point(0, _selectedBorder.ActualHeight), nearestOccupiedCellAhead).Y;
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
                    // set the boundary cell to the furthest open cell behind (i.e. on left)
                    furthestOpenCellBehind = _cellBorders[v.row, v.column - openCellsBehind];
                    _spaceBehind = vehicleBorder.TranslatePoint(new Point(0, 0), furthestOpenCellBehind).X;
                }

                // set nearestOccupiedCellAhead
                if (openCellsAhead == 0)
                {
                    _spaceAhead = 0;
                }
                else
                {
                    // set the boundary cell to the nearest occupied cell ahead (i.e. on right); could be a wall/hidden cell
                    nearestOccupiedCellAhead = _cellBorders[v.row, v.column + v.length + openCellsAhead];
                    _spaceAhead = vehicleBorder.TranslatePoint(new Point(_selectedBorder.ActualWidth, 0), nearestOccupiedCellAhead).X;
                }
            }

            //behindActual.Content = string.Format("{0}", (int)_spaceBehind); // delete this            
            //aheadActual.Content = string.Format("{0}", (int)_spaceAhead); // delete this 

            vehicleBorder.CaptureMouse(); // begin tracking mouse movements
            _isInDrag = true;
            e.Handled = true;
        }



        private void VehicleBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isInDrag)
            //if (Mouse.LeftButton == MouseButtonState.Pressed) // FIX 1 (COULD NOT REPRODUCE, BUT MULTIPLE VEHICLE TRANSFORM SOMEHOW OCCURED WITH THIS FIX)
            {
                // TODO: *FIX FOR BELOW FOR BUG*: RELEASING A DRAG ON WITH THE CURSOR OVER A VEHICLE OTHER THAN
                // THE DRAGGED VEHICLE TRIGGERS THIS HANDLER AND APPLIES THE TRANSFORM TO SAID OTHER VEHICLE.
                Border vehicleBorder = sender as Border;
                if (!vehicleBorder.IsMouseCaptured) // FIX 2 (ALTERNATIVE TO FIX 1)
                {
                    e.Handled = true;
                    return;
                }

                _currentMousePoint = e.GetPosition(gameGrid);
                bool vertical = Grid.GetRowSpan(vehicleBorder) > 1;
                double mousePointDelta;        // distance from _lastMousePoint
                double mouseRelativeToVehicle; // mouse position relative to top-right corner of selected vehicle
                double vehicleExtent;          // how tall or wide is the vehicle

                if (vertical)
                {
                    mousePointDelta = _currentMousePoint.Y - _lastMousePoint.Y;
                    mouseRelativeToVehicle = e.GetPosition(vehicleBorder).Y;
                    vehicleExtent = vehicleBorder.ActualHeight;
                }
                else // horizontal
                {
                    mousePointDelta = _currentMousePoint.X - _lastMousePoint.X;
                    mouseRelativeToVehicle = e.GetPosition(vehicleBorder).X;
                    vehicleExtent = vehicleBorder.ActualWidth;
                }
                _lastMousePoint = _currentMousePoint; // update _lastMousePoint

                bool mouseOutsideRange = mouseRelativeToVehicle < 0 || mouseRelativeToVehicle > vehicleExtent;

                //relativePosActual.Content = string.Format("{0}", (int)mouseRelativeToVehicle); // delete this

                // ignore drags where the mouse isn't over the vehicle
                if (mouseOutsideRange && (_spaceBehind == 0 || _spaceAhead == 0))
                {
                    e.Handled = true;
                    return;
                }

                // this function greatly remedies the gap that develops on the forward boundary when dragging a vehicle 
                // back and forth between boundaries, however repeated super fast drags can still trigger cell-sized gaps
                Action ResetSpaceAhead = () =>
                {
                    double unitSize = _cellBorders[0, 0].ActualHeight * -1;
                    _spaceAhead = Math.Ceiling(_spaceAhead / unitSize) * unitSize;
                };

                // moving behind (i.e. up or left)
                if (mousePointDelta < 0)
                {
                    // only render the vehicle if the amount it will move is within the available space
                    // (_spaceBehind is always >= 0, so check if the added (negative) delta is in that range)
                    if (_spaceBehind + mousePointDelta >= 0)
                    {
                        _transform.X += (vertical ? 0 : mousePointDelta); // ignore horizontal transformations if vertical
                        _transform.Y += (vertical ? mousePointDelta : 0); // ignore vertical transformations if horizontal
                        _spaceBehind += mousePointDelta;
                        _spaceAhead += mousePointDelta;
                    }
                    // the pending delta is too far; use all the space and place vehicle at the back boundary
                    else
                    {
                        _transform.X -= (vertical ? 0 : _spaceBehind);
                        _transform.Y -= (vertical ? _spaceBehind : 0);
                        _spaceBehind = 0;
                        ResetSpaceAhead();
                    }
                }

                // moving ahead (i.e. down or right)
                else
                {
                    // only render the vehicle if the amount it will move is within the available space
                    // (_spaceAhead is always <= 0, so check if the added delta is in that range)
                    if (_spaceAhead + mousePointDelta <= 0)
                    {
                        _transform.X += (vertical ? 0 : mousePointDelta); // ignore horizontal transformations if vertical
                        _transform.Y += (vertical ? mousePointDelta : 0); // ignore vertical transformations if horizontal
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
                vehicleBorder.RenderTransform = _transform; // render the vehicle in its new position
                //behindActual.Content = string.Format("{0}", (int)_spaceBehind); // delete this            
                //aheadActual.Content = string.Format("{0}", (int)_spaceAhead); // delete this
            }
        }


        private void VehicleBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isInDrag)
            {
                Border vehicleBorder = (Border)sender; 
                vehicleBorder.ReleaseMouseCapture(); // stop tracking mouse movements
                _isInDrag = false;
                e.Handled = true;

                // the transform is only used to render the drag
                vehicleBorder.RenderTransform = null;

                // determine the cell to place vehicle  
                int vehicleRow = Grid.GetRow(vehicleBorder);
                int vehicleColumn = Grid.GetColumn(vehicleBorder);
                double cellSize = _cellBorders[0, 0].ActualHeight;
                bool wasVehicleMoved;

                if (Grid.GetRowSpan(vehicleBorder) > 1) // vertical vehicle
                {
                    Point pointFromTopMostWall = vehicleBorder.TranslatePoint(new Point(0, 0), _cellBorders[0, vehicleColumn]);
                    int nearestRow = (int)Math.Round(pointFromTopMostWall.Y / cellSize, 0);
                    int spacesMoved = (nearestRow - vehicleRow);
                    Grid.SetRow(vehicleBorder, nearestRow);
                    wasVehicleMoved = _vehicleGrid.MoveVehicle(_bordersToVIDs[vehicleBorder], spacesMoved); // move vehicle in underlying _grid
                }
                else // horizontal vehicle
                {
                    Point pointFromLeftMostWall = vehicleBorder.TranslatePoint(new Point(0, 0), _cellBorders[vehicleRow, 0]);
                    int nearestColumn = (int)Math.Round(pointFromLeftMostWall.X / cellSize, 0);
                    int spacesMoved = (nearestColumn - vehicleColumn);
                    Grid.SetColumn(vehicleBorder, nearestColumn);
                    wasVehicleMoved = _vehicleGrid.MoveVehicle(_bordersToVIDs[vehicleBorder], spacesMoved); // move vehicle in underlying _grid
                }

                if (wasVehicleMoved)
                    solutionMoveButton.IsEnabled = false;

                // reset the transform // TODO: USE THIS OR SIMPLY REINITIALIZE TRANSORM ON MOUSEDOWN
                _transform.X = 0;
                _transform.Y = 0;

                if (_vehicleGrid.Solved) // TURNS ANY VEHICLE GREEN AFTER CLICKING ON IT
                    vehicleBorder.Background = Brushes.Green;
            }
        }


        private void solutionMoveButton_Click(object sender, RoutedEventArgs e)
        {
            VehicleStruct? movedVehicle = _vehicleGrid.NextSolutionMove();
            //if (((Button)sender).Equals(solutionMoveButton))
            //    movedVehicle = _vehicleGrid.NextSolutionMove();
            //else
            //    movedVehicle = _vehicleGrid.UndoSolutionMove(); // GET RID OF? CAN'T THINK OF REASONABLE USE-CASE.

            // THIS if SHOULDN'T BE NECESSARY->SOLUTION MOVES WILL ALWAYS WORK SO LONG AS THE BUTTON IS ENABLED
            if (movedVehicle.HasValue) // HANDLE NULL VehicleStruct (I.E. WHEN THE MOVE CAN'T BE MADE)
            {
                VehicleStruct mv = movedVehicle.Value;
                Border movedBorder = _vIDsToBorders[mv.id];
                Grid.SetRow(movedBorder, mv.row);
                Grid.SetColumn(movedBorder, mv.column);

                // disable button if puzzle is _solved
                if (_vehicleGrid.Solved)
                    solutionMoveButton.IsEnabled = false;
            }

            if (_selectedBorder != null)
                _selectedBorder.Focus();
        }

        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text); // THIS NEEDS TO BE VALIDATED, OR NON-NUMBERS SHOULD BE PROHIBITTED AT ENTRY
            _vehicleGrid.SetConfig(config);
            configEntryBox.Text = _vehicleGrid.CurrentConfig.ToString();
            SetGameGrid();
        }

        private void configEntryBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            //private static bool IsTextAllowed(string text)
            //{
            //    return !_regex.IsMatch(text);
            //}

            //int val;
            //string deleteme = e.Key.ToString();
            //if (!int.TryParse(e.Key.ToString(), out val))
            //{
            //    e.Handled = true;
            //}
            
        }

        private void configEntryBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int val;
            if (!int.TryParse(e.Text, out val))
            {
                e.Handled = true;
            }
        }

        private void configEntryBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null )
            {
                configEntryBox.Text = e.OldValue.ToString();
                e.Handled = true;
            }
            else if (int.Parse(e.NewValue.ToString()) > _vehicleGrid.TotalConfigs)
            {
                configEntryBox.Text = e.OldValue.ToString();
                e.Handled = true;
            }
            
        }

        private void configEntryBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int config = Int32.Parse(configEntryBox.Text); // THIS NEEDS TO BE VALIDATED, OR NON-NUMBERS SHOULD BE PROHIBITIED AT ENTRY
                _vehicleGrid.SetConfig(config);
                SetGameGrid();
            }
        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            _vehicleGrid.SetConfig(0);
            configEntryBox.Text = _vehicleGrid.CurrentConfig.ToString();
            SetGameGrid();
        }

        private void previousConfigButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text);
            if (config - 1 == 0)
                _vehicleGrid.SetConfig(_vehicleGrid.TotalConfigs);
            else
                _vehicleGrid.SetConfig(config - 1);
            configEntryBox.Text = _vehicleGrid.CurrentConfig.ToString();
            SetGameGrid();
        }

        private void nextConfigButton_Click(object sender, RoutedEventArgs e)
        {
            int config = Int32.Parse(configEntryBox.Text);
            if (config + 1 > _vehicleGrid.TotalConfigs)
                _vehicleGrid.SetConfig(1);
            else
                _vehicleGrid.SetConfig(config + 1);
            configEntryBox.Text = _vehicleGrid.CurrentConfig.ToString();
            SetGameGrid();
        }


        private void border_KeyDown(object sender, KeyEventArgs e)
        {
            // ignore keys while vehicle is being dragged
            if (_isInDrag)
            //if (Mouse.LeftButton == MouseButtonState.Pressed) // ALTERNATIVE TO ABOVE
            {
                e.Handled = true;
                return;
            }

            Border border = (Border)sender;
            bool vertical = Grid.GetRowSpan(border) > 1;

            // get ID of _selectedBorder Vehicle
            string vID = _bordersToVIDs[border];

            if (e.Key == Key.Left && !vertical)
            {
                if (_vehicleGrid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetColumn(border) - 1;
                    Grid.SetColumn(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Right && !vertical)
            {
                if (_vehicleGrid.MoveVehicle(vID, 1))
                {
                    int destination = Grid.GetColumn(border) + 1;
                    Grid.SetColumn(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Up && vertical)
            {
                if (_vehicleGrid.MoveVehicle(vID, -1))
                {
                    int destination = Grid.GetRow(border) - 1;
                    Grid.SetRow(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            else if (e.Key == Key.Down && vertical)
            {
                if (_vehicleGrid.MoveVehicle(vID, 1))
                {
                    int destination = Grid.GetRow(border) + 1;
                    Grid.SetRow(border, destination);
                    solutionMoveButton.IsEnabled = false;
                }
            }
            e.Handled = true;
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
}
