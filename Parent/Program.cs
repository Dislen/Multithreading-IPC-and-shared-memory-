using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

class ParentProcess
{
    private const int childCount = 9; 
    private const int timeLimit = 10; 
    private static string sharedMemoryFile = "/tmp/shared_memory.txt";
    private static Process[] childBox = new Process[childCount];

    static void Main()
    {
        File.WriteAllText(sharedMemoryFile, "0");

        for (int i = 0; i < childCount; i++)
        {
            string pipeName = $"ChildPipe{i}";
            Process child = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project ../Child/Child.csproj {pipeName}",
                    RedirectStandardOutput = false,
                    UseShellExecute = true
                }
            };
            child.Start();
            childBox[i] = child;
        }

        Thread monitorThread = new Thread(monitorChildren);
        monitorThread.Start();

        for (int i = 0; i < childCount; i++)
        {
            string pipeName = $"ChildPipe{i}";
            using NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In);
            pipeServer.WaitForConnection();
            using StreamReader reader = new StreamReader(pipeServer);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine($"Parent received: {line} from Child {i}");
            }
        }

        int finalCounter = int.Parse(File.ReadAllText(sharedMemoryFile));
        Console.WriteLine($"Final shared counter value: {finalCounter}");

        monitorThread.Join();
    }

    static void monitorChildren()
    {
        while (true)
        {
            Thread.Sleep(1000);

            for (int i = 0; i < childBox.Length; i++)
            {
                Process child = childBox[i];

                if (child == null || child.HasExited)
                    continue;

                TimeSpan runtime = DateTime.Now - child.StartTime;
                if (runtime.TotalSeconds > timeLimit)
                {
                    Console.WriteLine($"Terminating process {child.Id} exceeded {timeLimit} seconds");
                try
                {
                    child.Kill();
                    child.WaitForExit();
                    childBox[i] = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to terminate process {child.Id}: {ex.Message}");
                }
                }
            }
        }
    }
}
