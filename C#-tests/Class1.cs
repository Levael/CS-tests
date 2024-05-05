using NAudio.CoreAudioApi;
using NAudio.Wasapi;
using NAudio.Wave;
using System;
using System.IO.Ports;
using System.Management;

namespace G711MicStream
{
    /*class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //Mic2Speaker();
            //Song2Speaker();
            Cedrus();
        }





        //TODO: put ID to config file
        static void Cedrus()
        {
            string deviceId = @"FTDIBUS\VID_0403+PID_6001"; // Cedrus ID (common to all devices)
            string portName = FindPortByDeviceId(deviceId);

            if (!string.IsNullOrEmpty(portName))
            {
                Console.WriteLine($"Post number: {portName}");
            }
            else
            {
                Console.WriteLine("Device is not found.");
            }

            using (SerialPort serialPort = new SerialPort(portName))
            {
                serialPort.BaudRate = 9600; // Установите нужную скорость передачи данных
                serialPort.DataBits = 8;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.Handshake = Handshake.None;

                serialPort.DataReceived += SerialPort_DataReceived;

                serialPort.Open(); // Открыть порт

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(); // Ожидание нажатия клавиши для выхода

                serialPort.Close(); // Закрыть порт
            }

        }

        *//*private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            byte[] buffer = new byte[sp.BytesToRead];
            sp.Read(buffer, 0, buffer.Length);

            foreach (byte b in buffer)
            {
                Console.WriteLine($"Byte: {b}");
            }
        }*//*

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead; // Получаем количество доступных для чтения байтов
            Console.WriteLine($"Доступно байт для чтения: {bytesToRead}");

            byte[] buffer = new byte[bytesToRead]; // Создаем буфер под все доступные байты
            sp.Read(buffer, 0, bytesToRead); // Читаем все доступные байты

            Console.WriteLine("Получены данные: " + BitConverter.ToString(buffer));

            *//*SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead; // Получаем количество доступных для чтения байтов

            // Проверяем, достаточно ли байтов для чтения
            if (bytesToRead >= 6)
            {
                byte[] buffer = new byte[6]; // Буфер для 6 байтов
                int bytesRead = sp.Read(buffer, 0, 6); // Читаем ровно 6 байтов
                if (bytesRead == 6)
                {
                    // Обработка данных здесь
                    Console.WriteLine("Получены данные: " + BitConverter.ToString(buffer));
                    // Далее, вы можете декодировать сообщение аналогично примеру MATLAB
                }
            }*//*
        }


        static string FindPortByDeviceId(string deviceId)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0"))
            {
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["PNPDeviceID"].ToString().Contains(deviceId))
                    {
                        var nameProperty = queryObj["Name"].ToString(); // For example "USB Serial Port (COM6)"
                        if (nameProperty.Contains("(COM"))
                        {
                            // Get Port name. Will be look like "COM5"
                            return nameProperty.Substring(nameProperty.LastIndexOf("(COM")).Replace("(", "").Replace(")", "");
                        }
                    }
                }
            }
            return null;
        }











        *//*string desiredPortName = "ft232r usb uart"; // Имя устройства для поиска
        string connectedPortName = null;

        foreach (string portName in SerialPort.GetPortNames())
        {
            using (var serialPort = new SerialPort(portName))
            {
                try
                {
                    serialPort.Open(); // Попытка открыть порт
                    if (serialPort.Description.Contains(desiredPortName))
                    {
                        connectedPortName = portName;
                        break;
                    }
                    serialPort.Close();
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }

        if (connectedPortName != null)
        {
            Console.WriteLine($"Устройство '{desiredPortName}' подключено к порту: {connectedPortName}");
            // Можно продолжить работу с портом...
        }
        else
        {
            Console.WriteLine($"Устройство '{desiredPortName}' не найдено.");
        }*//*
    



        static void Song2Speaker()
        {
            *//*var enumerator = new MMDeviceEnumerator();
            MMDeviceCollection outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            MMDevice outputDevice = outputDevices[1];

            var audioOutput = new WasapiOut(outputDevice, AudioClientShareMode.Shared, false, 10);
            // number here is size of buffer in ms (less -- faster, but more chance of artifacts)

            
            audioOutput.Init(new AudioFileReader("C:\\Users\\Levael\\GitHub\\MOCU\\Assets\\Audio\\audioTestSample.mp3"));
            audioOutput.Play();

            Thread.Sleep(2000);

            audioOutput = new WasapiOut(outputDevice, AudioClientShareMode.Shared, false, 10);


            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();*//*

        }



        static void Mic2Speaker()
        {
            var enumerator = new MMDeviceEnumerator();
            MMDeviceCollection outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            MMDeviceCollection inputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            MMDevice outputDevice = outputDevices[3];
            MMDevice inputDevice = inputDevices[1];

            var audioInput = new WasapiCapture(inputDevice);
            var audioOutput = new WasapiOut(outputDevice, AudioClientShareMode.Shared, false, 10);
            // number here is size of buffer in ms (less -- faster, but more chance of artifacts)

            var buffer = new BufferedWaveProvider(audioInput.WaveFormat);
            audioOutput.Init(buffer);

            audioInput.DataAvailable += (sender, e) =>
            {
                buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
            };

            audioInput.StartRecording();
            audioOutput.Play();



            Console.WriteLine($"Output devices:");
            foreach (var audioDevice in outputDevices)
            {
                Console.WriteLine($"\t{audioDevice.FriendlyName}");
            }

            Console.WriteLine($"\nInput devices");
            foreach (var audioDevice in inputDevices)
            {
                Console.WriteLine($"\t{audioDevice.FriendlyName}");
            }

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();

            audioInput.StopRecording();
            audioOutput.Stop();
        }
    }*/

}


