using CALLBACK_CLIENT;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;

class PipeServer
{
    private static int numThreads = 5;

    static void Main()
    {
        var server = new NamedPipeServer("testpipe", numThreads);

        server.Start();

        Console.ReadKey();
    }

}

class StateObject
{
    public NamedPipeServerStream Pipe { get; set; }
    public byte[] Buffer { get; set; }
    public int BytesRead { get; set; }
}

class PipeState
{
    public NamedPipeServerStream Pipe { get; set; }
    public byte[] Buffer { get; set; }
}
