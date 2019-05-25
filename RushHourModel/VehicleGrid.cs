using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RushHourModel
{
    public class VehicleGrid
    {
        public byte[,] grid; //********************************* SET BACK TO PRIVATE **************************************************
        private string[] configurations;
        private List<string> solutionMoves = new List<string>(64);
        private int nextSolutionMove; // index into list above; keeps track of next solution move

        // BELOW BOOL ISN'T STRICTLY NECESSARY. ILLEGAL MOVES, REGARDLESS OF THE SOURCE, WON'T WORK.
        private bool userMoveMade, solutionMoveMade; 

        public int Rows
        { get; private set; }

        public int Columns
        { get; private set; }

        public int CurrentConfig
        { get; private set; }

        public int ConfigDifficulty
        { get; private set; }

        public bool Solved
        { get; private set; }

        // HOW TO DISALLOW EDITS TO VEHICLES BY THE VIEW? IT SEEMS THAT READONLY ISN'T AN OPTION FOR A USER-DEFINED TYPE SUCH AS Vehicle. AN ALTERNATIVE WOULD BE TO SIMPLY HAVE A PUBLIC DICTIONARY OF VEHICLES THAT IS UPDATED ALONG WITH THE PRIVATE DICTIONARY BELOW. THE PUBLIC LIST NEED NOT CONTAIN Vehicle BUT SIMPLY A NEW TYPE WITH ALL THE NEEDED INFO (ID, VERTICAL, LENGTH, ROW, COLUMN).
        private Dictionary<string, Vehicle> vehicles = new Dictionary<string, Vehicle>(32);
        //public Dictionary<string, VehicleInfo> vehicles = new Dictionary<string, VehicleInfo>(32);


        /// <summary>
        /// Constructs a grid using the specified initialConfig from the specified configurationsFile.
        /// A random configuration from configurationsFile will be selected if initialConfig is negative.
        /// </summary>
        /// <param name="configurationsFilePath">path to text file with grid configurations</param>
        /// <param name="initialConfig">configuration to set grid to (configs start at 1)</param>
        public VehicleGrid(string configurationsFilePath, int initialConfig)
        {
            ValidateConfigurationsFile(configurationsFilePath);
            SetConfig(initialConfig);
        }


        /// <summary>
        /// Validates the specified configurations file and throws exceptions if errors are encountered.
        /// </summary>
        /// <param name="filePath">path to the configurations file</param>
        private void ValidateConfigurationsFile(string filePath)
        {
            configurations = new string[File.ReadLines(filePath).Count()];
            if (configurations.Length == 0)
                throw new FileFormatException("Configurations file cannot be empty. File: '" + filePath + "'");

            int config = 1;
            foreach (string line in File.ReadLines(filePath))
            {
                // check for correct number of semicolon-delimited sections
                string[] sections = line.Split(';');
                if (sections.Length != 3)
                    throw new FileFormatException(string.Format("Expected 2 ';' (found {0}). File: '{1}', Line: {2}",
                        sections.Length - 1, filePath, config));

                // Check section 1 (difficulty, number of rows, number of columns)
                string[] settings = sections[0].Split(' ');
                int diff, rows, cols;
                if (settings.Length != 3 || !Int32.TryParse(settings[0], out diff) || !Int32.TryParse(settings[1], out rows) ||
                    !Int32.TryParse(settings[2], out cols) || diff < 1 || rows < 1 || cols < 1)
                    throw new FileFormatException(string.Format("Expected 3 positive integers. File: '{0}', Line: {1}, Section: '{2}'",
                        filePath, config, sections[0]));

                // Check section 2 (vehicle encodings)
                byte[,] tempGrid = new byte[rows, cols];
                string[] vehicleEncodings = sections[1].Split(',');
                if (vehicleEncodings.Length == 1 && vehicleEncodings[0].Equals(""))
                    throw new FileFormatException(string.Format("One or more vehicle encodings required. File: '{0}', Line: {1}", filePath, config));

                // validate each vehicle encoding (ID, row, col, vertical/horizontal, length)
                foreach (string ve in vehicleEncodings)
                {
                    string[] vehicleData = ve.Trim().Split(' ');
                    int _row, _col, length;
                    if (vehicleData.Length != 5 || !Int32.TryParse(vehicleData[1], out _row) || !Int32.TryParse(vehicleData[2], out _col) ||
                        !Int32.TryParse(vehicleData[4], out length) || (!vehicleData[3].Equals("V") && !vehicleData[3].Equals("H")))
                        throw new FileFormatException(string.Format("Expected vehicle encoding of the form '$ I I (V|H) I' where $ is a string, I is a positive integer, and the fourth element is a V or H. File: '{0}', Line: {1}, Encoding: '{2}'", filePath, config, ve));
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
                }

                // Check section 3 (solution moves)
                string[] solutionMoves = sections[2].Split(',');
                foreach (string sm in solutionMoves)
                {
                    string[] moveData = sm.Trim().Split(' ');
                    int spaces;
                    if (moveData.Length != 2 || !Int32.TryParse(moveData[1], out spaces))
                        throw new FileFormatException(string.Format("Expected solution move of the form '$ I' where $ is a string and I is an integer. File: '{0}', Line: {1}, Move: '{2}'", filePath, config, sm));
                }

                configurations[config++ - 1] = line; // configuration is valid, add to array
            }
        }


        /// <summary>
        /// Sets the grid to the specified configuration. Enter a negative value for a random configuration.
        /// </summary>
        /// <param name="config">configuration to set grid to (configs start at 1)</param>
        public void SetConfig(int config)
        {
            if (config == 0) // INCLUDE THIS FOR RANDOM CONFIG? ***************************************************************************
                return;

            if (config > configurations.Length)
                return;

            if (config == CurrentConfig)
                ResetConfig();
            
            // check if random config is desired
            if (config < 0)
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
            string[] settings = sections[0].Split(' ');
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
                //vehicles.Add(id, new VehicleInfo(row, col, vertical, length));

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
                    //VehicleInfo vi = vehicles[id];

                    // reset the Vehicle's position and mark the vehicle in the underlying grid
                    if (v.Vertical)
                    {
                        v.BackRow = row;
                        //vi.Row = row;
                        for (int i = 0; i < v.Length; i++)
                            grid[row + i, col] = 1;
                    }
                    else
                    {
                        v.BackCol = col;
                        //vi.Column = col;
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
        /// move resulted in a victory.
        /// </summary>
        /// <param name="vehicleID">the ID of the Vehicle to move</param>
        /// <param name="spaces">number of spaces to move (negative values move up/left)</param>
        /// <returns>true if the move was successful/legal</returns>
        public bool MoveVehicle(string vehicleID, int spaces)
        {
            bool successful = MoveVehiclePrivate(vehicleID, spaces);
            if (successful)
                userMoveMade = true;
            return successful;
        }

        // IF I DO DECIDE THAT SOLUTION MOVES AREN'T ALLOWED IF THE USER HAS MADE A MOVE (i.e. userMoveMade == true),
        // THEN THERE IS NO NEED TO CHECK IF THE SOLUTION MOVE IS VALID IN MoveVehicle(). THIS ASSUMES
        // THOUGH, THAT THE SOLUTION MOVES ARE VALID AND ENTERED CORRECTLY. THEREFORE, I SHOULD PROBABLY ONLY DO THIS
        // IF ValidateConfigurationsFile INDEED CHECKS THAT SOLUTIONS ARE CORRECT.
        public VehicleStruct? NextSolutionMove()
        {
            // a solution move can only be executed if the grid has just been set/reset with no user moves made
            if (userMoveMade || nextSolutionMove == solutionMoves.Count)
                return null;

            string[] moveData = solutionMoves[nextSolutionMove++].Trim().Split(' ');
            string vID = moveData[0];
            int spaces = Int32.Parse(moveData[1]);

            if (MoveVehiclePrivate(vID, spaces)) // THIS SHOULD ALWAYS BE SUCCESSFUL IF SOLUTION MOVES ARE VALIDATED
                solutionMoveMade = true;
            Vehicle movedVehicle = vehicles[vID];
            return new VehicleStruct(vID, movedVehicle.BackRow, movedVehicle.BackCol, movedVehicle.Vertical, movedVehicle.Length);            
        }


        public VehicleStruct? UndoSolutionMove()
        {
            if (userMoveMade || nextSolutionMove == 0)
                return null;

            string[] moveData = solutionMoves[--nextSolutionMove].Trim().Split(' ');
            string vID = moveData[0];
            int spaces = Int32.Parse(moveData[1]) * -1;

            if (MoveVehiclePrivate(vID, spaces))
                solutionMoveMade = true;
            Vehicle movedVehicle = vehicles[vID];
            return new VehicleStruct(vID, movedVehicle.BackRow, movedVehicle.BackCol, movedVehicle.Vertical, movedVehicle.Length);
        }


        /// <summary>
        /// Moves the specified vehicle the specified number of spaces (negative values move vertical
        /// vehicles up and horizontal vehicles left).
        /// </summary>
        /// <param name="vehicleID">the ID of the Vehicle to move</param>
        /// <param name="spaces">number of spaces to move (negative values move up/left)</param>
        /// <returns>true if the move was successful/legal</returns>
        private bool MoveVehiclePrivate(string vehicleID, int spaces)
        {
            Vehicle v = vehicles[vehicleID]; // get Vehicle being moved

            // Note: the technique used here is to delete/unmark one end of the Vehicle and add/mark
            // one cell ahead of the other end of the Vehicle, one space at a time. In other words,
            // chop off one cell of the Vehicle and add it to the other end for each movement.

            if (v.Vertical)
            {
                // move down
                if (spaces >= 0)
                {
                    if (v.FrontRow + spaces >= Rows) // check inbounds
                        return false;
                    for (int i = v.FrontRow + 1; i <= v.FrontRow + spaces; i++) // check spaces below are empty
                        if (grid[i, v.FrontCol] == 1)
                            return false;
                    for (int i = 0; i < spaces; i++)
                    {
                        grid[v.BackRow++, v.BackCol] = 0; // unmark top-most cell of Vehicle and scoot Vehicle one space down
                        grid[v.FrontRow, v.FrontCol] = 1; // mark the new bottom-most cell of the Vehicle
                    }
                }
                // move up
                else
                {
                    if (v.BackRow + spaces < 0) // check inbounds
                        return false;
                    for (int i = v.BackRow - 1; i >= v.BackRow + spaces; i--) // check spaces above are empty
                        if (grid[i, v.BackCol] == 1)
                            return false;
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
                    if (v.FrontCol + spaces >= Columns)
                        return false;
                    for (int i = v.FrontCol + 1; i <= v.FrontCol + spaces; i++) // check spaces ahead are empty
                        if (grid[v.FrontRow, i] == 1)
                            return false;
                    for (int i = 0; i < spaces; i++)
                    {
                        grid[v.BackRow, v.BackCol++] = 0; // unmark left-most cell of Vehicle and scoot Vehicle one space right
                        grid[v.FrontRow, v.FrontCol] = 1; // mark the new right-most cell of the Vehicle
                    }
                    if (vehicleID.Equals("X") && v.FrontCol == (Columns - 1)) // check for victory
                        Solved = true;
                }
                // move left
                else
                {
                    if (v.BackCol + spaces < 0)
                        return false;
                    for (int i = v.BackCol - 1; i >= v.BackCol + spaces; i--) // check spaces behind are empty
                        if (grid[v.BackRow, i] == 1)
                            return false;
                    for (int i = spaces; i < 0; i++)
                    {
                        grid[v.FrontRow, v.FrontCol] = 0; // unmark right-most cell of Vehicle
                        grid[v.BackRow, --v.BackCol] = 1; // scoot Vehicle one space left and mark the space                       
                    }
                }
            }
            return true;
        }


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

    }

    public struct VehicleStruct
    {
        public readonly string id;
        public readonly int row, column, length;
        public readonly bool vertical;        

        public VehicleStruct(string id, int row, int column, bool vertical, int length)
        {
            this.id = id;
            this.row = row;
            this.column = column;
            this.vertical = vertical;
            this.length = length;
        }
    }

    //public class VehicleInfo
    //{
    //    public int Row
    //    { get; set; }

    //    public int Column
    //    { get; set; }

    //    public readonly int Length;

    //    public readonly bool Vertical;

    //    public VehicleInfo(int row, int column, bool vertical, int length)
    //    {
    //        Row = row;
    //        Column = column;
    //        Vertical = vertical;
    //        Length = length;
    //    }
    //}
}

