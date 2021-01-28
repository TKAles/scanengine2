using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace wpfscanengine
{
    class MotionStage : INotifyPropertyChanged
    {
        /* MotionStage is a class for the THORLabs 
         * MLS203-1 XY Scanning Stage. 
         */
        private string _sn;
        private double _currentx;
        private double _currenty;
        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentSerial
        {
            get { return _sn; }
            set
            {
                _sn = value;
                OnPropertyChanged();
            }
        }

        public double CurrentX
        {
            get { return _currentx; }
            set
            {
                _currentx = value;
                OnPropertyChanged();
            }
        }

        public double CurrentY
        { 
            get { return _currenty; }
            set
            {
                _currenty = value;
                OnPropertyChanged();
            }
        }
        
        public MotionStage(string _serialno)
        {
            CurrentSerial = _serialno;

        }

        public void ConnectStage()
        {

        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