//var audioDevices = new MMDeviceEnumerator();

/*Console.WriteLine($"Output devices:");
foreach (var audioDevice in audioDevices.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
{
    Console.WriteLine($"\t{audioDevice.FriendlyName}");
}

Console.WriteLine($"\nInput devices");
foreach (var audioDevice in audioDevices.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
{
    Console.WriteLine($"\t{audioDevice.FriendlyName}");
}*/


/*var audioInput = new WasapiCapture();
audioInput.StartRecording();

var outputDevice = audioDevices.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
var audioOutput = new WasapiOut(outputDevice, AudioClientShareMode.Shared, false, 10);   // number here is size of buffer in ms (less -- faster, but more chance of artifacts)

var buffer = new BufferedWaveProvider(audioInput.WaveFormat);
audioOutput.Init(buffer);
audioOutput.Play();

audioInput.DataAvailable += (sender, e) =>
{
    buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
};


Console.WriteLine("\n\nPress any key to exit...");
Console.ReadKey();

audioInput.StopRecording();
audioOutput.Stop();*/



/*var audioDevices = new MMDeviceEnumerator();

Console.WriteLine($"Input devices:");
foreach (var audioDevice in audioDevices.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
{
    Console.WriteLine($"\t{audioDevice.FriendlyName}");
}

Console.WriteLine($"\nOutput devices");
foreach (var audioDevice in audioDevices.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
{
    Console.WriteLine($"\t{audioDevice.FriendlyName}");
}


var audioInput = new WasapiCapture();
audioInput.StartRecording();

var outputDevice = audioDevices.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
var audioOutput = new WasapiOut(outputDevice, AudioClientShareMode.Shared, false, 10);   // number here is size of buffer in ms (less -- faster, but more chance of artifacts)

var buffer = new BufferedWaveProvider(audioInput.WaveFormat);
audioOutput.Init(buffer);
audioOutput.Play();

audioInput.DataAvailable += (sender, e) =>
{
    buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
};


Console.WriteLine("\n\nPress any key to exit...");
Console.ReadKey();

audioInput.StopRecording();
audioOutput.Stop();*/











// Получение и вывод списка устройств ввода (микрофонов)
/*for (int n = 0; n < WaveInEvent.DeviceCount; n++)
{
    var capabilities = WaveInEvent.GetCapabilities(n);
    Console.WriteLine($"Input device {n}: {capabilities.ProductName}");
}*/

// Получение и вывод списка устройств вывода (колонок, наушников и т.д.)
/*for (int n = 0; n < WaveOutEvent.DeviceCount; n++)
{
    var capabilities = WaveOutEvent.GetCapabilities(n);
    Console.WriteLine($"Output device {n}: {capabilities.ProductName}");
}*/







/*
Console.WriteLine("Input devices:");
for (int deviceId = 0; deviceId < MidiIn.NumberOfDevices; deviceId++)
{
    Console.WriteLine($"{deviceId}: {MidiIn.DeviceInfo(deviceId).ProductName}");
}

Console.WriteLine("Output devices:");
for (int deviceId = 0; deviceId < MidiOut.NumberOfDevices; deviceId++)
{
    Console.WriteLine($"{deviceId}: {MidiOut.DeviceInfo(deviceId).ProductName}");
}

*//*Console.WriteLine("\nOutput devices:");
for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
{
    WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(deviceId);
    Console.WriteLine($"{deviceId}: {deviceInfo.ProductName}");
}*//*

Console.WriteLine("\n\nPress any key to exit...");
Console.ReadKey();*/





