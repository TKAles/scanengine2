using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa;
using Ivi.Driver.Interop;
using Ivi.Scope;
using Tektronix.TekSeriesScope.Interop;


namespace wpfscanengine
{
    public class MSOIVI
    {
        public TekSeriesScope _tekdriver;
        private string _addr;
        public string ResourceAddress
        {
            get { return _addr; }
            set { _addr = value; }
        }

        private string _triglvl = "250E-3";

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
        }

        // _horiztimescale - oscilloscope timescale in [seconds]
        // stored as a string to make sending to the scope easier
        private string _horiztimescale = "20.0E-9";
        private string[] _vscale = { "100.0E-3", "750.0E-3", "500.0E-3", "50.0E-3" };
        
        public MSOIVI(string instaddress = "")
        {
            // If instraddress is "" then use the default MSO 5
            // via USB3 address.
            _isConnected = false;
            this.ResourceAddress = "USB0::0x0699::0x0522::C013000::INSTR";
            _tekdriver = new TekSeriesScope();
            // Setup a ResourceManager to connect to the scope
            // this.MSOConnection = new VISA();

            // Channel Objects
        }

   

        public void ConnectMSO()
        {
            try
            {
            _tekdriver.Initialize(this.ResourceAddress, true, true, "Model=MSO54");
            this._isConnected = true;
            this.ConfigureChannels();
            } catch (Exception ex)
            {
                return;
            }
        }

        public void ConfigureChannels()
        {
            // Configuration using SCPI commands
            // Reset instrument. Turn on display of all four channels (No TLP058)
            string _channeldisplay = "*RST;:DIS:GLO:CH1:STATE 1;:DIS:GLO:CH2:STATE 1;:DIS:GLO:CH3:STATE 1;:DIS:GLO:CH4:STATE 1";
            this._tekdriver.System.WriteString(_channeldisplay);

            // Label Channels
            string _ch1Label = "SAW Signal";
            string _ch2Label = "IR Phototrigger";
            string _ch3Label = "Stage V Trigger";
            string _ch4Label = "DC Levels";

            // Build the SCPI string for setting the labels for the oscillscope channels. (No TLP058)
            string _channellabels = ":CH1:LAB:NAM \"" + _ch1Label + "\";:CH2:LAB:NAM \"" +
                _ch2Label + "\";:CH3:LAB:NAM \"" + _ch3Label + "\";:CH4:LAB:NAM \"" + _ch4Label + "\"";
            this._tekdriver.System.WriteString(_channellabels);

            // Set the vertical scale levels
            string _vscalecmd = ":CH1:SCA " + _vscale[0] + ";:CH2:SCA " + _vscale[1] +
                ";:CH3:SCA " + _vscale[2] + ";:CH4:SCA " + _vscale[3] + ";";
            this._tekdriver.System.WriteString(_vscalecmd);

            // Set the horizontal timescale and offset
            string _hscalecmd = ":HOR:POS 5;:HOR:SCA " + _horiztimescale;
            this._tekdriver.System.WriteString(_hscalecmd);

            // Uncomment to turn on the AFG and simulate a 20kHz laser pulse signal.
            // this.SimulateHelios();
            this.ConfigureTriggering(false);

        }

        public void ConfigureTriggering(bool _sim=false)
        {
            string _triggercmd;
            if(_sim)
            {
                // simulation mode uses the 20khz pulse on the AFG to create dummy events for the fastframe.
                _triggercmd = "TRIG:A:TYP EDGE;:TRIG:A:EDGE:SOU CH2;:TRIG:A:LEV:CH2 " + _triglvl + ";";
            } else
            {
                // actual acquisition mode: Logic Trigger CH2&CH3 HIGH.
                _triggercmd = "TRIG:A:TYP LOGI;:TRIG:A:LOGI:FUNC AND;:TRIG:A:LOGICP:CH1 X;:TRIG:A:LOGICP:CH2 HIGH;";
                _triggercmd += ":TRIG:A:LOGICP:CH3 HIGH;:TRIG:A:LOGICP:CH4 X;";
                _triggercmd += ":TRIG:A:LEV:CH2 " + _triglvl + ";:TRIG:A:LEV:CH3 1.0;";
            }
            this._tekdriver.System.WriteString(_triggercmd);
        }

        public void SimulateHelios()
        {
            string _afgenable = "AFG:FUNC PULS;:AFG:HIGHL 1.0;:AFG:LOWL 0.0;:AFG:PULS:WID 5.0E-6;:AFG:FREQ 20000;";
            _afgenable += ":AFG:OUTP:STATE 1;";
            this._tekdriver.System.WriteString(_afgenable);
        }

        public void StopHeliosSim()
        {
            this._tekdriver.System.WriteString(":AFG:OUTP:STATE 0");
        }

        public void ConfigureFastFrameAcq(int _numpts = 0)
        {
            // Command to setup the fastframe sequence for a given number of frames with no summary frame
            string _fastframecmd = "ACQ:STATE 0;:HOR:FAST:STATE 1;:HOR:FAST:COUN " 
                + _numpts.ToString() + ";:HOR:FAST:SUMF:STATE 0;";
            Console.WriteLine("FastFrame CMD: " + _fastframecmd);
            this._tekdriver.System.WriteString(_fastframecmd);
        }

        public void SaveWFMFileToSSD(int _filenumber, string _path="C:\\TestScan\\")
        {
            string _rfbase = _path + "RF-";
            string _dcbase = _path + "DC-";
            string _filefmt = "00000";
            string _rfsavecmd = "SAV:WAVE CH1, \"";
            string _dcsavecmd = "SAV:WAVE CH4, \"";
            _rfbase += _filenumber.ToString(_filefmt) + ".wfm";
            _dcbase += _filenumber.ToString(_filefmt) + ".wfm";
            _rfsavecmd += _rfbase + "\";";
            _dcsavecmd += _dcbase + "\";";
            Console.WriteLine("DCSave: " + _dcsavecmd + "\nRFSave: " + _rfsavecmd);
            this._tekdriver.System.WriteString(_rfsavecmd + ";");
            //this._tekdriver.System.ReadString();
            this._tekdriver.System.WriteString(_dcsavecmd + ";");
            //this._tekdriver.System.ReadString();

        }
        public void DisconnectMSO()
        {
            this._isConnected = false;
            // If helios AFG mock is running, uncomment to stop on disconnect
            // doesn't do much besides save some wear and tear on the MSO
            // this.StopHeliosSim();
            _tekdriver.Close();
            return;
        }
    }
}
