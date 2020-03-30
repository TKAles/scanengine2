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
        private TekSeriesScope _tekdriver;
        private string _addr;
        public string ResourceAddress
        {
            get { return _addr; }
            set { _addr = value; }
        }

        private double _triglvl;
        public double TriggerLevel
        {
            get { return _triglvl; }
            set { _triglvl = value; }

        }

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

            // Turn on the AFG and simulate a 20kHz laser pulse signal.
            this.SimulateHelios();

        }

        public void SimulateHelios()
        {
            string _afgenable = "AFG:FUNC PULS;:AFG:HIGHL 1.0;:AFG:LOWL 0.0;:AFG:PULS:WID 5.0E-6;";
            _afgenable += ":AFG:OUTP:STATE 1;";
            this._tekdriver.System.WriteString(_afgenable);
        }
        public void DisconnectMSO()
        {
            this._isConnected = false;
            _tekdriver.Close();
            return;
        }
        /*
         * 
         * OLD CODE FOR REFERENCE ONLY....
        public void ConnectMSO()
        {
            if(!IsConnected)
            {
                try
                {
                    MSOConnection.Open(this.ResourceAddress);
                    _isConnected = true;
                    this.InitalizeOscilloscope();
                    this.SetupOscilloscope();
                    this.ConfigureSRASTrigger();

                } catch (Exception e)
                {
                    Console.WriteLine("An exception occurred. Exception: " + e.Data);
                }
            }
        }

        public void DisconnectMSO()
        {
            if(IsConnected)
            {
                this.MSOConnection.Close();
                this._isConnected = false;
            }
        }

        public void InitalizeOscilloscope()
        {
            Console.WriteLine("Initalizing Oscillscope...");
            String _clearcommand = "*RST;:DIS:GLO:CH1:STATE 1;:DIS:GLO:CH2 STATE 1;:DIS:GLO:CH3 STATE1;:DIS:GLO:CH4: STATE 1";
            String _labelcommand = ":CH1:LAB:NAM \"RF\";:CH2:LAB:NAM \"Optotrig\";:CH3:LAB:NAM \"MLSSignal\";:CH4:LAB:NAM \"DC\"";
            this.MSOConnection.Write(_clearcommand);
            Thread.Sleep(50);
            this.MSOConnection.Write(_labelcommand);
        }
        
        public void SetupOscilloscope()
        {
            String _terminationCommand = ":CH1:TER 50;:CH2:TER 1000000";
            String _zoomCommand = ":CH1:SCA 20.0E-3;:CH2:SCA 1.0;:CH3:SCA 500.0E-3;:CH4:SCA 100.0E-3";
            this.MSOConnection.Write(_terminationCommand);
            Thread.Sleep(100);
            this.MSOConnection.Write(_zoomCommand);
            Thread.Sleep(100);
        }

        public void ConfigureSRASTrigger()
        {
            String _triggerconfig = "TRIGGER:A:TYPE LOGIC;:TRIGGER:A:LOGIC:FUNCTION AND;";
            _triggerconfig += ":TRIGGER:A:LOGICPATTERN:CH1 X;:TRIGGER:A:LOGICPATTERN:CH2 HIGH;";
            _triggerconfig += ":TRIGGER:A:LOGICPATTERN:CH3 HIGH;:TRIGGER:A:LOGICPATTERN:CH4 X;";
            _triggerconfig += ":TRIGGER:A:LEVEL:CH2 " + this._triglvl.ToString() + ";:TRIGGER:A:LEVEL:CH3 2.0;:TRIGGER:A:MODE NORMAL";

            this.MSOConnection.Write(_triggerconfig);
            Thread.Sleep(100);
        }

        public void SetupAcquisition(int record_length, int frames_required)
        {
            string _ffString = "HORIZONTAL:MODE:RECORDLENGTH " + record_length.ToString();
            _ffString += ";:HORIZONTAL:FASTFRAME:COUNT " + frames_required.ToString();
            _ffString += ";:HORIZONTAL:FASTFRAME:STATE ON;:HORIZONTAL:FASTFRAME:SUMFRAME NONE;";

            this.MSOConnection.Write(_ffString);
            return;
        }
        */

    }
}
