using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace wpfscanengine
{
    public partial class ScanengineViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentSerial;
        public string CurrentMLSSerial
        {
            get { return this._currentSerial; }
            set
            {
                if (this.PropertyChanged != null)
                {
                    _currentSerial = value;

                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentMLSSerial"));
                }
            }
        }

        MLS203Stage MLSStage;


        public ScanengineViewModel(string _ser = "73000001")
        {
            this.MLSStage = new MLS203Stage();
            this.CurrentMLSSerial = _ser;

        }
    }
}
