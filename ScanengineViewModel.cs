using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
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

        public MSOIVI TekScope;
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

        private decimal _xo = 0.000m;
        private decimal _yo = 0.000m;
        private decimal _xd = 0.000m;
        private decimal _yd = 0.000m;

        private decimal _xc = 0.000m;
        private decimal _yc = 0.000m;

        private decimal _mtx = 0.000m;
        private decimal _mty = 0.000m;

        private decimal _heliosfreq = 20000.0m;
        // Accessors for the coords
        public decimal XOrigin
        {
            get { return _xo; }
            set { _xo = value; }
        }
        public decimal YOrigin
        {
            get { return _yo; }
            set { _yo = value; }

        }
        public decimal XDelta
        {
            get { return _xd; }
            set { _xd = value; }
        }
        public decimal YDelta
        {
            get { return _yd; }
            set { _yd = value; }
        }
        public decimal XCurrent
        {
            get { return _xc; }
        }
        public decimal YCurrent
        {
            get { return _yc; }
        }
        public decimal XMoveCoord
        {
            get { return _mtx; }
            set { _mtx = value; }
        }
        public decimal YMoveCoord
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
        public bool IsReady
        {
            get { return this.MLSStage.IsXHomed && this.MLSStage.IsYHomed; }
        }
        public bool IsMSOConnected
        { 
            get { return this.TekScope.IsConnected; }
        }

        // Scan strategy information
        private int _linesneeded = 0;
        public int LinesNeeded
        {
            get { return _linesneeded; }
        }

        private decimal _scanpitch = 0.00m;
        private decimal _scanvelocity = 0.00m;
        private int _pointsneeded = 0;

        public decimal ScanPitch
        {
            get { return _scanpitch; }
            set { _scanpitch = value; }

        }

        public decimal ScanVelocity
        { 
            get { return _scanvelocity; }
            set { _scanvelocity = value;
                Console.WriteLine("Scan Velocity set to " + value + " mm/s");
            }
        }

        public int PointsNeeded
        {
            get { return _pointsneeded;  }
        }

        private bool _pollingactive = false;

        public ScanengineViewModel(string _ser = "73000001", string _mso = "not set!")
        {
            this.MLSStage = new MLS203Stage();
            this.TekScope = new MSOIVI();
            this.CurrentMLSSerial = _ser;
            this.CurrentMSOAddress = this.TekScope.ResourceAddress;

            // Subscribe to the required properties
            Task _positionPolling = Task.Run(() =>
            {
                this.StartEncoderPolling();
            });
        }

        public void ConnectMLSStage()
        {
            if(MLSStage.IsConnected == false)
            {
                // Connect to the stage.
                MLSStage.ConnectStage();   
            }
        }

        public void DisconnectMLSStage()
        {
            if(MLSStage.IsConnected == true)
            {
                // Disconnect the stage.
                MLSStage.DisconnectStage();
            }
        }

        public void HomeStage()
        {
            // Home the stage if connected.
            if(MLSStage.IsConnected == true)
            {
                MLSStage.HomeStage();
            }
        }

        public void MoveToPosition()
        {
            MLSStage.MoveStageTo(this.XMoveCoord, this.YMoveCoord);
        }

        public void MoveSingle(int _axis, decimal _coord)
        {
            MLSStage.MoveSingle(_axis, _coord);
        }

        public void StartEncoderPolling()
        {
            _pollingactive = true;
            while(this._pollingactive == true)
            {
                if(this.MLSStage.IsConnected)
                { 
                    _xc = this.MLSStage.XPosition;
                    _yc = this.MLSStage.YPosition;
                    Thread.Sleep(100);
                }
            }
        }

        public void CalculateScanStrategy()
        {
            if(_yd != 0)
            {
                _pointsneeded = Decimal.ToInt32((_yd / (_scanvelocity / _heliosfreq))) + 1;
            } else
            {
                _pointsneeded = 0;
            }
            _linesneeded = (int)(_xd / _scanpitch);
            return;
        }
        
    }
}
