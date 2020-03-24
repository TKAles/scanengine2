using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.Benchtop.BrushlessMotorCLI;

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
        // Oscilloscope VXi Address
        public string CurrentMSOAddress
        {
            get { return this._msoaddr; }
            set { this._msoaddr = value; }
        }
        // Coordinates in MM
        // o - Origin
        // d - Delta
        // c - Current
        // mt - Move To

        private double _xo = 0.000;
        private double _yo = 0.000;
        private double _xd = 0.000;
        private double _yd = 0.000;

        private double _xc = 0.000;
        private double _yc = 0.000;

        private double _mtx = 0.000;
        private double _mty = 0.000;

        // Accessors for the coords
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
        public double XCurrent
        {
            get { return _xc; }
            set { _xc = value; }
        }
        public double YCurrent
        {
            get { return _yc; }
            set { _yc = value; }
        }
        public double XMoveCoord
        {
            get { return _mtx; }
            set { _mtx = value; }
        }
        public double YMoveCoord
        {
            get { return _mty; }
            set { _mty = value; }

        }

        // Booleans for the various flags at the bottom of the
        // user interface.
        public bool IsMLSConnected
        {
            get { return MLSStage.IsConnected; }
        }

        public bool IsMLSXHomed
        {
            get { return MLSStage.IsXHomed; }
        }

        public bool IsMLSYHomed
        {
            get { return MLSStage.IsYHomed; }
        }
        public bool IsMLSHomed
        {
            get { return MLSStage.IsHomed; }
        }

        public ScanengineViewModel(string _ser = "73000001", string _mso = "not set!")
        {
            this.MLSStage = new MLS203Stage();
            this.CurrentMLSSerial = _ser;
            this.CurrentMSOAddress = _mso;

        }

        public void ConnectMLSStage()
        {
            if(MLSStage.IsConnected == false)
            {
                // Connect to the stage.
                MLSStage.ConnectStage();
            }
        }

    }
}
