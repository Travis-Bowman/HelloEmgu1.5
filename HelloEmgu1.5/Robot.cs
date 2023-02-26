using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloEmgu1._5
{
    public class Robot
    {
        public const byte STOP = 0x7F;
        public const byte FLOAT = 0x0F;
        public const byte FORWARD = 0x6f;
        public const byte BACKWARD = 0x5F;
        SerialPort _serialPort;
        public bool Online { get; private set; }

        public Robot() { }

        public Robot(String port)
        {
            SetupSerialComms(port);
        }

        public void SetupSerialComms(String port)
        {
            try
            {
                _serialPort = new SerialPort(port);
                _serialPort.BaudRate = 2400;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.Two;
                _serialPort.Open();
                Online = true;
            }
            catch
            {
                Online = false;
            }
        }

        public void Move(char command)
        {
            try
            {
                if (Online)
                {
                    byte[] buffer = { Convert.ToByte(command) };
                    _serialPort.Write(buffer,0,1);
                }
            }
            catch
            {
                Online = false;
            }
        }

        public void Close()
        {
            _serialPort.Close();
        }

    }
}

