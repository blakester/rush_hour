using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushHourModel
{
    public class Vehicle
    {
        private int _backRow, _backCol;
        private readonly bool _objectConstructed = false;
        
        /// <summary>
        /// Orientation of vehicle (true if vertical, fasle if horizontal)
        /// </summary>
        public readonly bool Vertical;
        
        /// <summary>
        /// Cell-length of vehicle
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Top-most row coordinate of vehicle
        /// </summary>
        public int BackRow          
        {
            get { return _backRow; }
            set
            {
                if (!Vertical && _objectConstructed)
                    throw new InvalidOperationException("Row cannot change for horizontal Vehicle.");
                if (value < 0)
                    throw new ArgumentException("Invalid argument '" + value + "'. 'BackRow' must be non-negative.");
                _backRow = value;
                if (Vertical) // only update row if vehicle is vertical
                    FrontRow = _backRow + Length - 1;
            }
        }
        
        /// <summary>
        /// Left-most column coordinate of vehicle
        /// </summary>
        public int BackCol
        {
            get { return _backCol; }
            set
            {
                if (Vertical && _objectConstructed)
                    throw new InvalidOperationException("Column cannot change for vertical Vehicle.");
                if (value < 0)
                    throw new ArgumentException("Invalid argument '" + value + "'. 'BackCol' must be non-negative.");
                _backCol = value;
                if (!Vertical) // only update column if vehicle is horizontal
                    FrontCol = _backCol + Length - 1;
            }
        }

        /// <summary>
        /// Bottom-most row coordinate of vehicle
        /// </summary>
        public int FrontRow
        { get; private set; }

        /// <summary>
        /// Right-most column coordinate of vehicle
        /// </summary>
        public int FrontCol
        { get; private set; }

        /// <summary>
        /// Constructs a new Vehicle
        /// </summary>
        /// <param name="_backRow">top-most row coordinate of vehicle</param>
        /// <param name="_backCol">left-most column coordinate of vehicle</param>
        /// <param name="vertical">orientation of vehicle (true if vertical, fasle if horizontal)</param>
        /// <param name="length">cell-length of vehicle</param>
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
            _objectConstructed = true; // row and column can no longer be changed for horizontal and vertical vehicles, respectively
        }
    }
}
