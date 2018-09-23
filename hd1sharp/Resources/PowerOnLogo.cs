/*
 * 
 *  Copyright(c) 2018 Jean-Michel Cohen
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
*/

using System;
using System.Windows.Forms;
using SerialPortLib2;

namespace hd1sharp.Resources
{
    public partial class PowerOnLogo : Form
    {
        public string defaultPort = "COM3";
        public  float percentage = 0.0f;
        public  Int32 progressPercent = 0;
        public  Byte[] command;
        public  SerialPortInput serialPort;
        public  Boolean cancelled = false;
        public  int currentIndex = 0;
        public  Byte[] receivedData = new byte[1200];

        public PowerOnLogo()
        {
            InitializeComponent();

            percent.Text = String.Format("Completion percentage {0}%", GetPercentage);

            if (!String.IsNullOrWhiteSpace(defaultPort))
            {
                serialPort = new SerialPortInput(defaultPort, 115200, SerialPortLib2.Port.Parity.None, 8, SerialPortLib2.Port.StopBits.One, SerialPortLib2.Port.Handshake.RequestToSend, true);
            }
        }

        public String Port
        {
            get
            {
                return defaultPort;
            }
            set
            {
                defaultPort = value;
            }
        }
         string ByteToHexBitFiddle(byte[] bytes)
        {
            Int32 b;
            Char[] c = new Char[bytes.Length * 2];
     
            for (Int16 i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (Char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (Char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new String(c);
        }

         void SetProgressBarSafe(int result)
        {
            // PowerOnProgressBar.Value = result;
        }

        public string GetPercentage
        {
            get
            {
                return Math.Round(percentage).ToString();
            }
        }

         void closeSerial()
        {
            if (serialPort.IsConnected)
            {
                serialPort.ConnectionStatusChanged -= SerialPort_ConnectionStatusChanged;
                serialPort.MessageReceived -= SerialPort_MessageReceived;

                Console.WriteLine("\nTest sequence completed, now disconnecting.");
                serialPort.Disconnect();
            }
        }

         void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (cancelled) {
                closeSerial();
                return;
            }

            // get received data
            for (UInt16 i = 0; i < args.Data.Length; i++)
                receivedData[currentIndex++] = args.Data[i];

            // check if echo of command is correct
            for(UInt16 i = 0; i < Math.Min(10, currentIndex); i++)
            {
                if (receivedData[i] != command[i])
                {
                    // Console.WriteLine("Received bad message: {0} {1}", receivedData[4], ByteToHexBitFiddle(args.Data));
                    cancelled = true;
                    closeSerial();
                    return;
                }
            }

            // check expected length (10+1024+1)
            if (currentIndex < 1035)
                return;

            // check correct buffer end
            if (receivedData[currentIndex-1] != (Byte)0x10)
            {
                cancelled = true;
                closeSerial();
                return;
            }

            // Console.WriteLine("Received message: {0} {1}", receivedData[4], ByteToHexBitFiddle(args.Data));

            if (receivedData[8] == 0xf7)
            {
                percentage = 100;
                command = new Byte[] { (Byte)'E', (Byte)'N', (Byte)'D' };
            }
            else
            {
                percentage += (float)2.5;
                command = new Byte[] { 0x68, 0x31, 0x00, 0x01, (Byte)Math.Min((Byte)Math.Round(percentage), (Byte)100), 0xCD, 0x00, 0x04, (Byte)(receivedData[8] + 1), 0x1D, 0x10 };
            }

            SetProgressBarSafe(progressPercent);
            // percent.Text = String.Format("Completion percentage {0}%", GetPercentage);

            currentIndex = 0;
            // Console.WriteLine("Sending message:     {0}", ByteToHexBitFiddle(command));
            serialPort.SendMessage(command);

            if (command.Length == 3)
            {
                DialogResult result = MessageBox.Show("Read OK !", "HD1 GPS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (result == DialogResult.OK)
                {
                }

                closeSerial();
            }
        }

        private void ReadPowerOnLogo(object sender, EventArgs e)
        {
            // 68 31 00 01 02 cd 00 04 d0 1d 10 first  
            // 68 31 00 01 64 cd 00 04 f7 1d 10 last
            // 45 4e 44                         E N D

            percentage = 2.0f;
            cancelled = false;
            command = new Byte[] { 0x68, 0x31, 0x00, 0x01, 0x02, 0xCD, 0x00, 0x04, 0xD0, 0x1D, 0x10 };

            serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
            serialPort.MessageReceived += SerialPort_MessageReceived;
            serialPort.Connect();

            // Try sending some data if connected
            if (serialPort.IsConnected)
            {
                // Console.WriteLine("Sending message:     {0}", ByteToHexBitFiddle(command));
                serialPort.SendMessage(command);
            }
        }

         void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            /*
            if (OnConnecting != null)
                OnConnecting(this, new StateDeviceEventArgs(args.Connected));
            */
            Console.WriteLine("Serial port connection status = {0}", args.Connected);
        }

        private void PowerOnLogo_FormClosed(Object sender, FormClosedEventArgs e)
        {
            closeSerial();
        }
    }
}
