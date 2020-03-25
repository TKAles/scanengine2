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

        public MainWindow()
        {
            InitializeComponent();
            this.svm = new ScanengineViewModel();
            DataContext = this.svm;
        }

        private void UiConnectMLSStage_Click(object sender, RoutedEventArgs e)
        {
            // Connect to the stage, or disconnect from the stage
            if(this.svm.IsMLSConnected == false)
            { 
            this.svm.ConnectMLSStage();
            ui_is_MLS_connected.Text = this.svm.IsMLSConnected.ToString();
            Button _connectedbutton = (Button)sender;
            _connectedbutton.Content = "Disconnect MLS203";
            } else
            {
                this.svm.DisconnectMLSStage();
                ui_is_MLS_connected.Text = this.svm.IsMLSConnected.ToString();
                Button _disconnectedbutton = (Button)sender;
                _disconnectedbutton.Content = "Connect MLS203";
            }

        }
        private void Ui_loaded(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
