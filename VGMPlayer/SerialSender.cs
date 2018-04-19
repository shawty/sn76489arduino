using System;
using System.IO.Ports;

namespace VGMPlayer
{
  public class SerialSender: IDisposable
  {
    private SerialPort _serialPort = new SerialPort();

    private string _portName = "COM11"; // Change this to the com port your arduino is connected too
    private int _baudRate = 115200;

    public SerialSender()
    {
      _serialPort.PortName = _portName;
      _serialPort.BaudRate = _baudRate;
      _serialPort.DataBits = 8;
      _serialPort.Parity = Parity.None;
      _serialPort.StopBits = StopBits.One;
      _serialPort.Open();
    }

    public void Send(byte b)
    {
      byte[] buf = new byte[1];
      buf[0] = b;
      _serialPort.Write(buf, 0, 1);
    }

    public void Dispose()
    {
      if(_serialPort.IsOpen)
      {
        _serialPort.Close();
      }
    }

  }
}
