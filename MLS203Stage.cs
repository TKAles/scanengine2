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
	public class MLS203Stage
	{
		private string _serial;
		// Public method raises a PropertyChangedEventArg
		// so the class is aware that there is new data in the UI.
		public string SerialNumber
		{
			get { return _serial; }
			set { _serial = value; }
		}


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
	}
}