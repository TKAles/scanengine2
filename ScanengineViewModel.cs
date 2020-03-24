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

        public string CurrentMLSSerial
        {
            get { return this.MLSStage.SerialNumber; }
            set
            {
                this.MLSStage.SerialNumber = value;
                Console.WriteLine("Property SerialNumber changed to:" + this.MLSStage.SerialNumber);
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
