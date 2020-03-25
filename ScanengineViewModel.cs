using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.Benchtop.BrushlessMotorCLI;

namespace wpfscanengine
{
    public partial class ScanengineViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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

        private decimal _xo = 0.000m;
        private decimal _yo = 0.000m;
        private decimal _xd = 0.000m;
        private decimal _yd = 0.000m;

        private decimal _xc = 0.000m;
        private decimal _yc = 0.000m;

        private decimal _mtx = 0.000m;
        private decimal _mty = 0.000m;

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
            set { _xc = value; }
        }
        public decimal YCurrent
        {
            get { return _yc; }
            set { _yc = value; }
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

        public ScanengineViewModel(string _ser = "73000001", string _mso = "not set!")
        {
            this.MLSStage = new MLS203Stage();
            this.CurrentMLSSerial = _ser;
            this.CurrentMSOAddress = _mso;

            // Subscribe to the required properties


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
                MLSStage.DisconnectStage();
            }
        }

        public void HomeStage()
        {
            if(MLSStage.IsConnected == true)
            {
                MLSStage.HomeStage();
            }
        }

        public void MoveToPosition()
        {
            MLSStage.MoveStageTo(this.XMoveCoord, this.YMoveCoord);
        }

    }
}
