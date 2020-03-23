using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.Benchtop.BrushlessMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;


namespace WPFUnifiedControllerPlatform
{
    public class MLSStageAgent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Stage Status Flags
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (this.PropertyChanged != null)
                {
                    if(value)
                    {
                        this._isConnected = true;
                    } else
                    {
                        this._isConnected = false;
                    }
                    PropertyChanged(this, new PropertyChangedEventArgs("IsConnected"));
                };
            }
        }
        private bool _xHomed { get; set; }
        public bool IsXHomed
        {
            get { return this._xHomed; }
            set
            {
                if (this.PropertyChanged != null)
                {
                    this._xHomed = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("IsXHomed"));
                }
            }
        }
        private bool _yHomed { get; set; }
        public bool IsYHomed
        {
            get { return this._yHomed; }
            set
            {
                if (this.PropertyChanged != null)
                {
                    this._yHomed = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("IsYHomed"));
                }
            }
        }
        private bool _xEn { get; set; }
        public bool XEnabled
        {
            get { return this._xEn; }
            set
            {
                if (this.PropertyChanged != null)
                {
                    this._xEn = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("XEnabled"));

                }
            }
        }
        private bool _yEn { get; set; }
        public bool YEnabled { get { return this._yEn; }
            set
            {
             if (this.PropertyChanged != null)
             {
             this._yEn = value;
             PropertyChanged(this, new PropertyChangedEventArgs("YEnabled"));
             }
            }
            }

        public bool IsSimulation { get; set; }

        // Flags for encoder data.
        private decimal _xPos { get; set; }
        public decimal XPosition
        {
            get { return this._xPos; }
            set
            {
                if (this.PropertyChanged != null)
                {
                    this._xPos = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("XPosition"));
                }
            }
        }
        private decimal _yPos { get; set; }
        public decimal YPosition
        {
            get { return _yPos; }
            set
            {
                if (this.PropertyChanged != null)
                {
                    this._yPos = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("YPosition"));

                }
            }
        }

        string SerialNumber { get; set; }

        int InitalizationTimeoutInMillis = 5000;
        int HomingTimeoutInMillis = 10000;
        int MovementTimeoutInMillis = 25000;
        int PollingFrequencyInMillis = 100;

        decimal DefaultVelocity = 200.0m;    // Stage Velocity in mm/s

        public BenchtopBrushlessMotor MicroscopeMotor { get; set; }

        public BrushlessMotorChannel MicroscopeXAxis { get; set; }
        public BrushlessMotorChannel MicroscopeYAxis { get; set; }

        MotorConfiguration XAxisConfiguration { get; set; }
        MotorConfiguration YAxisConfiguration { get; set; }

        SAWCaptureAgent TekMSO54;
        // Scan configuration information for linking to the UI
        private decimal _xOrigin { get; set; }
        private decimal _yOrigin { get; set; }
        private decimal _xDelta { get; set; }
        private decimal _yDelta { get; set; }

        public decimal XOrigin
        {
            get { return this._xOrigin; }
            set
            {
                if(this.PropertyChanged != null)
                {
                    this._xOrigin = (decimal)value;
                    PropertyChanged(this, new PropertyChangedEventArgs("XOrigin"));
                }
            }
        }

        public decimal YOrigin
        {
            get { return this._yOrigin; }
            set
            {
                if(this.PropertyChanged != null)
                {
                    this._yOrigin = (decimal)value;
                    PropertyChanged(this, new PropertyChangedEventArgs("YOrigin"));

                }
            }
        }

        public decimal XDelta
        {
            get { return this._xDelta; }
            set
            {
                if(this.PropertyChanged != null)
                {
                    this._xDelta = Convert.ToDecimal(value);
                    PropertyChanged(this, new PropertyChangedEventArgs("XDelta"));

                }
            }
        }

        public decimal YDelta
        {
            get { return this._yDelta; }
            set
            {
                if(this.PropertyChanged != null)
                {
                    this._yDelta = Convert.ToDecimal(value);
                    PropertyChanged(this, new PropertyChangedEventArgs("YDelta"));

                }
            }
        }

        // More scan configuration data
        private int _pointsPerLine { get; set; }
        private int _linesPerPatch { get; set; }
        private int _pointsPerPatch { get; set; }
        private int _currentRow { get; set; }

        public double ScanResolution;
        public double RequestedVelocity;

        public int PointsPerLine
        {
            get { return this._pointsPerLine; }
            set
            {
                if(this.PropertyChanged != null)
                {
                    this._pointsPerLine = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("PointsPerLine"));
                    
                }
            }
        }
        public int LinesPerPatch
        {
            get { return this._linesPerPatch; }
            set
            {
                if(this.PropertyChanged != null)
                {
                    this._linesPerPatch = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("LinesPerPatch"));

                }
            }
        }
        public int PointsPerPatch
        {
            get { return this._pointsPerPatch; }
            set
            {
                if (PropertyChanged != null)
                {
                    this._pointsPerPatch = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("PointsPerPatch"));

                }
            }
        }
        public int CurrentRow
        {
            get { return this._currentRow; }
            set
            {
                if(PropertyChanged != null)
                {
                    this._currentRow = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentRow"));

                }
            }
        }
        public MLSStageAgent(string serial="73000001", bool simmode=true)
        {
            this._isConnected = false;
            this.IsXHomed = false;
            this.IsYHomed = false;
            this.XEnabled = false;
            this.YEnabled = false;
            this.SerialNumber = serial;
            this.IsSimulation = simmode;
            this.TekMSO54 = new SAWCaptureAgent();

            // Build the device list and connect to the microscope stage
            try
            {
                // Do some simulation specific stuff if that's going on.
                if (this.IsSimulation == true)
                {
                    SimulationManager.Instance.InitializeSimulations();

                }
                else
                {
                    DeviceManagerCLI.BuildDeviceList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured when trying to build the device list for the stage.");
                Console.WriteLine("Exception: {0}", ex.Message);
                Console.WriteLine("Press <ENTER> to continue execution.");
                Console.ReadLine();
            }

        }


        public void ConnectStage()
        {
            if(!this._isConnected)
            {
                try
                {
                    // Create the stage object.
                    this.MicroscopeMotor = BenchtopBrushlessMotor.CreateBenchtopBrushlessMotor(this.SerialNumber);
                    this.MicroscopeMotor.Connect(this.SerialNumber);
                } 
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured when trying to create the actual stage object.");
                    Console.WriteLine("Exception: {0}", ex.Message);
                    Console.WriteLine("Press <ENTER> to continue execution.");
                    Console.ReadLine();
                }

                try
                {
                    // Create the axes
                    this.MicroscopeXAxis = this.MicroscopeMotor.GetChannel(1);
                    this.MicroscopeXAxis.StartPolling(this.PollingFrequencyInMillis);
                    if (!this.MicroscopeXAxis.IsSettingsInitialized())
                    {
                        this.MicroscopeXAxis.WaitForSettingsInitialized(this.InitalizationTimeoutInMillis);

                    }
                    this.MicroscopeXAxis.EnableDevice();
                    this.XEnabled = true;
                    this.MicroscopeYAxis = this.MicroscopeMotor.GetChannel(2);
                    this.MicroscopeYAxis.StartPolling(this.PollingFrequencyInMillis);

                    if (!this.MicroscopeYAxis.IsSettingsInitialized())
                    {
                        this.MicroscopeYAxis.WaitForSettingsInitialized(this.InitalizationTimeoutInMillis);

                    }
                    this.MicroscopeYAxis.EnableDevice();
                    this.YEnabled = true;
                    // Get the settings so you can use metric instead of encoder steps
                    this.XAxisConfiguration = this.MicroscopeXAxis.GetMotorConfiguration(this.MicroscopeXAxis.DeviceID, DeviceConfiguration.DeviceSettingsUseOptionType.UseDeviceSettings);
                    this.YAxisConfiguration = this.MicroscopeYAxis.GetMotorConfiguration(this.MicroscopeYAxis.DeviceID, DeviceConfiguration.DeviceSettingsUseOptionType.UseDeviceSettings);

                } catch (Exception ex)
                {
                    Console.WriteLine("An error occured when trying to create the stage axes, or load one of the axes MotorConfigurations.");
                    Console.WriteLine("Exception: {0}", ex.Message);
                    Console.WriteLine("Press <ENTER> to continue execution.");
                    Console.ReadLine();
                }
                this.IsConnected = true;

                } else if (this.IsConnected)
                {
                this.MicroscopeMotor.Disconnect(true);
                this.IsConnected = false;
                this.IsXHomed = false;
                this.IsYHomed = false;
                this.XEnabled = false;
                this.YEnabled = false;
                
                }
            }

        public void HomeX()
        {
            if(this.MicroscopeXAxis.IsEnabled)
            {
                this.MicroscopeXAxis.Home(this.HomingTimeoutInMillis);
                this.IsXHomed = true;
            }
        }

        public void HomeY()
        {
            if(this.MicroscopeYAxis.IsEnabled)
            {
                this.MicroscopeYAxis.Home(this.HomingTimeoutInMillis);
                this.IsYHomed = true;
            }
        }

        public void PositionUpdateWorker()
        {
            if(this.MicroscopeMotor.IsConnected)
            {
                while(true)
                {
                    this.XPosition = this.MicroscopeXAxis.Position;
                    this.YPosition = this.MicroscopeYAxis.Position;
                    Thread.Sleep(this.PollingFrequencyInMillis);
                }
            }
        }

        public void MoveTest()
        {
            if (this.XEnabled)
            {
                if (this.YEnabled)
                {
                    decimal position = 10.0m;
                    this.MoveTo(position, position);
                }
            }
        }

        public bool MoveTo(decimal _requestedX, decimal _requestedY, decimal _requestedVelocity=1m)
        {
            if(_requestedVelocity == 1m)
            {
                VelocityParameters xParam = this.MicroscopeXAxis.GetVelocityParams();
                xParam.MaxVelocity = 200.0m;
                this.MicroscopeXAxis.SetVelocityParams(xParam);
                VelocityParameters yParam = this.MicroscopeYAxis.GetVelocityParams();
                xParam.MaxVelocity = 200.0m;
                this.MicroscopeYAxis.SetVelocityParams(yParam);
                _requestedVelocity = this.DefaultVelocity;
            }

            if(this.XEnabled)
            {
                this.MicroscopeXAxis.MoveTo(_requestedX, this.MovementTimeoutInMillis);
            }
            if(this.YEnabled)
            {
                this.MicroscopeYAxis.MoveTo(_requestedY, this.MovementTimeoutInMillis);
            }
            return true;

        }

        public bool ExecuteScan()
        {
            // Check that some critical flags are set.
            if (!this.IsConnected)
            {
                MessageBox.Show("You have to connect the stage before you can run a scan.", "Stage not connected!");
                return false;

            }
            if (!this.IsXHomed)
            {
                MessageBox.Show("You have to home the X axis before you can run a scan.", "Stage not homed!");
                return false;
            }
            if (!this.IsYHomed)
            {
                MessageBox.Show("You have to home the Y axis before you can run a scan.", "Stage not homed!");
                return false;
            }
            decimal currentXPosition = this.MicroscopeXAxis.Position;
            decimal currentYPosition = this.MicroscopeYAxis.Position;
            // TODO: Insert Oscilloscope Initalization Code Here.

            // Move to the start of the scan
            if(currentXPosition != XOrigin)
            {
                this.MoveTo(this.XOrigin, this.YOrigin);
            }

            // Do the focus dance around the area of interest.
            // Check/Test focus at: 180*, 300*, 60* @ 3mm, 5mm radii
            // Why? Because that's where the piezomikes are wrt to the stage CS
            this.MoveTo(this.XOrigin - 5.0m, this.YOrigin);
            MessageBox.Show("Focus the sample using piezomike #1. Press OK to continue.", "Focus Procedure 1 of 6");
            this.MoveTo(this.XOrigin + 2.5m, this.YOrigin - 4.330m);
            MessageBox.Show("Focus the sample using piezomike #2. Press OK to continue.", "Focus Procedure 2 of 6");
            this.MoveTo(this.XOrigin + 2.5m, this.YOrigin + 4.330m);
            MessageBox.Show("Focus the sample using piezomike #3. Press OK to continue.", "Focus Procedure 3 of 6");

            // Make the circle bigger now and do another scan to make sure it'll be 'flat enough'
            this.MoveTo(this.XOrigin - 7.0m, this.YOrigin);
            MessageBox.Show("Verify focus, adjust using piezomike #1 if needed. Press OK to continue.", "Focus Procedure 4 of 6");
            this.MoveTo(this.XOrigin  + 3.5m, this.YOrigin - 6.062m);
            MessageBox.Show("Verify focus, adjust using piezomike #2 if needed. Press OK to continue.", "Focus Procedure 5 of 6");
            this.MoveTo(this.XOrigin + 3.5m, this.YOrigin + 6.062m);
            MessageBox.Show("Verify focus, adjust using piezomike #3 if needed. Press OK to continue.", "Focus Procedure 6 of 6");

            // Move back to the start of the scan and prompt the user to align the detector.
            if (currentXPosition != XOrigin)
            {
                this.MoveTo(this.XOrigin, this.YOrigin);
            }
            MessageBoxResult AlignmentPrompt = MessageBox.Show("Align the beam onto the detector and check for SAW presence before continuing.", "Align Knife-edge Detector");

            decimal _yStartCoordinate = this.YOrigin;
            decimal _yFinishCoordinate = this.YOrigin + this.YDelta;

            //byte[] _transCmd = System.Text.Encoding.ASCII.GetBytes("TRANSFER\n");
            //int _transCmdSize = _transCmd.Length;

            

            // Begin the scan logic loop here.
            for (int i = 0; i < this.LinesPerPatch + 1; i++)
            {
                // TODO: Configure Oscilloscope to acquire pointsPerLine points in fastframe.
                this.CurrentRow = i;
                this.TekMSO54.MBSession.SendEndEnabled = true;
                this.TekMSO54.MBSession.FormattedIO.WriteLine("ACQ:STATE STOP");

                this.TekMSO54.SetupFastFrame(6000, this.PointsPerLine);

                decimal _currentXCoordinate = this.XOrigin + (decimal)(i * this.ScanResolution);

                // Move to the beginning of the line
                this.MoveTo(_currentXCoordinate, _yStartCoordinate);
                this.TekMSO54.MBSession.FormattedIO.WriteLine("ACQ:STATE RUN");
                Thread.Sleep(100);
                
                // Execute move
                this.MoveTo(_currentXCoordinate, _yFinishCoordinate);
                this.TekMSO54.MBSession.FormattedIO.WriteLine("ACQ:STATE STOP");

                string[,] _lineResults = new string[this.PointsPerLine, 2];
                string[] _csvData = new string[this.PointsPerLine];
                for (int k = 0; k < this.PointsPerLine; k++)
                {
                    int j = k + 1;
                    string _selCmd = "HORIZONTAL:FASTFRAME:SELECTED " + j.ToString() + ";*OPC?";
                    this.TekMSO54.MBSession.FormattedIO.WriteLine(_selCmd);
                    this.TekMSO54.MBSession.FormattedIO.ReadLine();
                    this.TekMSO54.MBSession.FormattedIO.WriteLine("MEASU:MEAS1:RESU:CURR:MEAN?");
                    _lineResults[k,0] = this.TekMSO54.MBSession.FormattedIO.ReadLine();
                    this.TekMSO54.MBSession.FormattedIO.WriteLine("MEASU:MEAS3:RESU:CURR:MEAN?");
                    _lineResults[k,1] = this.TekMSO54.MBSession.FormattedIO.ReadLine();
                    _csvData[k] = _lineResults[k, 0].Replace("\n", String.Empty) + "," + _lineResults[k, 1].Replace("\n", String.Empty);
                }

                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@"E:\sts\line-" + i.ToString() + ".csv"))
                {
                    foreach (string line in _csvData)
                    {
                        file.WriteLine(line);
                    }
                }
            }
            // TODO: Insert Oscillscope Fastframe configuration scripts here.
            
            return true;
        }
        }

        
    }
