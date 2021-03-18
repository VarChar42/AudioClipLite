using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Utils;
using NAudio.Wave;

namespace AudioBuffer
{
    class Program
    {

        private static CircularBuffer buffer;

        static void Main(string[] args)
        {

            var deviceEnumerator = new MMDeviceEnumerator();
            var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();
            
            for (var i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($" {i} - {devices[i].FriendlyName}");
            }

            var id = int.Parse(Console.ReadLine());

            var gaming = new WasapiLoopbackCapture(devices[id]);

            int bufferLen = gaming.WaveFormat.AverageBytesPerSecond * 60 * 5;
            Console.WriteLine(bufferLen);
           

            buffer = new CircularBuffer(bufferLen);
            buffer.Reset();

            //var gaming = new WaveInEvent();
            //gaming.DeviceNumber = device;

            gaming.DataAvailable += Handle;

            
            gaming.StartRecording();

            Console.Read();


            gaming.StopRecording();

            var writer = new WaveFileWriter("out.wav", gaming.WaveFormat);

            var writeBuffer = new byte[1024];
            var len = 0;

            while ((len = buffer.Read(writeBuffer, 0, writeBuffer.Length)) > 0)
            {
                writer.Write(writeBuffer, 0, len);
            }

            writer.Close();
            gaming.Dispose();
        }

        private static void Handle(object sender, WaveInEventArgs e)
        {
            buffer.Write(e.Buffer, 0, e.BytesRecorded);
            Console.WriteLine(e.BytesRecorded);
        }
    }
}
