using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace wpfscanengine
{
    public partial class ScanengineViewModel 
    {
        public string CurrentMLSSerial
        {
            get { return this.MLSStage.SerialNumber; }
            set
            { this.MLSStage.SerialNumber = value; }
        }
        MLS203Stage MLSStage;

        private string _msoaddr;
        public string CurrentMSOAddress
        {
            get { return this._msoaddr; }
            set { this._msoaddr = value; }
        }

        private double _xo = 0.000f;
        private double _yo = 0.000f;
        private double _xd = 0.000f;
        private double _yd = 0.000f;

        public double XOrigin
        {
            get { return _xo; }
            set { _xo = value; }
        }
        public double YOrigin
        {
            get { return _yo; }
            set { _yo = value; }

        }
        public double XDelta
        {
            get { return _xd; }
            set { _xd = value; }
        }
        public double YDelta
        {
            get { return _yd; }
            set { _yd = value; }
        }

        public ScanengineViewModel(string _ser = "73000001", string _mso = "not set!")
        {
            this.MLSStage = new MLS203Stage();
            this.CurrentMLSSerial = _ser;
            this.CurrentMSOAddress = _mso;

        }

    }
}
