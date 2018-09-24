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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using SerialPortLib2;

namespace hd1sharp.Resources
{
    public partial class PowerOnLogo : Form
    {
        String defaultPort = "COM3";
        float percentage = 0.0f;
        Int32 progressPercent = 0;
        Byte[] readCommand;
        Byte[] sendCommand;
        SerialPortInput serialPort;
        Boolean cancelled = false;
        int currentReceiveIndex = 0;
        int currentSendIndex = 0;
        Byte[] receivedData = new byte[1200];
        Byte[] bmpWriteBuffer = null;

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
        String ByteToHexBitFiddle(byte[] bytes)
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

        String GetPercentage
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
                serialPort.MessageReceived -= SerialPort_WriteMessageReceived;

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
                receivedData[currentReceiveIndex++] = args.Data[i];

            // check if echo of command is correct
            for(UInt16 i = 0; i < Math.Min(10, currentReceiveIndex); i++)
            {
                if (receivedData[i] != readCommand[i])
                {
                    // Console.WriteLine("Received bad message: {0} {1}", receivedData[4], ByteToHexBitFiddle(args.Data));
                    cancelled = true;
                    closeSerial();
                    return;
                }
            }

            // check expected length (10+1024+1)
            if (currentReceiveIndex < 1035)
                return;

            // check correct buffer end
            if (receivedData[currentReceiveIndex-1] != (Byte)0x10)
            {
                cancelled = true;
                closeSerial();
                return;
            }

            // Console.WriteLine("Received message: {0} {1}", receivedData[4], ByteToHexBitFiddle(args.Data));

            if (receivedData[8] == 0xf7)
            {
                percentage = 100;
                readCommand = new Byte[] { (Byte)'E', (Byte)'N', (Byte)'D' };
            }
            else
            {
                percentage += (float)2.5;
                readCommand = new Byte[] { 0x68, 0x31, 0x00, 0x01, (Byte)Math.Min((Byte)Math.Round(percentage), (Byte)100), 0xCD, 0x00, 0x04, (Byte)(receivedData[8] + 1), 0x1D, 0x10 };
            }

            SetProgressBarSafe(progressPercent);
            // percent.Text = String.Format("Completion percentage {0}%", GetPercentage);

            currentReceiveIndex = 0;
            // Console.WriteLine("Sending message:     {0}", ByteToHexBitFiddle(readCommand));
            serialPort.SendMessage(readCommand);

            if (readCommand.Length == 3)
            {
                DialogResult result = MessageBox.Show("Read OK !", "HD1 GPS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (result == DialogResult.OK)
                {
                }

                closeSerial();
            }
        }

        // 68 31 01 01 0a 31 00 10 d0 1d 10 // write bmp

        void ReadPowerOnLogo(object sender, EventArgs e)
        {
            // 68 31 00 01 02 cd 00 04 d0 1d 10 first  
            // 68 31 00 01 64 cd 00 04 f7 1d 10 last
            // 45 4e 44                         E N D

            percentage = 2.0f;
            cancelled = false;
            readCommand = new Byte[] { 0x68, 0x31, 0x00, 0x01, 0x02, 0xCD, 0x00, 0x04, 0xD0, 0x1D, 0x10 };

            serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
            serialPort.MessageReceived += SerialPort_MessageReceived;
            serialPort.Connect();

            // Try sending some data if connected
            if (serialPort.IsConnected)
            {
                // Console.WriteLine("Sending message:     {0}", ByteToHexBitFiddle(redCommand));
                serialPort.SendMessage(readCommand);
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

        void PowerOnLogo_FormClosed(Object sender, FormClosedEventArgs e)
        {
            closeSerial();
        }

        void PowerOnLogoOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = ".";
            openFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Open Bitmap File";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    // Retrieve the image.
                    Bitmap image = new Bitmap(openFileDialog.FileName, true);
                    SolidBrush brush = new SolidBrush(Color.White);

                    float width = 160;
                    float height = 128;
                    float scale = Math.Min(width / image.Width, height / image.Height);

                    Bitmap bmp = new Bitmap((UInt16)width, (UInt16)height);
                    Graphics graph = Graphics.FromImage(bmp);

                    // uncomment for higher quality output
                    graph.InterpolationMode = InterpolationMode.High;
                    graph.CompositingQuality = CompositingQuality.HighQuality;
                    graph.SmoothingMode = SmoothingMode.AntiAlias;

                    UInt16 scaleWidth = (UInt16)(image.Width * scale);
                    UInt16 scaleHeight = (UInt16)(image.Height * scale);

                    graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                    graph.DrawImage(image, ((UInt16)width - scaleWidth) / 2, ((UInt16)height - scaleHeight) / 2, scaleWidth, scaleHeight);

                    // Set the PictureBox to display the image.
                    PowerOnLogoPicture.Image = bmp;
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("There was an error." + "Check the path to the image file.");
                }
            }
        }
        
