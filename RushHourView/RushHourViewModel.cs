using RushHourModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RushHourView
{
    public class RushHourViewModel : INotifyPropertyChanged
    {
        
        private int _config = 1;
        private int _difficulty;

        public RushHourViewModel()
        {
            try
            {
                VehicleGrid = new VehicleGrid("../../../configurations.txt", _config);
                TotalConfigs = VehicleGrid.TotalConfigs;
                _difficulty = VehicleGrid.ConfigDifficulty;
            }
            catch (Exception ex)
            {
                // TODO: HOW TO HANDLE BAD CONFIG FILES?
                MessageBox.Show("ERROR FROM ViewModel: " + ex.Message);
            }
        }

        public VehicleGrid VehicleGrid { get; private set; }

        public int TotalConfigs
        {
            get;
            private set;
        }

        public int Config
        {
            get { return _config; }
            set 
            { 
                if (SetProperty(ref _config, value))
                {
                    Difficulty = VehicleGrid.ConfigDifficulty;
                }
            }
        }

        public int Difficulty
        {
            get { return _difficulty; }
            private set { SetProperty(ref _difficulty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetProperty<T>(ref T property, T newValue, [CallerMemberName]string propertyName = null)
        {
            if (!property.Equals(newValue))
            {
                property = newValue;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        private void OnPropertyChanged([CallerMemberName]string name = null)
        {
           if (PropertyChanged != null)
           {
               PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
           }            
        }
    }
}
