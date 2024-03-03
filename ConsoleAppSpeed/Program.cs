// See https://aka.ms/new-console-template for more information


using BitflowUtils;

Console.WriteLine("Hello, speed testers!");

OmniPreSenseOPS242 speedSensor;

speedSensor = new OmniPreSenseOPS242();

var serialPortName = OmniPreSenseOPS242.DetectSerialPort();

foreach (string com in System.IO.Ports.SerialPort.GetPortNames())
{
    Console.WriteLine($"com ports {com}");
}

var key = Console.ReadKey();

Console.WriteLine( $"You pressed {key.KeyChar}");


//speedSensor.Display = txtVelocity.Text;//ref?

//if (comboBox1.SelectedIndex == 0)
//{
//    return;
//}
//speedSensor.ConnectUsb(comboBox1.Items[comboBox1.SelectedIndex].ToString());

speedSensor.DisconnectUsb();

