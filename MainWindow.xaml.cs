using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;

namespace wpfscanengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ScanengineViewModel svm;
        private bool ui_updating = false;
        public MainWindow()
        {
            this.svm = new ScanengineViewModel();
            InitializeComponent();
            DataContext = this.svm;
            this.ui_MSOaddress.Text = this.svm.CurrentMSOAddress;
            // Start the encoder update task
            ui_updating = true;
        }

        private void UiConnectMLSStage_Click(object sender, RoutedEventArgs e)
        {
            Task _connectTask = Task.Run(() =>
            {
            // Connect to the stage, or disconnect from the stage
            if (this.svm.IsMLSConnected == false)
            {
                    Button _connectedbutton = (Button)sender;
                    this.Dispatcher.Invoke(() =>
                    {
                        _connectedbutton.Content = "Connecting...";
                        _connectedbutton.IsEnabled = false;
                    });
                    this.svm.ConnectMLSStage();
                    this.Dispatcher.Invoke(() =>
                    {
                        ui_is_MLS_connected.Text = this.svm.IsMLSConnected.ToString();
                        _connectedbutton.Content = "Disconnect MLS203";
                        _connectedbutton.IsEnabled = true;
                    });
                } else
                {
                    this.svm.DisconnectMLSStage();
                    this.Dispatcher.Invoke(() =>
                    {
                        ui_is_MLS_connected.Text = this.svm.IsMLSConnected.ToString();
                        Button _disconnectedbutton = (Button)sender;
                        _disconnectedbutton.Content = "Connect MLS203";
                        ui_homeMLS.IsEnabled = true;
                        ui_homeMLS.Content = "Home MLS203";
                    });
                }
            });
        }

        private void UiRecomputeScanStrategyCombo(object sender, SelectionChangedEventArgs e)
        {
            this.svm.CalculateScanStrategy();
            this.Dispatcher.Invoke(() =>
            {
                ui_rowsreqd_mm.Text = this.svm.LinesNeeded.ToString();
                ui_ptsreqd.Text = this.svm.PointsNeeded.ToString();
            });
        }
        private void UiMoveToPosition_Click(object sender, RoutedEventArgs e)
        {
            Task _moveTask = Task.Run(() => { this.svm.MoveToPosition(); });
        }

        private void UiHomeMLSStage_Click(object sender, RoutedEventArgs e)
        {
            Task _homeTask = Task.Run(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Button _homing = (Button)sender;
                    _homing.Content = "Homing...";
                    _homing.IsEnabled = false;
                });
                this.svm.HomeStage();
                if (this.svm.IsMLSXHomed)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ui_is_MLSX_homed.Text = this.svm.IsMLSXHomed.ToString();
                    });
                }
                if (this.svm.IsMLSYHomed)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ui_is_MLSY_homed.Text = this.svm.IsMLSYHomed.ToString();
                    });
                }
                this.Dispatcher.Invoke(() =>
                {
                    ui_homeMLS.Content = "Homed.";
                });
            });
        }

        private void UiUpdateTask()
        {
            while (this.ui_updating == true)
            {
                // Check the MLS readiness status and set the appropriate
                // true/false flag. Use Dispatcher.Invoke() to change the
                // property.
                if (this.svm.IsReady)
                {
                    this.Dispatcher.Invoke(() =>
                    { ui_is_ready.Text = "True"; });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    { ui_is_ready.Text = "False"; });
                }

                // Read off encoder position and update TextBlocks
                if(this.svm.IsMLSConnected)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ui_xencoder_mm.Text = this.svm.XCurrent.ToString();
                        ui_yencoder_mm.Text = this.svm.YCurrent.ToString();
                    });
                }
                if (this.svm.IsMSOConnected)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ui_is_MSO_connected.Text = "True";
                    });
                } else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ui_is_MSO_connected.Text = "False";
                    });
                }
                Thread.Sleep(500);
            }
        }
        private void UiScanVelocityComboChanged(object sender, RoutedEventArgs e)
        {
            int _comboindex = ui_scanspeed.SelectedIndex;
            switch(_comboindex)
            {
                case 0:
                    this.svm.ScanVelocity = 50.0m;
                    break;
                case 1:
                    this.svm.ScanVelocity = 100.0m;
                    break;
                case 2:
                    this.svm.ScanVelocity = 200.0m;
                    break;
            }
            this.svm.CalculateScanStrategy();
        }

        private void UiScanPitchComboChanged(object sender, RoutedEventArgs e)
        {
            int _pitchindex = ui_scanpitch.SelectedIndex;
            switch(_pitchindex)
            {
                case 0:
                    this.svm.ScanPitch = 0.05m;
                    break;
                case 1:
                    this.svm.ScanPitch = 0.10m;
                    break;
                case 2:
                    this.svm.ScanPitch = 0.25m;
                    break;
            }
        }

        private void UiRecomputeScanStrategy(object sender, TextChangedEventArgs e)
        {
            // Get the latest values
            try
            { 
            if(!this.ui_ydelta_mm.Text.Equals(""))
                {
                    this.svm.YDelta = Convert.ToDecimal(ui_ydelta_mm.Text);
                }
            this.svm.XDelta = Convert.ToDecimal(ui_xdelta_mm.Text);
            this.svm.CalculateScanStrategy();
                this.Dispatcher.Invoke(() =>
                {
                    ui_rowsreqd_mm.Text = this.svm.LinesNeeded.ToString();
                    ui_ptsreqd.Text = this.svm.PointsNeeded.ToString();
                });
            } catch (NullReferenceException ex)
            {
                return;
            }
        }

        private void UiScanProcessStart(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("X-Origin: " + this.svm.XOrigin + " X-Delta: " + this.svm.XDelta);
            Console.WriteLine("Y-Origin: " + this.svm.YOrigin + " Y-Delta: " + this.svm.YDelta);
            Console.WriteLine("Rows to Scan: " + this.svm.LinesNeeded + " Points per row: " + this.svm.PointsNeeded);
            Task _scanningTask = Task.Run(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Button _scanBtn = (Button)sender;
                    _scanBtn.IsEnabled = false;
                    _scanBtn.Content = "Scanning in Progress";
                    this.ui_is_scanning.Text = "True";

                });
                decimal _startScan = this.svm.YOrigin;
                decimal _endScan = _startScan + this.svm.YDelta;
                decimal _startStepover = this.svm.XOrigin;
                /*
                 * SCAN ENGINE LOGIC
                 */

                String _savePath = "C:/TestScan";
                String _lineNumberFormat = "00000";
                for (int rows = 0; rows < this.svm.LinesNeeded; rows++)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.ui_currentrow_mm.Text = rows.ToString();
                    });
                    decimal _currentRowPosition = _startStepover + this.svm.ScanPitch * rows;
                    Console.WriteLine("Starting Scan of line " + rows.ToString());
                    Console.WriteLine("Row Coordinate: " + _currentRowPosition.ToString());
                    // Move to beginning of scan
                    Console.WriteLine("Moving to: Y: " + _startScan);
                    this.svm.MoveSingle(0, _startScan);
                    Console.WriteLine("Moving to: X: " + _currentRowPosition);
                    this.svm.MoveSingle(1, _currentRowPosition);
                    // Stage is at the beginning of the scan. Setup and arm oscillscope for fastframe.
                    /* OLD CODE FOR REFERENCE ONLY
                    this.svm.TekScope.MSOConnection.Write("ACQ:STATE 0");
                    this.svm.TekScope.SetupAcquisition(2500, this.svm.PointsNeeded);
                    */

                    String _baseName = "line-" + rows.ToString(_lineNumberFormat) + ".wfm";
                    String _RFString = _savePath + "RF-" + _baseName;
                    String _DCString = _savePath + "DC-" + _baseName;
                    String _RFSaveCMD = "SAVE:WAVEFORM CH1, \"" + _RFString + "\";*OPC?";
                    String _DCSaveCMD = "SAVE:WAVEFORM CH4, \"" + _DCString + "\";*OPC?";

                    // Arm the oscillscope, then move along the scanline
                    
                    Console.WriteLine("Moving to: Y2: " + _endScan);
                    //this.svm.TekScope.MSOConnection.Query("ACQ:STATE RUN;*OPC?", out response);
                    Thread.Sleep(50);
                    this.svm.MoveSingle(0, _endScan);
                    //this.svm.TekScope.MSOConnection.Write("ACQ:STATE STOP");
                    // Write the files to disk
                    //this.svm.TekScope.MSOConnection.Query(_RFSaveCMD, out response);
                    Thread.Sleep(100);
                    //this.svm.TekScope.MSOConnection.Query(_DCSaveCMD, out response);

                    Console.WriteLine("Linescan completed.");
                }

                this.Dispatcher.Invoke(() =>
                {
                    Button _scanBtn = (Button)sender;
                    _scanBtn.IsEnabled = true;
                    _scanBtn.Content = "Begin Scan";
                    this.ui_is_scanning.Text = "False";

                });
            });

        }
        private void UiConnectMSO_Click(object sender, RoutedEventArgs e)
        {
            if(this.svm.IsMSOConnected == false)
            { 
                this.svm.TekScope.ConnectMSO();
                this.Dispatcher.Invoke(() =>
                {
                    Button _msobutton = (Button)sender;
                    _msobutton.Content = "Disconnect MSO";
                });
            } else
            {
                this.Dispatcher.Invoke(() =>
                {
                    ui_connectMSO.Content = "Connect MSO";

                });
                this.svm.TekScope.DisconnectMSO();
            }
        }
        private void Ui_loaded(object sender, RoutedEventArgs e)
        {
            Task _UiUpdateTask = Task.Run(() => this.UiUpdateTask());
            this.ui_scanpitch.SelectionChanged += new SelectionChangedEventHandler(UiRecomputeScanStrategyCombo);
            this.ui_scanspeed.SelectionChanged += new SelectionChangedEventHandler(UiRecomputeScanStrategyCombo);

        }

    }
}
