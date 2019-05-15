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
        private readonly string configsFile;
        public byte[,] grid; //********************************* SET BACK TO PRIVATE **************************************************
        private string[] configurations;
        private string[] solutionMoves;

        public int Rows
        { get; private set; }

        public int Columns
        { get; private set; }

        public int ConfigDifficulty
        { get; private set; }

        public bool Solved
        { get; private set; }

        // HOW TO DISALLOW EDITS TO VEHICLES BY THE VIEW? THERE SEEMS TO BE SOME ReadOnly C# THINGS I COULD USE BUT I DON'T
        // KNOW EXACTLY HOW. OR I COULD RETURN A LIST OF STRUCTS THAT CONTAIN ALL THE NEEDED VEHICLE INFO (ID, VERTICAL, LENGTH, ROW, COLUMN).
        //public Dictionary<int, Vehicle> vehicles; // *** SWITCH THIS TO AN ARRAYLIST. IDS WILL DIRECTLY INDEX VEHICLES. ***
        public List<Vehicle> vehicles = new List<Vehicle>(16);


        /// <summary>
        /// Constructs a grid using the specified initialConfig from the specified configurationsFile.
        /// A random configuration from configurationsFile will be selected if initialConfig is negative.
        /// </summary>
        /// <param name="configurationsFile">path to text file with grid configurations</param>
        /// <param name="initialConfig">configuration to set grid to (configs start at 1)</param>
        public VehicleGrid(string configurationsFile, int initialConfig)
        {
            // NEED ERROR HANDLING HERE IF FILE CAN'T BE FOUND ********************************************************************
            configsFile = configurationsFile;
            configurations = new string[File.ReadLines(configurationsFile).Count()];
            int index = 0;
            foreach (string line in File.ReadLines(configurationsFile))
                // VALIDATE ALL CONFIGS UPON LOADING? OTHERWISE A CONFIG IS UNNECESSARILY CHECKED EVERYTIME IT'S SET.
                // ValidateConfig(line);
                configurations[index++] = line;
            
            SetConfig(initialConfig);            
        }


        /// <summary>
        /// Sets the grid to the specified configuration. Enter a negative value for a random configuration.
        /// </summary>
        /// <param name="config">configuration to set grid to (configs start at 1)</param>
        public void SetConfig(int config)
        {
            // check if random config is desired
            if (config < 0)
            {
                Random rand = new Random();
                config = rand.Next(configurations.Length) + 1;
            }

            //string regexString = @"^(\d ){2}\d; \d \d [VH] \d(, \d \d [VH] \d)*$";
            //Regex regex = new Regex(regexString);
            //if (!regex.IsMatch(configurations[config - 1]))
            //    throw new FileFormatException(string.Format("Invalid Format. File: \"{0}\", Line: {1}", configsFile, config));

            // get the main sections of the configuration
            string[] sections = configurations[config - 1].Split(';');
            int elementCount = sections.Length;
            if (elementCount != 2)
                throw new FileFormatException(string.Format("Expected 1 ';' (found {0}). File: '{1}', Line: {2}",
                    elementCount - 1, configsFile, config));

            // Get the config's difficulty and number of rows and columns
            string[] settings = sections[0].Split(' ');
            elementCount = settings.Length;
            int int1, int2, int3;
            if (elementCount != 3 || !Int32.TryParse(settings[0], out int1) || !Int32.TryParse(settings[1], out int2) ||
                !Int32.TryParse(settings[2], out int3) || int1 < 1 || int2 < 1 || int3 < 1)
                throw new FileFormatException(string.Format("Expected 3 positive integers. File: '{0}', Line: {1}, Section: '{2}'",
                    configsFile, config, sections[0]));
            ConfigDifficulty = int1;
            Rows = int2;
            Columns = int3;

            // Create the Vehicles and set up the grid
            grid = new byte[Rows, Columns];
            string[] vehicleEncodings = sections[1].Split(',');
            elementCount = vehicleEncodings.Length;
            if (elementCount == 1 && vehicleEncodings[0].Equals(""))
                throw new FileFormatException(string.Format("One or more vehicle encodings required. File: '{0}', Line: {1}", configsFile, config));
            //vehicles = new Dictionary<int, Vehicle>(vehicleEncodings.Length);
            //int vehicleID = 1;
            foreach(string ve in vehicleEncodings)
            {
                // parse and validate the vehicle data
                string[] vehicleData = ve.Trim().Split(' ');
                if (vehicleData.Length != 4 || !Int32.TryParse(vehicleData[0], out int1) || !Int32.TryParse(vehicleData[1], out int2) ||
                    !Int32.TryParse(vehicleData[3], out int3) || (!vehicleData[2].Equals("V") && !vehicleData[2].Equals("H")))
                    throw new FileFormatException(string.Format("Expected vehicle encoding of the form 'D D (V|H) D' where D is a positive integer and the third element is a V or H. File: '{0}', Line: {1}, Encoding: '{2}'", configsFile, config, ve));
                int row = int1 - 1;
                int col = int2 - 1;
                bool vertical = vehicleData[2].Equals("V");
                int length = int3;
                
                if (row < 0 || col < 0 || length < 1 || (vertical && row + length > Rows) || (!vertical && col + length > Columns))
                    throw new FileFormatException(string.Format("Vehicle position and/or length is invalid or out of range. File: '{0}', Line: {1}, Encoding: '{2}'", configsFile, config, ve));

                // mark/validate the vehicle in the underlying grid
                if (vertical)
                    for (int i = 0; i < length; i++)
                    {
                        if (grid[row + i, col] == 1)
                            throw new FileFormatException(string.Format("Vehicle overlap. File: '{0}', Line: {1}, Encoding: '{2}'",
                                configsFile, config, ve));
                        grid[row + i, col] = 1;
                    }
                else
                    for (int i = 0; i < length; i++)
                    {
                        if (grid[row, col + i] == 1)
                            throw new FileFormatException(string.Format("Vehicle overlap. File: '{0}', Line: {1}, Encoding: '{2}'",
                                configsFile, config, ve));
                        grid[row, col + i] = 1;
                    }                        

                // add the vehicle to the Vehicle Dictionary
                //vehicles.Add(vehicleID++, new Vehicle(row, col, vertical, length));
                vehicles.Add(new Vehicle(row, col, vertical, length));
                Solved = false; // configuration is now set and unsolved
            }
        }


        /// <summary>
        /// Moves the specified vehicle the specified number of spaces (negative values move vertical
        /// vehicles up and horizontal vehicles left).
        /// </summary>
        /// <param name="vehicleID">the ID of the Vehicle to move</param>
        /// <param name="spaces">number of spaces to move (negative values move up/left)</param>
        /// <returns>true if the move was successful/legal</returns>
        public bool MoveVehicle(int vehicleID, int spaces)
        {
            Vehicle v = vehicles[vehicleID - 1]; // get Vehicle being moved

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
                    if (vehicleID == 1 && v.FrontCol == (Columns - 1)) // check for victory
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
        
        
    }
}