/*int inputDeviceIndex;
int outputDeviceIndex;
var numberOfDevices = AsioOut.GetDriverNames();

*//*Console.WriteLine($"Choose input:");
int.TryParse(Console.ReadLine(), out inputDeviceIndex);

Console.WriteLine($"Choose output:");
int.TryParse(Console.ReadLine(), out outputDeviceIndex);*//*

foreach (string deviceName in AsioOut.GetDriverNames())
{
    Console.WriteLine(deviceName);
}*/


/*int inputDeviceIndex;
int outputDeviceIndex;

Console.WriteLine($"Choose input. Total number is {WaveInEvent.DeviceCount}:");
int.TryParse(Console.ReadLine(), out inputDeviceIndex);

Console.WriteLine($"Choose output. Total number is {WaveOutEvent.DeviceCount}:");
int.TryParse(Console.ReadLine(), out outputDeviceIndex);

Console.WriteLine($"inputDeviceIndex: {inputDeviceIndex}, outputDeviceIndex: {outputDeviceIndex}. Technology using: NAudio.Wave");




var waveIn = new WaveInEvent
{
    DeviceNumber = inputDeviceIndex,
    WaveFormat = new WaveFormat(16000, 16, 1) // Формат: 16kHz, 16 бит, моно
};

var waveOut = new WaveOutEvent
{
    DeviceNumber = outputDeviceIndex
};

var buffer = new BufferedWaveProvider(waveIn.WaveFormat);
waveOut.Init(buffer);
waveOut.Play();

waveIn.DataAvailable += (sender, e) =>
{
    buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
};

waveIn.StartRecording();

Console.WriteLine("\n\nPress any key to exit...");
Console.ReadKey();

waveIn.StopRecording();
waveOut.Stop();*/



















/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NAudio.Wave;
using NAudio.Codecs;

namespace G711MicStream
{
    class Program
    {
        delegate byte EncoderMethod(short _raw);
        delegate short DecoderMethod(byte _encoded);

        // Change these to their ALaw equivalent if you want.
        static EncoderMethod Encoder = MuLawEncoder.LinearToMuLawSample;
        static DecoderMethod Decoder = MuLawDecoder.MuLawToLinearSample;



        static void Main(string[] args)
        {
            // Fire off our Sender thread.
            Thread sender = new Thread(new ThreadStart(Sender));
            sender.Start();

            // And receiver...
            Thread receiver = new Thread(new ThreadStart(Receiver));
            receiver.Start();

            // We're going to try for 16-bit PCM, 8KHz sampling, 1 channel.
            // This should align nicely with u-law
            CommonFormat = new WaveFormat(16000, 16, 1);

            // Prep the input.
            IWaveIn wavein = new WaveInEvent();
            wavein.WaveFormat = CommonFormat;
            wavein.DataAvailable += new EventHandler<WaveInEventArgs>(wavein_DataAvailable);
            wavein.StartRecording();

            // Prep the output.  The Provider gets the same formatting.
            WaveOutEvent waveout = new WaveOutEvent();
            OutProvider = new BufferedWaveProvider(CommonFormat);
            waveout.Init(OutProvider);
            waveout.Play();


            // Now we can just run until the user hits the <X> button.
            Console.WriteLine("Running g.711 audio test.  Hit <X> to quit.");
            for (; ; )
            {
                Thread.Sleep(100);
                if (!Console.KeyAvailable) continue;
                ConsoleKeyInfo info = Console.ReadKey(false);
                if ((info.Modifiers & ConsoleModifiers.Alt) != 0) continue;
                if ((info.Modifiers & ConsoleModifiers.Control) != 0) continue;

                // Quit looping on non-Alt, non-Ctrl X
                if (info.Key == ConsoleKey.X) break;
            }

            Console.WriteLine("Stopping...");

            // Shut down the mic and kick the thread semaphore (without putting
            // anything in the queue).  This will (eventually) stop the thread
            // (which also signals the receiver thread to stop).
            wavein.StopRecording();
            try { wavein.Dispose(); } catch (Exception) { }
            SenderKick.Release();

            // Wait for both threads to exit.
            sender.Join();
            receiver.Join();

            // And close down the output.
            waveout.Stop();
            try { waveout.Dispose(); } catch (Exception) { }

            // Sleep a little.  This seems to be accepted practice when shutting
            // down these audio components.
            Thread.Sleep(500);
        }


