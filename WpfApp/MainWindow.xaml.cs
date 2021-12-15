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

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        // Modifiable parameters for this code (not necessarily the sensor's profile)
        private static int TARGET_MAX_SPEED_ALLOWED = 150;  // max speed to be tracked; anything faster is ignored
        private static int TARGET_MIN_SPEED_ALLOWED = 13;   // min speed to be tracked; anything slower is ignored
        private double IDLE_NOTICE_INTERVAL = 10.0;         // time in secs waiting to, um, take action on idle (in state of "not tracking" only)
        private double TARGETLESS_MIN_INTERVAL_TIME = 0.75; // grace period for object to track again (hysteresis)
        // note that a change in direction does not allow for this.  
        private double MIN_TRACK_TO_ACQUIRED_TIME = 0.1;    // min time in secs that object needs to be tracked for it to be counted
        private string OPS24X_INFO_QUERY_COMMAND = "??";

        // OPS24x configuration parameters (sent to sensor)
        private string OPS24X_UNITS_PREF = "UC";            // US for MPH, UK for Km/H, "UC" for cm/s
        private string OPS24X_SAMPLING_FREQUENCY = "SX";    // 10Ksps
        private string OPS24X_TRANSMIT_POWER = "PX";        // max power
        private string OPS24X_MAGNITUDE_MIN = "M>20\n";     // Magnitude must be > this
        private string OPS24X_DECIMAL_DIGITS = "F0";        // F-zero for no decimal reporting
        private string OPS24X_BLANKS_PREF = "BZ";           // Blanks pref: send 0's not silence
        private string OPS24X_LIVE_SPEED = "O1OS";          // OS cancels 9243 mode, enables no-delay speeds.  O1 only one speed
        private string OPS24X_MAX_REPORTABLE = "R<" + TARGET_MAX_SPEED_ALLOWED + "\n";   // Report only < than this speed
        private string OPS24X_MIN_REPORTABLE = "R>" + TARGET_MIN_SPEED_ALLOWED + "\n";       // Report only > this speed
        private string OPS24X_BIDIRECTIONAL = "R|";
        private string OPS24X_INBOUND_ONLY = "R+";
        private string OPS24X_OUTBOUND_ONLY = "R|";
        private string OPS24X_DIRECTION_PREF = "R|";        // OPS24X_BIDIRECTIONAL / OPS24X_INBOUND_ONLY;

        // These are for lab development only, so hand-waves are usable
        // private string OPS24X_UNITS_PREF = "UC";  // "UC" for cm/s
        // private int TARGET_MAX_SPEED_ALLOWED = 150;
        // private string OPS24X_DIRECTION_PREF = OPS24X_INBOUND_ONLY;
        // remove them when moving to actual vehicle testing.

        private System.IO.Ports.SerialPort serialPort1;

        public MainWindow()
        {
            this.serialPort1 = new System.IO.Ports.SerialPort(new System.ComponentModel.Container());
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (string com in System.IO.Ports.SerialPort.GetPortNames())
                comboBox1.Items.Add(com);
        }

        private void ConnectUsb(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                return;
            }

            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                serialPort1.PortName = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            }
            else
            {
                serialPort1.PortName = comboBox1.Items[comboBox1.SelectedIndex].ToString();
                serialPort1.Open();
                SendMessage(OPS24X_SAMPLING_FREQUENCY);
                SendMessage(OPS24X_TRANSMIT_POWER);
                SendMessage(OPS24X_MAGNITUDE_MIN);
                SendMessage(OPS24X_DECIMAL_DIGITS);
                SendMessage(OPS24X_MIN_REPORTABLE);
                SendMessage(OPS24X_MAX_REPORTABLE);
                SendMessage(OPS24X_UNITS_PREF);
                SendMessage(OPS24X_BLANKS_PREF);
                SendMessage(OPS24X_LIVE_SPEED);
                SendMessage(OPS24X_INBOUND_ONLY);
                
                while (true)
                {
                    string recvMsg = ReceiveMessage();
                    if (recvMsg.IndexOf('{') == -1)
                    {
                        double velocity = Double.Parse(recvMsg);
                        Console.WriteLine("Velocity : " + velocity);
                        txtVelocity.Text = recvMsg;
                        // Thread.Sleep(500);
                    }
                }
            }
        }

        private void DisconnectUsb(object sender, RoutedEventArgs e)
        {
            serialPort1.Close();
        }

        private void SendMessage(string message)
        {
            // string은 Encoding을 통해 byte[]로 변환
            // System.Text.Encoding.Unicode.GetBytes (16바이트) / System.Text.Encoding.UTF8.GetBytes (12바이트)
            byte[] utf8bytes = System.Text.Encoding.UTF8.GetBytes(message);
            serialPort1.Write(utf8bytes, 0, utf8bytes.Length);
            ReceiveMessage();
        }

        private string ReceiveMessage()
        {
            string recvMsg = serialPort1.ReadLine();
            return recvMsg;
        }

    }
}
