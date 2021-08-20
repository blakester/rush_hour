using RushHourModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RushHourView
{
    public class RushHourViewModel : INotifyPropertyChanged
    {
        public ICommand ConfigEnteredCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }

        public RushHourViewModel()
        {
            try
            {
                //VehicleGrid = new VehicleGrid("../../../configurations.txt", _config);
                VehicleGrid = new VehicleGrid("../../../configurations.txt", 10);
                //TotalConfigs = VehicleGrid.TotalConfigs;
                //_difficulty = VehicleGrid.ConfigDifficulty;
                ConfigEnteredCommand = new RelayCommand(x => ConfigEntered(x));
                UndoCommand = new RelayCommand(Undo, UndoCanExecute);
            }
            catch (Exception ex)
            {
                // TODO: HOW TO HANDLE BAD CONFIG FILES?
                MessageBox.Show("ERROR FROM ViewModel: " + ex.Message);
            }
        }

        private void Undo()
        {
            VehicleGrid.UndoMove();
            // RAISE UndoCanExecuteChanged?
        }

        private bool UndoCanExecute()
        {
            return VehicleGrid.CanUndoMove;
        }

        // THIS IS FOR EXPERIMENTATION. THERE'S PROBABLY A BETTER WAY TO HANDLE ENTERING A CONFIG.
        private void ConfigEntered(object param)
        {
            // ATTEMPT 1
            //if (Keyboard.IsKeyDown(Key.Enter))
            //{
            //    OnPropertyChanged("Config");
            //}
            // END ATTEMPT 1

            // ATTEMPT 2 - TODO: THIS WORKS, BUT HOW TO SET GAME GRID IN VIEW? SEEMS THAT REALLY HAS TO BE DONE IN THE CODE BEHIND.
            // MAYBE VIEWMODEL FIRES SOME EVENT THAT THE CONFIG CHANGED AND THE VIEW UPDATES ACCORDINGLY?
            if (param is Xceed.Wpf.Toolkit.IntegerUpDown)
            {
                Xceed.Wpf.Toolkit.IntegerUpDown upDownBox = (Xceed.Wpf.Toolkit.IntegerUpDown)param;
                Config = Int32.Parse(upDownBox.Text);
            }
            // END ATTEMPT 2           
        }

        public VehicleGrid VehicleGrid { get; private set; }

        //public int TotalConfigs
        //{
        //    get;
        //    private set;
        //}

        public int TotalConfigs
        {
            get { return VehicleGrid.TotalConfigs; }
        }

        //public int Config
        //{
        //    get { return _config; VehicleGrid.}
        //    set 
        //    { 
        //        if (SetProperty(ref _config, value))
        //        {
        //            Difficulty = VehicleGrid.ConfigDifficulty;
        //        }
        //    }
        //}

        public int Config
        {
            get { return VehicleGrid.CurrentConfig; }
            set 
            {
                if (value != VehicleGrid.CurrentConfig)
                {
                    VehicleGrid.SetConfig(value);
                    OnPropertyChanged("Difficulty");
                }
            }
        }

        //public int Difficulty
        //{
        //    get { return _difficulty; }
        //    private set { SetProperty(ref _difficulty, value); }
        //}

        public int Difficulty
        {
            get { return VehicleGrid.ConfigDifficulty; }
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
