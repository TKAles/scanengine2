using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace wpfscanengine
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            MotionStage MLSStage;
            UIDataModel ScanEngineDataModel;
            ScanEngineDataModel = new UIDataModel();
            DataContext = ScanEngineDataModel.GetData();
        }

        
    }
}
