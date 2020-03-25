using System;
using System.ComponentModel;

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
			this._isConnected = true;
			return 0;
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
	}
}