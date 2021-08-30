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
        // TODO: CLASS VehicleGrid.MoveInfo HAS SUFFICIENT INFO FOR A MOVE (COULD REPLACE VehicleStruct BELOW), BUT WOULD HAVE TO MAKE PUBLIC
        public event EventHandler<VehicleStruct?> VehicleMoved;

        public DelegateCommand ConfigEnteredCommand { get; private set; }
        //public DelegateCommand MoveVehicleCommand { get; private set; }
        public DelegateCommand UndoCommand { get; private set; }
        public DelegateCommand RedoCommand { get; private set; }
        

        public RushHourViewModel()
        {
            try
            {
                //VehicleGrid = new VehicleGrid("../../../configurations.txt", _config);
                VehicleGrid = new VehicleGrid("../../../configurations.txt", 1);
                //TotalConfigs = VehicleGrid.TotalConfigs;
                //_difficulty = VehicleGrid.ConfigDifficulty;
                ConfigEnteredCommand = new DelegateCommand(ConfigEntered);
                //MoveVehicleCommand = new DelegateCommand(MoveVehicle);
                UndoCommand = new DelegateCommand(Undo, UndoCanExecute);
                RedoCommand = new DelegateCommand(Redo, RedoCanExecute);
            }
            catch (Exception ex)
            {
                // TODO: HOW TO HANDLE BAD CONFIG FILES?
                MessageBox.Show("ERROR FROM ViewModel: " + ex.Message);
            }
        }

        // TODO: MAKE COMMAND OUT OF THIS? THE ISSUE IS THE PARAMETERS. ONE SOLUTION WOULD BE TO SIMPLY HAVE THEM BE ADDITIONAL
        // PUBLIC PROPERTIES, E.G. SelectedVehicleID AND SpacesToMove. BUT THIS SEEMS MESSIER. ANY REASON TO MAKE THIS A COMMAND?
        public bool MoveVehicle(string vehicleID, int spaces)
        {
            bool moveSuccessful = VehicleGrid.MoveVehicle(vehicleID, spaces);
            //CanUndo = VehicleGrid.CanUndoMove;
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
            return moveSuccessful;
        }



        private void Undo()
        {
            VehicleStruct? movedVehicle = VehicleGrid.UndoMove();
            VehicleMoved.Invoke(this, movedVehicle);
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void Redo()
        {
            VehicleGrid.RedoMove();
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private bool UndoCanExecute()
        {
            return VehicleGrid.CanUndoMove;
        }

        private bool RedoCanExecute()
        {
            return VehicleGrid.CanRedoMove;
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
        //    get { return _config; }
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
                // TODO: SHOULD I BE USING SetProperty()?
                if (value != VehicleGrid.CurrentConfig)
                {
                    VehicleGrid.SetConfig(value);
                    OnPropertyChanged("Difficulty");
                    OnPropertyChanged("TotalMoves");
                    OnPropertyChanged("RequiredSolutionMoves");
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

        public int TotalMoves
        {
            get { return VehicleGrid.TotalMoves; }
        }

        public int RequiredSolutionMoves
        {
            get { return VehicleGrid.RequiredSolutionMoves; }
        }

        #region INotifyPropertyChanged Members

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

        #endregion
    }
}