        /// <summary>
        /// Grabs the mic data and just queues it up for the Sender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void wavein_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Create a local copy buffer.
            byte[] buffer = new byte[e.BytesRecorded];
            System.Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

            // Drop it into the queue.  We'll need to lock for this.
            Lock.WaitOne();
            SenderQueue.AddLast(buffer);
            Lock.ReleaseMutex();

            // and kick the thread.
            SenderKick.Release();
        }


        static
        void
        Sender()
        {
            // Holds the data from the DataAvailable event.
            byte[] qbuffer = null;

            for (; ; )
            {
                // Wait for a 'kick'...
                SenderKick.WaitOne();

                // Lock...
                Lock.WaitOne();
                bool dataavailable = (SenderQueue.Count != 0);
                if (dataavailable)
                {
                    qbuffer = SenderQueue.First.Value;
                    SenderQueue.RemoveFirst();
                }
                Lock.ReleaseMutex();

                // If the queue was empty on a kick, then that's our signal to
                // exit.
                if (!dataavailable) break;

                // Convert each 16-bit PCM sample to its 1-byte u-law equivalent.
                int numsamples = qbuffer.Length / sizeof(short);
                byte[] g711buff = new byte[numsamples];

                // I like unsafe for this kind of stuff!
                unsafe
                {
                    fixed (byte* inbytes = &qbuffer[0])
                    fixed (byte* outbytes = &g711buff[0])
                    {
                        // Recast input buffer to short[]
                        short* buff = (short*)inbytes;

                        // And loop over the samples.  Since both input and
                        // output are 16-bit, we can use the same index.
                        for (int index = 0; index < numsamples; ++index)
                        {
                            outbytes[index] = Encoder(buff[index]);
                        }
                    }
                }

                // This gets passed off to the reciver.  We'll queue it for now.
                Lock.WaitOne();
                ReceiverQueue.AddLast(g711buff);
                Lock.ReleaseMutex();
                ReceiverKick.Release();
            }

            // Log it.  We'll also kick the receiver (with no queue addition)
            // to force it to exit.
            Console.WriteLine("Sender: Exiting.");
            ReceiverKick.Release();
        }

        static
        void
        Receiver()
        {
            byte[] qbuffer = null;
            for (; ; )
            {
                // Wait for a 'kick'...
                ReceiverKick.WaitOne();

                // Lock...
                Lock.WaitOne();
                bool dataavailable = (ReceiverQueue.Count != 0);
                if (dataavailable)
                {
                    qbuffer = ReceiverQueue.First.Value;
                    ReceiverQueue.RemoveFirst();
                }
                Lock.ReleaseMutex();

                // Exit on kick with no data.
                if (!dataavailable) break;

                // As above, but we convert in reverse, from 1-byte u-law
                // samples to 2-byte PCM samples.
                int numsamples = qbuffer.Length;
                byte[] outbuff = new byte[qbuffer.Length * 2];
                unsafe
                {
                    fixed (byte* inbytes = &qbuffer[0])
                    fixed (byte* outbytes = &outbuff[0])
                    {
                        // Recast the output to short[]
                        short* outpcm = (short*)outbytes;

                        // And loop over the u-las samples.
                        for (int index = 0; index < numsamples; ++index)
                        {
                            outpcm[index] = Decoder(inbytes[index]);
                        }
                    }
                }

                // And write the output buffer to the Provider buffer for the
                // WaveOut devices.
                OutProvider.AddSamples(outbuff, 0, outbuff.Length);
            }

            Console.Write("Receiver: Exiting.");
        }


        /// <summary>Lock for the sender queue.</summary>
        static Mutex Lock = new Mutex();

        static WaveFormat CommonFormat;

        /// <summary>"Kick" semaphore for the sender queue.</summary>
        static Semaphore SenderKick = new Semaphore(0, int.MaxValue);
        /// <summary>Queue of byte buffers from the DataAvailable event.</summary>
        static LinkedList<byte[]> SenderQueue = new LinkedList<byte[]>();

        static Semaphore ReceiverKick = new Semaphore(0, int.MaxValue);
        static LinkedList<byte[]> ReceiverQueue = new LinkedList<byte[]>();

        /// <summary>WaveProvider for the output.</summary>
        static BufferedWaveProvider OutProvider;
    }
}*/