        T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        void SerialPort_WriteMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (cancelled)
            {
                closeSerial();
                return;
            }

            // get received data
            for (UInt16 i = 0; i < args.Data.Length; i++)
                receivedData[currentReceiveIndex++] = args.Data[i];

            // check if echo of command is correct
            for (UInt16 i = 0; i < Math.Min(10, currentReceiveIndex); i++)
            {
                if (receivedData[i] != sendCommand[i])
                {
                    // Console.WriteLine("Received bad message: {0} {1}", receivedData[4], ByteToHexBitFiddle(args.Data));
                    cancelled = true;
                    closeSerial();
                    return;
                }
            }

            // check expected length (10)
            if (currentReceiveIndex < 10)
                return;

            if (currentSendIndex >= bmpWriteBuffer.Length)
            {
                percentage = 100;
                sendCommand = new Byte[] { (Byte)'E', (Byte)'N', (Byte)'D' };
            }
            else
            {
                percentage = (float)(currentSendIndex+4096)/bmpWriteBuffer.Length*100;
                sendCommand = new Byte[] { 0x68, 0x31, 0x01, 0x01, (Byte)Math.Min((Byte)Math.Round(percentage), (Byte)100), 0x31, 0x00, 0x10, (Byte)(receivedData[8] + 4), 0x1D };
                Byte[] sb = new byte[4107];
                Array.Copy(sendCommand, 0, sb, 0, 10);
                Array.Copy(bmpWriteBuffer, currentSendIndex, sb, 10, 4096);
                sb[4107 - 1] = 0x10;
                sendCommand = sb;

                currentSendIndex += 4096;

                // Console.WriteLine("Sending message: {0}", percentage);
            }

            currentReceiveIndex = 0;

            serialPort.SendMessage(sendCommand);

            if (sendCommand.Length == 3)
            {
                DialogResult result = MessageBox.Show("Write OK !", "HD1 GPS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (result == DialogResult.OK)
                {
                }

                closeSerial();
            }
        }

        private byte[] GetRGBValues(Bitmap bmp)
        {

            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);

            return rgbValues;
        }

        public static Bitmap ConvertTo16bpp(Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            using (Graphics gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        void PowerOnLogoWrite(Object sender, EventArgs e)
        {
            bmpWriteBuffer = GetRGBValues(ConvertTo16bpp(PowerOnLogoPicture.Image));

            percentage = 0.0f;
            cancelled = false;
            currentSendIndex = 0;
            sendCommand = new Byte[] { 0x68, 0x31, 0x01, 0x01, 0x0a, 0x31, 0x00, 0x10, 0xD0, 0x1D };

            serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
            serialPort.MessageReceived += SerialPort_WriteMessageReceived;
            serialPort.Connect();

            // Try sending some data if connected
            if (serialPort.IsConnected)
            {
                // Console.WriteLine("Sending message:     {0} {1}", ByteToHexBitFiddle(sendCommand), ByteToHexBitFiddle(SubArray(bmpWriteBuffer, currentSendIndex, 4096)));

                percentage = (float)(currentSendIndex+4096) / bmpWriteBuffer.Length;
                sendCommand = new Byte[] { 0x68, 0x31, 0x01, 0x01, (Byte)Math.Min((Byte)Math.Round(percentage), (Byte)100), 0x31, 0x00, 0x10, 0xD0, 0x1D };
                Byte[] sb = new byte[4107];
                Array.Copy(sendCommand, 0, sb, 0, 10);
                Array.Copy(bmpWriteBuffer, currentSendIndex, sb, 10, 4096);
                sb[4107 - 1] = 0x10;
                sendCommand = sb;
                serialPort.SendMessage(sendCommand);
                currentSendIndex += 4096;
            }
        }
    }
}
