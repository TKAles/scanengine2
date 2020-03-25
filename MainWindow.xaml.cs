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

        MLS203Stage scopestage;
        ScanengineViewModel svm;
        private bool ui_updating = false;
        public MainWindow()
        {
            InitializeComponent();
            this.svm = new ScanengineViewModel();
            DataContext = this.svm;
            ui_updating = true;
            Task _UiUpdateTask = Task.Run(() => this.UiUpdateTask());

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
                    ui_is_MLS_connected.Text = this.svm.IsMLSConnected.ToString();
                    Button _disconnectedbutton = (Button)sender;
                    _disconnectedbutton.Content = "Connect MLS203";
                }
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
                    ui_homeMLS.IsEnabled = false;
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
            }
        }
        private void Ui_loaded(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
