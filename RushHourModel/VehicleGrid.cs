using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace RushHourModel
{
    /// <summary>
    /// Represents a grid of Vehicles that can be moved around to solve the configuration/puzzle as in the game "Rush Hour"
    /// </summary>
    public class VehicleGrid
    {
        private byte[,] grid;                                                               // underlying grid
        private string[] configurations;                                                    // configuration/puzzle encodings
        private Dictionary<string, Vehicle> vehicles = new Dictionary<string, Vehicle>(32); // Vehicles in grid
        private List<string> solutionMoves = new List<string>(64);                          // moves to solve configuration
        private int nextSolutionMove;                                                       // index to next solution move
        private bool solved;                                                                // grid has been solved
        private ConcurrentBag<string> errors;                                               // holds all errors for multi-threaded validation
        private bool userMoveMade, solutionMoveMade;                                        

        /// <summary>
        /// Number of rows in the current configuration
        /// </summary>
        public int Rows
        { get; private set; }

        /// <summary>
        /// Number of columns in the current configuration
        /// </summary>
        public int Columns
        { get; private set; }

        /// <summary>
        /// Total number of configurations
        /// </summary>
        public int TotalConfigs
        { get { return configurations.Length; } }

        /// <summary>
        /// Current configuration number
        /// </summary>
        public int CurrentConfig
        { get; private set; }

        /// <summary>
        /// Difficulty of current configuration (1 being easiest)
        /// </summary>
        public int ConfigDifficulty
        { get; private set; }

        /// <summary>
        /// Indicates whether the current configuration has been solved.
        /// Resets to false on successful call to SetConfig() or ResetConfig().
        /// </summary>
        public bool Solved
        { 
            get { return solved; }
            private set { solved = value; }
        }


        /// <summary>
        /// Constructs a grid using the specified initialConfig from the specified configurationsFile.
        /// A random configuration from configurationsFile will be selected if initialConfig is less than 1.
        /// </summary>
        /// <param name="configurationsFilePath">path to text file with grid configurations</param>
        /// <param name="initialConfig">configuration to set grid to (configs start at 1)</param>
        public VehicleGrid(string configurationsFilePath, int initialConfig)
        {
            ValidateConfigurationsFile(configurationsFilePath);            
            SetConfig(initialConfig);            
        }


        /// <summary>
        /// Validates the specified configurations file and throws an exception if an error is encountered.
        /// </summary>
        /// <param name="filePath">path to the configurations file</param>
        public void ValidateConfigurationsFile(string filePath) // ************************************* SWITCH BACK TO PRIVATE **********
        {
            //configurations = new string[File.ReadLines(filePath).Count()];
            configurations = File.ReadLines(filePath).ToArray();
            if (configurations.Length == 0)
                throw new FileFormatException("Configurations file cannot be empty. File: '" + filePath + "'");

            Dictionary<string, Vehicle> tempVehicles = new Dictionary<string, Vehicle>(32);
            byte[,] tempGrid = new byte[6, 6]; // assume a 6X6 grid; change if needed

            int config = 1;
            //foreach (string line in File.ReadLines(filePath))
            foreach (string line in configurations)
            {
                // check for correct number of semicolon-delimited sections
                string[] sections = line.Split(';');
                if (sections.Length != 3)
                    throw new FileFormatException(string.Format("Expected 2 ';' (found {0}). File: '{1}', Line: {2}",
                        sections.Length - 1, filePath, config));

                // ~~~ Check section 1 ~~~ (difficulty, number of rows, number of columns)
                string[] settings = sections[0].Trim().Split(' ');
                int diff, rows, cols;
                if (settings.Length != 3 || !Int32.TryParse(settings[0], out diff) || !Int32.TryParse(settings[1], out rows) ||
                    !Int32.TryParse(settings[2], out cols) || diff < 1 || rows < 1 || cols < 1)
                    throw new FileFormatException(string.Format("Expected 3 positive integers. File: '{0}', Line: {1}, Section: '{2}'",
                        filePath, config, sections[0]));

                // ~~~ Check section 2 ~~~ (vehicle encodings)
                //byte[,] tempGrid = new byte[rows, cols];
                if (rows != tempGrid.GetLength(0) || cols != tempGrid.GetLength(1)) // only allocate new grid if dimensions have changed
                    tempGrid = new byte[rows, cols];
                else
                    Array.Clear(tempGrid, 0, tempGrid.Length);
                string[] vehicleEncodings = sections[1].Split(',');
                if (vehicleEncodings.Length == 1 && vehicleEncodings[0].Equals(""))
                    throw new FileFormatException(string.Format("One or more vehicle encodings required. File: '{0}', Line: {1}", filePath, config));                

                // validate each vehicle encoding (ID, row, col, vertical/horizontal, length)
                tempVehicles.Clear();
                foreach (string ve in vehicleEncodings)
                {
                    string[] vehicleData = ve.Trim().Split(' ');
                    int _row, _col, length;
                    if (vehicleData.Length != 5 || !Int32.TryParse(vehicleData[1], out _row) || !Int32.TryParse(vehicleData[2], out _col) ||
                        !Int32.TryParse(vehicleData[4], out length) || (!vehicleData[3].Equals("V") && !vehicleData[3].Equals("H")))
                        throw new FileFormatException(string.Format("Expected vehicle encoding of the form '$ I I (V|H) I' where $ is a string, I is a positive integer, and the fourth element is a V or H. File: '{0}', Line: {1}, Encoding: '{2}'", filePath, config, ve));

                    if (tempVehicles.ContainsKey(vehicleData[0]))
                        throw new FileFormatException(string.Format("Duplicate vehicle ID. File: '{0}', Line: {1}, Encoding: '{2}'", filePath, config, ve));

                    int row = _row - 1; // change row and col to zero-indexed
                    int col = _col - 1;
                    bool vertical = vehicleData[3].Equals("V");

                    if (row < 0 || col < 0 || length < 1 || (vertical && row + length > rows) || (!vertical && col + length > cols))
                        throw new FileFormatException(string.Format("Vehicle position and/or length is invalid or out of range. File: '{0}', Line: {1}, Encoding: '{2}'", filePath, config, ve));

                    // make sure vehicles don't overlap
                    if (vertical)
                        for (int i = 0; i < length; i++)
                        {
                            if (tempGrid[row + i, col] == 1)
                                throw new FileFormatException(string.Format("Vehicle overlap. File: '{0}', Line: {1}, Encoding: '{2}'",
                                    filePath, config, ve));
                            tempGrid[row + i, col] = 1;
                        }
                    else
                        for (int i = 0; i < length; i++)
                        {
                            if (tempGrid[row, col + i] == 1)
                                throw new FileFormatException(string.Format("Vehicle overlap. File: '{0}', Line: {1}, Encoding: '{2}'",
                                    filePath, config, ve));
                            tempGrid[row, col + i] = 1;
                        }
                    tempVehicles.Add(vehicleData[0], new Vehicle(row, col, vertical, length));
                }

                // ~~~ Check section 3 ~~~ (solution moves)
                bool solved = false;
                string[] solutionMoves = sections[2].Split(',');
                foreach (string sm in solutionMoves)
                {
                    // validate the format
                    string[] moveData = sm.Trim().Split(' ');
                    int spaces;
                    if (moveData.Length != 2 || !Int32.TryParse(moveData[1], out spaces))
                        throw new FileFormatException(string.Format("Expected solution move of the form '$ I' where $ is a string and I is an integer. File: '{0}', Line: {1}, Move: '{2}'", filePath, config, sm));

                    // validate vehicle ID
                    if (!tempVehicles.ContainsKey(moveData[0]))
                        throw new FileFormatException(string.Format("Undefined vehicle ID in solution move. File: '{0}', Line: {1}, Move: '{2}'", filePath, config, sm));

                    // execute/validate the move
                    if (!MoveVehiclePrivate(moveData[0], spaces, tempGrid, tempVehicles, true, out solved))
                        throw new FileFormatException(string.Format("Illegal solution move (ensure previous moves are also correct). File: '{0}', Line: {1}, Move: '{2}'", filePath, config, sm));
                }

                // make sure the moves solved the configuration
                if (!solved)
                    throw new FileFormatException(string.Format("Solution moves did not solve configuration. File: '{0}', Line: {1}", filePath, config));
                config++;
            }
        }


        // THIS SEEMS TO BE SLOWER THAN THE SINGLE-THREADED VERSION. PERHAPS USING AN IDEAL/DYNAMIC NUMBER OF THREADS
        // EACH WOULD HELP, BUT I DOUBT IT. THIS MIGHT ONLY COME IN HANDY IF YOU WERE PROCESSING A HUGE CONFIGURATIONS FILE,
        // PERHAPS HUNDREDS OR EVEN THOUSANDS OF LINES. HOWEVER, IF YOU REMOVE THE CountdownEvent, THIS IS INDEED VERY FAST,
        // BUT YOU'D HAVE TO GENERATE SOME SORT OF EVENT TO NOTIFY THE VIEW THAT A CONFIG WAS BAD SINCE THIS WOULD BECOME
        // AN ASYNCHRONOUS METHOD.
        /// <summary>
        /// Validates the specified configurations file using multiple threads and throws an exception reporting any/all errors.
        /// </summary>
        /// <param name="filePath">path to the configurations file</param>
        public void ValidateConfigurationsFile_MltThrd(string filePath) // ******* SWITCH BACK TO PRIVATE **********
        {
            configurations = File.ReadLines(filePath).ToArray();
            if (configurations.Length == 0)
                throw new FileFormatException("Configurations file cannot be empty. File: '" + filePath + "'");

            errors = new ConcurrentBag<string>();
            int workItems = Environment.ProcessorCount;             // NOT POSITIVE THIS IS BEST
            int linesPerThread = configurations.Length / workItems; // how many lines each thread will process
            int linesRemainder = configurations.Length % workItems; // extra lines to tack on to last thread

            // split the configurations array into 'workItems' pieces and validate with seperate threads
            using (var countdownEvent = new CountdownEvent(workItems))
            {
                for (int i = 0; i < workItems; i++)
                {
                    int start = i * linesPerThread;
                    int end = (i == workItems - 1) ? (start + linesPerThread + linesRemainder) : (start + linesPerThread);
                    ThreadPool.QueueUserWorkItem(
                            x =>
                            {
                                ValidateConfigs(configurations, start, end, filePath);
                                countdownEvent.Signal();
                            });
                }
                // don't proceed until all threads have completed
                countdownEvent.Wait();
            }

            // report any/all errors
            if (!errors.IsEmpty)
            {
                string errorMessage = "";
                foreach (string error in errors)
                    errorMessage += ("\n" + error);
                throw new FileFormatException("The following configuration error(s) were found:" + errorMessage);
            }
        }


        /// <summary>
        /// Validates the specified configs array from index start (inclusive) to end (exclusive) and uses
        /// the specified filePath simply for error messages.
        /// </summary>
        /// <param name="configs">configurations array</param>
        /// <param name="start">configuration index to start validation (inclusive)</param>
        /// <param name="end">configuration index to end validation (exclusive)</param>
        /// <param name="filePath">filePath of configurations file (used for error messages)</param>
        private void ValidateConfigs(string[] configs, int start, int end, string filePath)
        {
            for (int line = start; line < end; line++)
            {
                bool error = false;

                // check for correct number of semicolon-delimited sections
                string[] sections = configs[line].Split(';');
                if (sections.Length != 3)
                {
                    errors.Add(string.Format("Expected 2 ';' (found {0}). File: '{1}', Line: {2}",
                            sections.Length - 1, filePath, line + 1));
                    continue;
                }

                // ~~~ Check section 1 ~~~ (difficulty, number of rows, number of columns)
                string[] settings = sections[0].Trim().Split(' ');
                int diff, rows, cols;
                if (settings.Length != 3 || !Int32.TryParse(settings[0], out diff) || !Int32.TryParse(settings[1], out rows) ||
                    !Int32.TryParse(settings[2], out cols) || diff < 1 || rows < 1 || cols < 1)
                {
                    errors.Add(string.Format("Expected 3 positive integers. File: '{0}', Line: {1}, Section: '{2}'",
                        filePath, line + 1, sections[0]));
                    continue;
                }

                // ~~~ Check section 2 ~~~ (vehicle encodings)
                byte[,] tempGrid = new byte[rows, cols];
                if (rows != tempGrid.GetLength(0) || cols != tempGrid.GetLength(1)) // only allocate new grid if dimensions have changed
                    tempGrid = new byte[rows, cols];
                else
                    Array.Clear(tempGrid, 0, tempGrid.Length);
                string[] vehicleEncodings = sections[1].Split(',');
                if (vehicleEncodings.Length == 1 && vehicleEncodings[0].Equals(""))
                {
                    errors.Add(string.Format("One or more vehicle encodings required. File: '{0}', Line: {1}", filePath, line + 1));
                    continue;
                }

                // the Vehicles need to be stored so the solution moves can be validated
                Dictionary<string, Vehicle> tempVehicles = new Dictionary<string, Vehicle>(vehicleEncodings.Length * 2);

                // validate each vehicle encoding (ID, row, col, vertical/horizontal, length)
                foreach (string ve in vehicleEncodings)
                {
                    string[] vehicleData = ve.Trim().Split(' ');
                    int _row, _col, length;
                    if (vehicleData.Length != 5 || !Int32.TryParse(vehicleData[1], out _row) || !Int32.TryParse(vehicleData[2], out _col) ||
                        !Int32.TryParse(vehicleData[4], out length) || (!vehicleData[3].Equals("V") && !vehicleData[3].Equals("H")))
                    {
                        errors.Add(string.Format("Expected vehicle encoding of the form '$ I I (V|H) I' where $ is a string, I is a positive integer, and the fourth element is a V or H. File: '{0}', Line: {1}, Encoding: '{2}'", filePath, line + 1, ve));
                        error = true;
                        break;
                    }
                    int row = _row - 1; // change row and col to zero-indexed
                    int col = _col - 1;
                    bool vertical = vehicleData[3].Equals("V");

                    if (row < 0 || col < 0 || length < 1 || (vertical && row + length > rows) || (!vertical && col + length > cols))
                    {
                        errors.Add(string.Format("Vehicle position and/or length is invalid or out of range. File: '{0}', Line: {1}, Encoding: '{2}'", filePath, line + 1, ve));
                        error = true;
                        break;
                    }

                    // make sure vehicles don't overlap
                    if (vertical)
                        for (int i = 0; i < length; i++)
                        {
                            if (tempGrid[row + i, col] == 1)
                            {
                                errors.Add(string.Format("Vehicle overlap. File: '{0}', Line: {1}, Encoding: '{2}'",
                                        filePath, line + 1, ve));
                                error = true;
                                break;
                            }
                            tempGrid[row + i, col] = 1;
                        }
                    else
                        for (int i = 0; i < length; i++)
                        {
                            if (tempGrid[row, col + i] == 1)
                            {
                                errors.Add(string.Format("Vehicle overlap. File: '{0}', Line: {1}, Encoding: '{2}'",
                                        filePath, line + 1, ve));
                                error = true;
                                break;
                            }
                            tempGrid[row, col + i] = 1;
                        }
                    if (error)
                        break;
                    tempVehicles.Add(vehicleData[0], new Vehicle(row, col, vertical, length));
                }
                if (error)
                    continue;

                // ~~~ Check section 3 ~~~ (solution moves)
                bool solved = false;
                string[] solutionMoves = sections[2].Split(',');
                foreach (string sm in solutionMoves)
                {
                    // validate the format
                    string[] moveData = sm.Trim().Split(' ');
                    int spaces;
                    if (moveData.Length != 2 || !Int32.TryParse(moveData[1], out spaces))
                    {
                        errors.Add(string.Format("Expected solution move of the form '$ I' where $ is a string and I is an integer. File: '{0}', Line: {1}, Move: '{2}'", filePath, line + 1, sm));
                        error = true;
                        break;
                    }

                    // validate vehicle ID
                    if (!tempVehicles.ContainsKey(moveData[0]))
                    {
                        errors.Add(string.Format("Undefined vehicle ID in solution move. File: '{0}', Line: {1}, Move: '{2}'", filePath, line + 1, sm));
                        error = true;
                        break;
                    }

                    // execute/validate the move
                    if (!MoveVehiclePrivate(moveData[0], spaces, tempGrid, tempVehicles, true, out solved))
                    {
                        errors.Add(string.Format("Illegal solution move (ensure previous moves are also correct). File: '{0}', Line: {1}, Move: '{2}'", filePath, line + 1, sm));
                        error = true;
                        break;
                    }
                }
                if (error)
                    continue;

                // make sure the moves solved the config
                if (!solved)
                {
                    errors.Add(string.Format("Solution moves did not solve configuration. File: '{0}', Line: {1}", filePath, line + 1));
                    continue;
                }
            }
        }


        /// <summary>
        /// Sets the grid to the specified configuration. Enter value less than 1 for a random configuration.
        /// </summary>
        /// <param name="config">configuration to set grid to (configs start at 1)</param>
        public void SetConfig(int config)
        {
            if (config > configurations.Length)
                return;

            if (config == CurrentConfig)
                ResetConfig();
            
            // check if random config is desired
            if (config < 1)
            {
                Random rand = new Random();
                config = rand.Next(configurations.Length) + 1;
                while (config == CurrentConfig && configurations.Length > 1) // ensure random config doesn't equal the current one
                    config = rand.Next(configurations.Length) + 1;
            }
            CurrentConfig = config;

            // get the semicolon-delimited sections of the configuration
            string[] sections = configurations[config - 1].Split(';');

            // get the config's difficulty and number of rows and columns
            string[] settings = sections[0].Trim().Split(' ');
            ConfigDifficulty = Int32.Parse(settings[0]);
            int rows = Int32.Parse(settings[1]);
            int columns = Int32.Parse(settings[2]);

            // create the Vehicles and set up the grid
            if (rows != Rows || columns != Columns) // only allocate new grid if dimensions have changed
            {
                grid = new byte[rows, columns];
                Rows = rows;
                Columns = columns;
            }
            else
                Array.Clear(grid, 0, grid.Length);
            string[] vehicleEncodings = sections[1].Split(',');
            vehicles.Clear();
            foreach (string ve in vehicleEncodings)
            {
                // parse the vehicle data
                string[] vehicleData = ve.Trim().Split(' ');
                string id = vehicleData[0];
                int row = Int32.Parse(vehicleData[1]) - 1;
                int col = Int32.Parse(vehicleData[2]) - 1;
                bool vertical = vehicleData[3].Equals("V");
                int length = Int32.Parse(vehicleData[4]);             
                vehicles.Add(id, new Vehicle(row, col, vertical, length));

                // mark the vehicle in the underlying grid
                if (vertical)
                    for (int i = 0; i < length; i++)
                        grid[row + i, col] = 1;
                else
                    for (int i = 0; i < length; i++)
                        grid[row, col + i] = 1;               
            }

            // add each solution move
            solutionMoves.Clear();
            foreach (string solutionMove in sections[2].Split(','))
                solutionMoves.Add(solutionMove.Trim());

            nextSolutionMove = 0;
            userMoveMade = false;
            solutionMoveMade = false;
            Solved = false; // configuration is now set and unsolved
        }


        /// <summary>
        /// Resets the grid to the current configuration.
        /// </summary>
        public void ResetConfig()
        {            
            // only reset if a move has been made
            if (userMoveMade || solutionMoveMade)
            {
                // reset the Vehicle positions and the underlying grid
                Array.Clear(grid, 0, grid.Length);
                string[] sections = configurations[CurrentConfig - 1].Split(';');
                string[] vehicleEncodings = sections[1].Split(',');
                foreach (string ve in vehicleEncodings)
                {
                    // parse the vehicle data
                    string[] vehicleData = ve.Trim().Split(' ');
                    string id = vehicleData[0];
                    int row = Int32.Parse(vehicleData[1]) - 1;
                    int col = Int32.Parse(vehicleData[2]) - 1;
                    Vehicle v = vehicles[id];

                    // reset the Vehicle's position and mark the vehicle in the underlying grid
                    if (v.Vertical)
                    {
                        v.BackRow = row;
                        for (int i = 0; i < v.Length; i++)
                            grid[row + i, col] = 1;
                    }
                    else
                    {
                        v.BackCol = col;
                        for (int i = 0; i < v.Length; i++)
                            grid[row, col + i] = 1;
                    }
                }                
                nextSolutionMove = 0;
                userMoveMade = false;
                solutionMoveMade = false;
                Solved = false;
            }
        }


        /// <summary>
        /// Moves the specified vehicle the specified number of spaces (negative values move vertical
        /// vehicles up and horizontal vehicles left). Check boolean property 'Solved' to see if the
        /// move solved the configuration.
        /// </summary>
        /// <param name="vehicleID">the ID of the Vehicle to move</param>
        /// <param name="spaces">number of spaces to move (negative values move up/left)</param>
        /// <returns>true if the move was successful/legal</returns>
        public bool MoveVehicle(string vehicleID, int spaces)
        {
            bool successful = MoveVehiclePrivate(vehicleID, spaces, grid, vehicles, true, out solved);
            if (successful)
                userMoveMade = true;
            return successful;
        }


        /// <summary>
        /// Moves the specified vehicle the specified number of spaces (negative values move vertical
        /// vehicles up and horizontal vehicles left) in the specified grid using the specified vehicles
        /// Dictionary. Specify true for 'validate' if move validation is desired. Otherwise the move
        /// will be attempted without any checks for legality.
        /// </summary>
        /// <param name="vehicleID">ID of Vehicle to move</param>
        /// <param name="spaces">number of spaces to move (negative values move up/left)</param>
        /// <param name="grid">underlying vehicles grid</param>
        /// <param name="vehicles">Vehicle Dictionary</param>
        /// <param name="validate">true if move validation is desired</param>
        /// <param name="solved">true if move solved the configuration/grid</param>
        /// <returns>true if move was successful/legal</returns>
        private bool MoveVehiclePrivate(string vehicleID, int spaces, byte[,] grid, Dictionary<string, Vehicle> vehicles, bool validate, out bool solved)
        {
            Vehicle v = vehicles[vehicleID]; // get Vehicle being moved
            int rows = grid.GetLength(0);
            int columns = grid.GetLength(1);
            solved = false;

            // Note: the technique used here is to delete/unmark one end of the Vehicle and add/mark
            // one cell ahead of the other end of the Vehicle, one space at a time. In other words,
            // chop off one cell of the Vehicle and add it to the other end for each movement.

            if (v.Vertical)
            {
                // move down
                if (spaces >= 0)
                {
                    if (validate)
                    {
                        if (v.FrontRow + spaces >= rows) // check inbounds
                            return false;
                        for (int i = v.FrontRow + 1; i <= v.FrontRow + spaces; i++) // check spaces below are empty
                            if (grid[i, v.FrontCol] == 1)
                                return false;
                    }
                    for (int i = 0; i < spaces; i++)
                    {
                        grid[v.BackRow++, v.BackCol] = 0; // unmark top-most cell of Vehicle and scoot Vehicle one space down
                        grid[v.FrontRow, v.FrontCol] = 1; // mark the new bottom-most cell of the Vehicle
                    }
                }
                // move up
                else
                {
                    if (validate)
                    {
                        if (v.BackRow + spaces < 0) // check inbounds
                            return false;
                        for (int i = v.BackRow - 1; i >= v.BackRow + spaces; i--) // check spaces above are empty
                            if (grid[i, v.BackCol] == 1)
                                return false;
                    }
                    for (int i = spaces; i < 0; i++)
                    {
                        grid[v.FrontRow, v.FrontCol] = 0; // unmark bottom-most cell of Vehicle
                        grid[--v.BackRow, v.BackCol] = 1; // scoot Vehicle one space up and mark the space                
                    }
                }
            }
            else
            {
                // move right
                if (spaces >= 0)
                {
                    if (validate)
                    {
                        if (v.FrontCol + spaces >= columns)
                            return false;
                        for (int i = v.FrontCol + 1; i <= v.FrontCol + spaces; i++) // check spaces ahead are empty
                            if (grid[v.FrontRow, i] == 1)
                                return false;
                    }
                    for (int i = 0; i < spaces; i++)
                    {
                        grid[v.BackRow, v.BackCol++] = 0; // unmark left-most cell of Vehicle and scoot Vehicle one space right
                        grid[v.FrontRow, v.FrontCol] = 1; // mark the new right-most cell of the Vehicle
                    }
                    if (vehicleID.Equals("X") && v.FrontCol == (columns - 1)) // check for victory
                        solved = true;
                }
                // move left
                else
                {
                    if (validate)
                    {
                        if (v.BackCol + spaces < 0)
                            return false;
                        for (int i = v.BackCol - 1; i >= v.BackCol + spaces; i--) // check spaces behind are empty
                            if (grid[v.BackRow, i] == 1)
                                return false;
                    }
                    for (int i = spaces; i < 0; i++)
                    {
                        grid[v.FrontRow, v.FrontCol] = 0; // unmark right-most cell of Vehicle
                        grid[v.BackRow, --v.BackCol] = 1; // scoot Vehicle one space left and mark the space                       
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Executes the next solution move in the grid. If a user move has been made, the grid must be
        /// set or reset to execute a solution move.
        /// </summary>
        /// <returns>VehicleStruct of moved vehicle if move was successful, null otherwise</returns>
        public VehicleStruct? NextSolutionMove()
        {
            // a solution move can only be executed if the grid has just been set/reset with no user moves made
            if (userMoveMade || nextSolutionMove == solutionMoves.Count)
                return null;

            string[] moveData = solutionMoves[nextSolutionMove++].Trim().Split(' ');
            string vID = moveData[0];
            int spaces = Int32.Parse(moveData[1]);

            MoveVehiclePrivate(vID, spaces, grid, vehicles, false, out solved);
            solutionMoveMade = true;
            Vehicle movedVehicle = vehicles[vID];
            return new VehicleStruct(vID, movedVehicle.BackRow, movedVehicle.BackCol, movedVehicle.Vertical, movedVehicle.Length);
        }


        /// <summary>
        /// Undos the most recent solution move in the grid. If a user move has been made, the grid must be
        /// set or reset to undo a solution move.
        /// </summary>
        /// <returns>VehicleStruct of moved vehicle if move was successful, null otherwise</returns>
        public VehicleStruct? UndoSolutionMove()
        {
            if (userMoveMade || nextSolutionMove == 0)
                return null;

            string[] moveData = solutionMoves[--nextSolutionMove].Trim().Split(' ');
            string vID = moveData[0];
            int spaces = Int32.Parse(moveData[1]) * -1;

            MoveVehiclePrivate(vID, spaces, grid, vehicles, false, out solved);
            solutionMoveMade = true;
            Vehicle movedVehicle = vehicles[vID];
            return new VehicleStruct(vID, movedVehicle.BackRow, movedVehicle.BackCol, movedVehicle.Vertical, movedVehicle.Length);
        }


        /// <summary>
        /// Returns an array of VehicleStructs representing all Vehicles in the grid.
        /// </summary>
        /// <returns>array of all VehicleStructs in the grid</returns>
        public VehicleStruct[] GetVehicleStucts()
        {
            VehicleStruct[] vs = new VehicleStruct[vehicles.Count];
            int i = 0;
            foreach (KeyValuePair<string, Vehicle> kv in vehicles)
            {
                Vehicle v = kv.Value;
                vs[i++] = new VehicleStruct(kv.Key, v.BackRow, v.BackCol, v.Vertical, v.Length);
            }
            return vs;
        }


        public VehicleStruct GetVehicleStuct(string vehicleID)
        {
            Vehicle v = vehicles[vehicleID];
            return new VehicleStruct(vehicleID, v.BackRow, v.BackCol, v.Vertical, v.Length);
        }


        public bool IsCellOpen(int row, int col)
        {
            if (row >= 0 && row < Rows && col >= 0 && col < Columns)
                return grid[row, col] == 0;
            return false;
        }


        public int GetOpenCells(string vehicleID, bool ahead)
        {
            int openCells = 0;
            int nextCell = 1;
            Vehicle v = vehicles[vehicleID];

            if (v.Vertical)
            {
                if (ahead)
                {
                    while (IsCellOpen(v.FrontRow + nextCell, v.FrontCol))
                    {
                        openCells++;
                        nextCell++;
                    }
                }
                else
                {
                    while (IsCellOpen(v.BackRow - nextCell, v.BackCol))
                    {
                        openCells++;
                        nextCell++;
                    }
                }
            }
            else
            {
                if (ahead)
                {
                    while (IsCellOpen(v.FrontRow, v.FrontCol + nextCell))
                    {
                        openCells++;
                        nextCell++;
                    }
                }
                else
                {
                    while (IsCellOpen(v.BackRow, v.BackCol - nextCell))
                    {
                        openCells++;
                        nextCell++;
                    }
                }
            }
            return openCells;
        }

    }


    /// <summary>
    /// Readonly struct which represents a Vehicle object in the underlying grid.
    /// </summary>
    public struct VehicleStruct
    {
        /// <summary>
        /// ID of vehicle
        /// </summary>
        public readonly string id;

        /// <summary>
        /// Top-most row coordinate of vehicle
        /// </summary>
        public readonly int row;

        /// <summary>
        /// Left-most column coordinate of vehicle
        /// </summary>
        public readonly int column;

        /// <summary>
        /// Cell-length of vehicle
        /// </summary>
        public readonly int length;

        /// <summary>
        /// Orientation of vehicle (true if vertical, fasle if horizontal)
        /// </summary>
        public readonly bool vertical;

        /// <summary>
        /// Constructs a new VehicleStruct
        /// </summary>
        /// <param name="id">id of vehicle</param>
        /// <param name="row">top-most row coordinate of vehicle</param>
        /// <param name="column">left-most column coordinate of vehicle</param>
        /// <param name="vertical">orientation of vehicle (true if vertical, fasle if horizontal)</param>
        /// <param name="length">cell-length of vehicle</param>
        public VehicleStruct(string id, int row, int column, bool vertical, int length)
        {
            this.id = id;
            this.row = row;
            this.column = column;
            this.vertical = vertical;
            this.length = length;
        }
    }
}

