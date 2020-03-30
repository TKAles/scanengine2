using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;
using System.Windows;


public class SAWCaptureAgent
{
    private string _instrumentAddress { get; set; }
    private bool _isConnected { get; set; }

    public ResourceManager SessionResourceManager = null;
    public MessageBasedSession MBSession = null;

    public SAWCaptureAgent(string instAddr = "")
    {
        if (instAddr == "")
        {
            this._instrumentAddress = "USB0::0x0699::0x0522::C013000::INSTR";
            //this._instrumentAddress = "TCPIP0::192.168.0.5::4000:SOCKET";
            
            //this._instrumentAddress = "TCPIP0::192.168.0.5::inst0::INSTR";
        }
        // Immediately attempt to connect to the resource and establish a messagebasedsession with it
        this.SessionResourceManager = new ResourceManager();
        try
        {
            this.MBSession = (MessageBasedSession)this.SessionResourceManager.Open(this._instrumentAddress);
        }
        catch (Exception ex)
        {
            MessageBox.Show("An Error Occurred. Exception" + ex.Data, "Error Opening OScope Connection!");
        }
        this.ResetAllChannels();
        this.EnableSRASTriggerMimic();
        this.ConfigureTriggerLogical();

    }

    public bool ResetAllChannels()
    {
        // Needed to assert END low for the Tek scopes.
        MBSession.TimeoutMilliseconds = 2000;
        MBSession.SendEndEnabled = true;
        // Reset the oscilloscope, turn on all the channnels
        string _clearCommand = "*RST;:DIS:GLO:CH1:STATE 1;:DIS:GLO:CH2:STATE 1;:DIS:GLO:CH3:STATE 1;:DIS:GLO:CH4:STATE 1";
        MBSession.FormattedIO.WriteLine(_clearCommand);

        // Label the channels
        string _annotationCommand = ":CH1:LAB:NAM \"RF\";:CH2:LAB:NAM \"Optotrigger\";:CH3:LAB:NAM \"Stage Signal\";:CH4:LAB:NAM \"DC\"";
        MBSession.FormattedIO.WriteLine(_annotationCommand);

        // Set the terminations.
        string _offsetCommand = ":CH1:TER 50;:SCA 0.2E-3;:CH2:TER 1000000";
        MBSession.FormattedIO.WriteLine(_offsetCommand);

        // Set the scales
        string _scaleCommand = "CH1:SCA 20.0E-3;:CH2:SCA 1.0;:CH3:SCA 500.0E-3;:CH4:SCA 100.0E-3;";
        MBSession.FormattedIO.WriteLine(_scaleCommand);
        return true;
    }

    public bool EnableSRASTriggerMimic()
    {
        // Setup AFG to send a 5 microsecond pulse @ 20kHz.
        string _AFGCommand = "AFG:FUNC PULS;:AFG:PUL:WID 5.0E-6;:AFG:FREQ 20000.0;:AFG:HIGHL 1.0;";
        _AFGCommand = _AFGCommand + ":AFG:LOWL 0.0;:AFG:OUTP:LOA:IMPED HIGHZ;:AFG:OUTP:STATE 1";
        MBSession.SendEndEnabled = true;
        MBSession.FormattedIO.WriteLine(_AFGCommand);
        return true;
    }

    public bool ConfigureTriggerLogical()
    {
        string _triggerConfig = "TRIGGER:A:TYPE LOGIC;:TRIGGER:A:LOGIC:FUNCTION AND;";
        _triggerConfig = _triggerConfig + ":TRIGGER:A:LOGICPATTERN:CH1 X;:TRIGGER:A:LOGICPATTERN:CH2 HIGH;";
        _triggerConfig = _triggerConfig + ":TRIGGER:A:LOGICPATTERN:CH3 HIGH;:TRIGGER:A:LOGICPATTERN:CH4 X;";
        _triggerConfig = _triggerConfig + ":TRIGGER:A:LEVEL:CH2 0.750;:TRIGGER:A:LEVEL:CH3 2.0;:TRIGGER:A:MODE NORMAL";
        MBSession.SendEndEnabled = true;
        MBSession.FormattedIO.WriteLine(_triggerConfig);
        MBSession.FormattedIO.WriteLine("MEASU:ADDMEAS FREQUENCY;:MEASU:MEAS1:SOU CH1;:MEASU:ADDMEAS MEAN;:MEASU:MEAS2:SOU CH3;:MEASU:ADDMEAS MEAN;:MEASU:MEAS3:SOU CH4");
        MBSession.FormattedIO.WriteLine("MEASU:GAT CURS");

        return true;
    }

    public bool SetupFastFrame(int _recordLength, int _requiredFrames)
    {
        string _ffString = "HORIZONTAL:MODE:RECORDLENGTH " + _recordLength.ToString();
        _ffString = _ffString + ";:HORIZONTAL:FASTFRAME:COUNT " + _requiredFrames.ToString();
        _ffString = _ffString + ";:HORIZONTAL:FASTFRAME:STATE ON;:HORIZONTAL:FASTFRAME:SUMFRAME NONE;";

        MBSession.SendEndEnabled = true;
        MBSession.FormattedIO.WriteLine(_ffString);

        return true;
    }
};