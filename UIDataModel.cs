using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace wpfscanengine
{
	public class UIDataModel : INotifyPropertyChanged
	{
		private decimal _rowstep;
		private int _pointsperrow;
		private decimal _scanvelocity;

		public event PropertyChangedEventHandler PropertyChanged;

		static UIDataModel _dataset;
		
		// XY Origin / Delta Data
		private decimal _xorigin;
		private decimal _yorigin;
		private decimal _xdelta;
		private decimal _ydelta;
		public string XOrigin
        {
			get { return _xorigin.ToString("F3");  }
			set { _xorigin = Decimal.Parse(value);
				OnPropertyChanged();
			}
        }
		public string YOrigin
        {
			get { return _yorigin.ToString("F3"); }
			set { _yorigin = Decimal.Parse(value);
				OnPropertyChanged();
			}
        }
		public string XDelta
        {
			get { return _xdelta.ToString("F3"); }
			set { _xdelta = Decimal.Parse(value);
				OnPropertyChanged();
			}
        }
		public string YDelta
		{
			get { return _ydelta.ToString("F3"); }
			set
			{
				_ydelta = Decimal.Parse(value);
				OnPropertyChanged();
			}
		}

		// Mechatronics Serial Number information
		private long _motionsn;
		private long _rotationsn;
		public string MotionSerial
		{
			get { return _motionsn.ToString(); }
			set { 
				_motionsn = Int64.Parse(value);
				OnPropertyChanged();
			}
		}
		public string RotationSerial
			{
			get { return _rotationsn.ToString(); }
			set {
				_rotationsn = Int64.Parse(value);
				OnPropertyChanged();
				}
		}


		private decimal _xposition;
		private decimal _yposition;
		
		public string XPosition
		{
			get { return _xposition.ToString("F3");}
			set { _xposition = Decimal.Parse(value);
				  OnPropertyChanged();
				}
		}

		public string YPosition
		{
			get { return _yposition.ToString("F3");}
			set { _yposition = Decimal.Parse(value);
				  OnPropertyChanged();}
		}

		public UIDataModel()
		{
			this.MotionSerial = "73000001";
			this.XPosition = "0.000";
			this.YPosition = "0.000";

			return;
		}

		public UIDataModel GetData()
        {
			if(_dataset == null)
            {
				_dataset = new UIDataModel();
            }
			return _dataset;
        }
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}