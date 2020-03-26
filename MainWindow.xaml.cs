using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            DataContext = this.svm;
            InitializeComponent();
            // Start the encoder update task
            ui_updating = true;
            Task _UiUpdateTask = Task.Run(() => this.UiUpdateTask());
            this.ui_scanpitch.SelectionChanged += new SelectionChangedEventHandler(UiRecomputeScanStrategyCombo);
            this.ui_scanspeed.SelectionChanged += new SelectionChangedEventHandler(UiRecomputeScanStrategyCombo);
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
            this.svm.YDelta = Convert.ToDecimal(ui_ydelta_mm.Text);
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
                decimal _startScan = this.svm.YOrigin;
                decimal _endScan = _startScan + this.svm.YDelta;
                decimal _startStepover = this.svm.XOrigin;

                for (int rows = 0; rows < this.svm.LinesNeeded; rows++)
                {
                    decimal _currentRowPosition = _startStepover + this.svm.ScanPitch * rows;
                    Console.WriteLine("Row Coordinate: " + _currentRowPosition.ToString());
                    // Move to beginning of scan
                    Console.WriteLine("Moving to: Y: " + _startScan);
                    this.svm.MoveSingle(0, _startScan);
                    Console.WriteLine("Moving to: X: " + _currentRowPosition);
                    this.svm.MoveSingle(1, _currentRowPosition);
                    Console.WriteLine("Starting Scan of line " + rows.ToString());
                    this.svm.MoveSingle(0, _endScan);
                }
            });

        }
        private void Ui_loaded(object sender, RoutedEventArgs e)
        {
           
        }

    }
}
