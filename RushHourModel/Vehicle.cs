using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushHourModel
{
    class Vehicle
    {
        private int backRow, backCol;
        private readonly bool objectConstructed = false;
        
        /// <summary>
        /// Orientation of vehicle (true if vertical, fasle if horizontal)
        /// </summary>
        public readonly bool Vertical;
        
        /// <summary>
        /// Cell-length of vehicle
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Left/top-most row coordinate of vehicle
        /// </summary>
        public int BackRow          
        {
            get { return backRow; }
            set
            {
                if (!Vertical && objectConstructed)
                    throw new InvalidOperationException("Row cannot change for horizontal Vehicle.");
                if (value < 0)
                    throw new ArgumentException("Invalid argument '" + value + "'. 'BackRow' must be non-negative.");
                backRow = value;
                if (Vertical) // only update row if vehicle is vertical
                    FrontRow = backRow + Length - 1;
            }
        }
        
        /// <summary>
        /// Left/top-most column coordinate of vehicle
        /// </summary>
        public int BackCol
        {
            get { return backCol; }
            set
            {
                if (Vertical && objectConstructed)
                    throw new InvalidOperationException("Column cannot change for vertical Vehicle.");
                if (value < 0)
                    throw new ArgumentException("Invalid argument '" + value + "'. 'BackCol' must be non-negative.");
                backCol = value;
                if (!Vertical) // only update column if vehicle is horizontal
                    FrontCol = backCol + Length - 1;
            }
        }

        /// <summary>
        /// Right/bottom-most row coordinate of vehicle
        /// </summary>
        public int FrontRow
        { get; private set; }

        /// <summary>
        /// Right/bottom-most column coordinate of vehicle
        /// </summary>
        public int FrontCol
        { get; private set; }

        /// <summary>
        /// Constructs a new Vehicle
        /// </summary>
        /// <param name="backRow">left/top-most row coordinate of vehicle</param>
        /// <param name="backCol">left/top-most column coordinate of vehicle</param>
        /// <param name="vertical">orientation of vehicle (true if vertical, fasle if horizontal)</param>
        public Vehicle(int backRow, int backCol, bool vertical, int length)
        {
            if (length < 1)
                throw new ArgumentException("Invalid argument '" + length + "'. 'length' must be positive.");
            
            Length = length;
            Vertical = vertical;
            BackRow = backRow;
            BackCol = backCol;

            // These only need to be set once (column won't change for vertical, row won't change for horizontal)
            if (vertical)
                FrontCol = backCol;
            else
                FrontRow = backRow;
            objectConstructed = true; // row and column can no longer be changed for horizontal and vertical vehicles, respectively
        }
    }
}
