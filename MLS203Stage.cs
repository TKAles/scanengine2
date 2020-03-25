using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.Benchtop.BrushlessMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;


namespace wpfscanengine
{
	public class MLS203Stage : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private string _serial;
		public string SerialNumber
		{
			get { return _serial; }
			set { _serial = value; }
		}

		private bool _isConnected;
		public bool IsConnected
		{
			get { return _isConnected;  }
		}

		private bool _isXHomed;
		public bool IsXHomed
		{
			get { return _isXHomed;  }
		}

		private bool _isYHomed;
		public bool IsYHomed
		{
			get { return _isYHomed; }
		}

		public bool IsHomed
		{
			get { return (_isXHomed & _isYHomed); }
		}

		private BenchtopBrushlessMotor _microscopemotor;
		private BrushlessMotorChannel _stepoverchannel;
		private BrushlessMotorChannel _scanningchannel;
		private MotorConfiguration _stepoverconfig;
		private MotorConfiguration _scanningconfig;

		private decimal _dfVel = 200.0m;
		public decimal ScanVelocity
		{
			get { return _dfVel; }
			set { _dfVel = value; }
		}

		private int _initTimeoutMillis = 5000;
		private int _homingTimeoutMillis = 15000;
		private int _mvmtTimeoutMillis = 15000;
		private int _pollFrequency = 100;

		public decimal XPosition
		{
			get { return _stepoverchannel.Position; }
		}

		public decimal YPosition
		{ 
			get { return _scanningchannel.Position; }
		}


		private bool _pollingactive = false;

		public MLS203Stage(string _ser = "73000001")
		{
			if (_ser.Equals("73000001"))
			{
				SimulationManager.Instance.InitializeSimulations();
			}
			else
			{
				DeviceManagerCLI.BuildDeviceList();
			}
			return;
		}

		public int ConnectStage()
		{
			try
			{
				if (SerialNumber.Equals("73000001"))
				{
					/*
					 * 73000001 is the initial SN given to any
					 * BBD203 controller by the Kinesis Simulation software
					 * this is most likely running under simulation and should 
					 * use that startup sequence.
					 */
					SimulationManager.Instance.InitializeSimulations();
				}
				else
				{
					// Otherwise populate the device manager like you normally do.
					DeviceManagerCLI.BuildDeviceList();
				}
			} catch (Exception ex)
			{
				Console.WriteLine("An error occured when trying to build the device list for the stage.");
				Console.WriteLine("Exception: {0}", ex.Message);
				Console.WriteLine("Press <ENTER> to continue execution.");
				Console.ReadLine();
				return -1;
			}
			// Create the stage object and attempt to connect to it.
			try
			{
				this._microscopemotor = BenchtopBrushlessMotor.CreateBenchtopBrushlessMotor(this._serial);
				this._microscopemotor.Connect(this._serial);
			} catch (Exception ex)
			{
				Console.WriteLine("An error occured when trying to create the actual stage object.");
				Console.WriteLine("Exception: {0}", ex.Message);
				Console.WriteLine("Press <ENTER> to continue execution.");
				Console.ReadLine();
			}

			// Setup the axes
			// X - Stepover Axis
			// Y - Scanning Axis
			this._stepoverchannel = this._microscopemotor.GetChannel(2);
			this._stepoverchannel.StartPolling(this._pollFrequency);
			if(!this._stepoverchannel.IsSettingsInitialized())
			{
				this._stepoverchannel.WaitForSettingsInitialized(this._initTimeoutMillis);
			}
			this._scanningchannel = this._microscopemotor.GetChannel(1);
			this._scanningchannel.StartPolling(this._pollFrequency);
			if(!this._scanningchannel.IsSettingsInitialized())
			{
				this._scanningchannel.WaitForSettingsInitialized(this._initTimeoutMillis);
			}
			// Enable both axes
			this._stepoverchannel.EnableDevice();
			this._scanningchannel.EnableDevice();
			// Load the motor configurations off the device
			this._stepoverconfig = this._stepoverchannel.GetMotorConfiguration(this._stepoverchannel.DeviceID, DeviceConfiguration.DeviceSettingsUseOptionType.UseDeviceSettings);
			this._scanningconfig = this._scanningchannel.GetMotorConfiguration(this._scanningchannel.DeviceID, DeviceConfiguration.DeviceSettingsUseOptionType.UseDeviceSettings);
			this._isConnected = true;
			return 0;

			// Start various stage polling functions
			this._pollingactive = true;
			Task _pollStage = Task.Run(() => this.StageFlagPoller());
		}

		public int DisconnectStage()
		{
			if(this._isConnected == true)
			{
				this._microscopemotor.DisconnectTidyUp();
				this._isConnected = false;
			}
			return 0;
		}

		public int HomeStage()
		{
			if(this._isConnected == true)
			{
				this._stepoverchannel.Home(this._homingTimeoutMillis);
				while(this._stepoverchannel.IsDeviceBusy)
				{
					Thread.Sleep(250);
				}
				this._isXHomed = true;
				this._scanningchannel.Home(this._homingTimeoutMillis);
				while(this._scanningchannel.IsDeviceBusy)
				{
					Thread.Sleep(250);
				}
				this._isYHomed = true;
			} else
			{
				// Do something else. Not sure what yet.
			}
			return 0;
		}

		public void StageFlagPoller()
		{
			return;
		}
		public int MoveStageTo(decimal _reqXCoord, decimal _reqYCoord)
		{
			if(this._isConnected)
			{
				try
				{ 
					_stepoverchannel.MoveTo(_reqXCoord, this._mvmtTimeoutMillis);
					_scanningchannel.MoveTo(_reqYCoord, this._mvmtTimeoutMillis);
				} catch (InvalidPositionException)
				{
					// TODO: Add exception handling for invalid positions.
					return -1;
				}
			}
			return 0;
		}
	}
}