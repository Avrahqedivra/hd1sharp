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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using SerialPortLib2;

namespace hd1sharp.Resources
{
    public partial class PowerOnLogo : Form
    {
        private string defaultPort = "COM1";
        private static SerialPortInput serialPort;

        public PowerOnLogo()
        {
            InitializeComponent();
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        private void ReadPowerOnLogo(object sender, EventArgs e)
        {
            serialPort = new SerialPortInput();
            serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
            serialPort.MessageReceived += SerialPort_MessageReceived;

            while (true)
            {
                if (String.IsNullOrWhiteSpace(defaultPort))
                    break;

                serialPort.SetPort(defaultPort, 19200);
                serialPort.Connect();

                while (!serialPort.IsConnected)
                {
                    Console.Write(".");
                    Thread.Sleep(250);
                }

                var command = new byte[] { 0x68, 0x31, 0x00, 0x01, 0x02, 0xCD, 0x00, 0x04, 0xD0, 0x1D };
                // Try sending some data if connected
                if (serialPort.IsConnected)
                {
                    Console.WriteLine("Sending message: {0}", ByteToHexBitFiddle(command));
                    serialPort.SendMessage(command);
                }
                Console.WriteLine("\nTest sequence completed, now disconnecting.");

                serialPort.Disconnect();

                Thread.Sleep(2500);
                break;
            }

            serialPort.ConnectionStatusChanged -= SerialPort_ConnectionStatusChanged;
            serialPort.MessageReceived -= SerialPort_MessageReceived;
        }

        static void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Console.WriteLine("Received message: {0}", ByteToHexBitFiddle(args.Data));
        }

        static void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            Console.WriteLine("Serial port connection status = {0}", args.Connected);
        }
    }
}
