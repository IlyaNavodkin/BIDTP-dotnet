using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CALLBACK_CLIENT
{
    public class NamedPipeServer
    {
        public event EventHandler<string> MessageReceived;

        private readonly string pipeName;
        private readonly int numThreads;

        public NamedPipeServer(string pipeName, int numThreads)
        {
            this.pipeName = pipeName;
            this.numThreads = numThreads;
        }

        public void Start()
        {
            Task[] tasks = new Task[numThreads];

            Console.WriteLine("Named Pipe Server running...");

            for (int i = 0; i < numThreads; i++)
            {
                tasks[i] = Task.Run(() => ServerThread());
            }

            Task.WaitAll(tasks);
        }

        private void ServerThread()
        {
            while (true)
            {

                var threadId = Thread.CurrentThread.ManagedThreadId;

                Console.WriteLine("Starting thread {0}...", threadId);

                var pipeServer = new NamedPipeServerStream
                    (pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);


                pipeServer.WaitForConnection();


                if (threadId % 2 == 0)
                {
                    Console.WriteLine("[AHAHAHAH] Thread {0} sleeping...", threadId);
                    Thread.Sleep(5555);
                }
                else if (threadId % 3 == 0)
                {
                    Console.WriteLine("[SLEEPING] Thread {0} sleeping...", threadId);
                    Thread.Sleep(3444);
                }
 

                var state = new StateObject { Pipe = pipeServer, Buffer = new byte[4] };
                pipeServer.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ReadLengthCallback), state);


            }
        }

        private void ReadLengthCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                int bytesRead = state.Pipe.EndRead(ar);
                if (bytesRead == 4)
                {
                    int messageLength = BitConverter.ToInt32(state.Buffer, 0);
                    state.Buffer = new byte[messageLength];
                    state.BytesRead = 0;
                    state.Pipe.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(ReadMessageCallback), state);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                state.Pipe.Dispose();
            }
        }

        private void ReadMessageCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                int bytesRead = state.Pipe.EndRead(ar);
                if (bytesRead > 0)
                {
                    state.BytesRead += bytesRead;
                    if (state.BytesRead < state.Buffer.Length)
                    {
                        state.Pipe.BeginRead(state.Buffer, state.BytesRead, state.Buffer.Length - state.BytesRead, new AsyncCallback(ReadMessageCallback), state);
                    }
                    else
                    {
                        string message = Encoding.UTF8.GetString(state.Buffer, 0, state.BytesRead);
                        MessageReceived?.Invoke(this, message); // Notify subscribers about the received message

                        // Send response to the client
                        byte[] responseBuffer = Encoding.UTF8.GetBytes("Echo: " + message);
                        byte[] responseLength = BitConverter.GetBytes(responseBuffer.Length);
                        state.Pipe.BeginWrite(responseLength, 0, responseLength.Length, new AsyncCallback(WriteLengthCallback), new PipeState { Pipe = state.Pipe, Buffer = responseBuffer });
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                state.Pipe.Dispose();
            }
        }

        private void WriteLengthCallback(IAsyncResult ar)
        {
            PipeState state = (PipeState)ar.AsyncState;
            try
            {
                state.Pipe.EndWrite(ar);
                state.Pipe.BeginWrite(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(WriteCallback), state);
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                state.Pipe.Dispose();
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            PipeState state = (PipeState)ar.AsyncState;
            try
            {
                var treadId = Thread.CurrentThread.ManagedThreadId;
                state.Pipe.EndWrite(ar);
                state.Pipe.Dispose();
                Console.WriteLine("Thread {0} [DONE] +1.", treadId);
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                state.Pipe.Dispose();
            }
        }
    }
}
