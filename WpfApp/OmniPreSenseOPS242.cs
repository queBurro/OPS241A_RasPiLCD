using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BitflowUtils
{
    public class OmniPreSenseOPS242
    {
        public const string NO_DATA = "--" ;

        TextBlock display ;
        public TextBlock Display {
            set {
                display = value ;
                display.Text = NO_DATA ;
            }
            private get {
                return display ;
            }
        }

        // Modifiable parameters for this code (not necessarily the sensor's profile)
        const    int TARGET_MAX_SPEED_ALLOWED = 150;      // max speed to be tracked; anything faster is ignored
        const    int TARGET_MIN_SPEED_ALLOWED = 13;       // min speed to be tracked; anything slower is ignored
        const double IDLE_NOTICE_INTERVAL = 10.0;         // time in secs waiting to, um, take action on idle (in state of "not tracking" only)
        const double TARGETLESS_MIN_INTERVAL_TIME = 0.75; // grace period for object to track again (hysteresis)
        // note that a change in direction does not allow for this.  
        const double MIN_TRACK_TO_ACQUIRED_TIME = 0.1;    // min time in secs that object needs to be tracked for it to be counted
        const string OPS24X_INFO_QUERY_COMMAND = "??";

        // OPS24x configuration parameters (sent to sensor)
        const string OPS24X_UNITS_PREF = "UK";            // US for MPH, UK for Km/H, "UC" for cm/s
        const string OPS24X_SAMPLING_FREQUENCY = "SX";    // 10Ksps
        const string OPS24X_TRANSMIT_POWER = "PX";        // max power
        const string OPS24X_MAGNITUDE_MIN = "M>20\n";     // Magnitude must be > this
        const string OPS24X_DECIMAL_DIGITS = "F0";        // F-zero for no decimal reporting
        const string OPS24X_BLANKS_PREF = "BZ";           // Blanks pref: send 0's not silence
        const string OPS24X_LIVE_SPEED = "O1OS";          // OS cancels 9243 mode, enables no-delay speeds.  O1 only one speed
        readonly static string OPS24X_MAX_REPORTABLE = "R<" + TARGET_MAX_SPEED_ALLOWED + "\n";   // Report only < than this speed
        readonly static string OPS24X_MIN_REPORTABLE = "R>" + TARGET_MIN_SPEED_ALLOWED + "\n";   // Report only > this speed
        const string OPS24X_BIDIRECTIONAL = "R|";
        const string OPS24X_INBOUND_ONLY = "R+";
        const string OPS24X_OUTBOUND_ONLY = "R|";
        const string OPS24X_DIRECTION_PREF = "R|";        // OPS24X_BIDIRECTIONAL / OPS24X_INBOUND_ONLY;

        // These are for lab development only, so hand-waves are usable
        // private string OPS24X_UNITS_PREF = "UC";  // "UC" for cm/s
        // private int TARGET_MAX_SPEED_ALLOWED = 150;
        // private string OPS24X_DIRECTION_PREF = OPS24X_INBOUND_ONLY;
        // remove them when moving to actual vehicle testing.

        System.IO.Ports.SerialPort serialPort;

        public OmniPreSenseOPS242()
        {
            serialPort = new System.IO.Ports.SerialPort(new System.ComponentModel.Container());
        }

        public void ConnectUsb( string portName )
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }

            serialPort.PortName = portName ;
            // baudrate = 115200
            // parity = serial.PARITY_NONE
            // stopbits = serial.STOPBITS_ONE
            // bytesize = serial.EIGHTBITS
            // timeout = 1
            // writeTimeout = 2
            serialPort.Open();

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

            FetchOutVelocity() ;
        }

        public void DisconnectUsb()
        {
            serialPort.Close();
        }

        void SendMessage(string message)
        {
            // string은 Encoding을 통해 byte[]로 변환
            // System.Text.Encoding.Unicode.GetBytes (16바이트) / System.Text.Encoding.UTF8.GetBytes (12바이트)
            byte[] utf8bytes = System.Text.Encoding.UTF8.GetBytes(message);
            serialPort.Write(utf8bytes, 0, utf8bytes.Length);
            ReceiveMessage();
        }

        string ReceiveMessage()
        {
            string recvMsg = serialPort.ReadLine();
            return recvMsg;
        }

        async void FetchOutVelocity()
        {
            string contents ;
            try {
                contents = await Task.Run( ()=>serialPort.ReadExisting() ) ;
            }
            catch {
                display.Text = NO_DATA ;
                return ;
            }
            double? velocity = null ;
            using( StringReader sr = new StringReader(contents) ) {
                string line ;
                while( ( line=sr.ReadLine() ) != null ) {
                    if( line.IndexOf('{') >= 0 ) {
                        continue ;
                    }
                    try {
                        velocity = double.Parse( line ) ;
                    }
                    catch {
                        continue ;
                    }
                }
            }
            if( velocity != null ) {
                Display.Text = ( (int)velocity ).ToString() ;
            }
            FetchOutVelocity() ;
        }
    }
}
