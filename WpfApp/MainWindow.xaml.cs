using BitflowUtils;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        OmniPreSenseOPS242 speedSensor ;

        public MainWindow()
        {
            InitializeComponent();
            speedSensor = new OmniPreSenseOPS242() ;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (string com in System.IO.Ports.SerialPort.GetPortNames())
                comboBox1.Items.Add(com);

            speedSensor.Display = txtVelocity ;
        }

        private void ConnectUsb(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                return;
            }
            speedSensor.ConnectUsb( comboBox1.Items[comboBox1.SelectedIndex].ToString() );
        }

        private void DisconnectUsb(object sender, RoutedEventArgs e)
        {
            speedSensor.DisconnectUsb() ;
        }
    }
}
