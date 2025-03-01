using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

class ChildProcess
{
    private static string sharedMemoryFile = "/tmp/shared_memory.txt";
    private static Mutex mutex = new Mutex(false, "Global\\SharedMemoryMutex");

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("No pipe name provided.");
            return;
        }

        string pipeName = args[0];

        using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        pipeClient.Connect();
        using StreamWriter writer = new StreamWriter(pipeClient) { AutoFlush = true };

        for (int i = 0; i < 5; i++)
        {
            mutex.WaitOne();

            try
            {
                int counter = int.Parse(File.ReadAllText(sharedMemoryFile));
                counter++;
                File.WriteAllText(sharedMemoryFile, counter.ToString());
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            writer.WriteLine($"Counter {i}");
            Thread.Sleep(1000); 
        }
    }
}


