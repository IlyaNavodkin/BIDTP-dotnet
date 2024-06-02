using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace CALLBACK_CLIENT
{
    public class CallBackPipeClient
    {
        public event EventHandler<string> MessageReceived;

        public async Task<string> SendMessage(string clientId)
        {
            var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous);
            Console.WriteLine($"Client {clientId} connecting to server...");

            await pipeClient.ConnectAsync();

            Console.WriteLine($"Client {clientId} connected to server.");

            // Send a message to the server
            var message = $"Hello from Client {clientId}!";
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

            // Create a task completion source to await the response
            var tcs = new TaskCompletionSource<string>();

            // Send length first, then the message
            var state = new PipeState { Pipe = pipeClient, Buffer = messageBuffer, ClientId = clientId, TaskCompletionSource = tcs };

            pipeClient.BeginWrite(lengthBuffer, 0, lengthBuffer.Length, new AsyncCallback(WriteLengthCallback), state);

            // Wait for the response and return it
            return await tcs.Task;
        }

        private void WriteLengthCallback(IAsyncResult ar)
        {
            var state = (PipeState)ar.AsyncState;
            try
            {
                state.Pipe.EndWrite(ar);
                state.Pipe.BeginWrite(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(WriteMessageCallback), state);
            }
            catch (IOException e)
            {
                Console.WriteLine("Client {0} ERROR: {1}", state.ClientId, e.Message);
                state.TaskCompletionSource.SetException(e);
            }
        }

        private void WriteMessageCallback(IAsyncResult ar)
        {
            PipeState state = (PipeState)ar.AsyncState;
            try
            {
                state.Pipe.EndWrite(ar);

                // Read response length from the server
                StateObject newState = new StateObject { Pipe = state.Pipe, Buffer = new byte[4], ClientId = state.ClientId, TaskCompletionSource = state.TaskCompletionSource };
                state.Pipe.BeginRead(newState.Buffer, 0, newState.Buffer.Length, new AsyncCallback(ReadLengthCallback), newState);
            }
            catch (IOException e)
            {
                Console.WriteLine("Client {0} ERROR: {1}", state.ClientId, e.Message);
                state.TaskCompletionSource.SetException(e);
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
                Console.WriteLine("Client {0} ERROR: {1}", state.ClientId, e.Message);
                state.TaskCompletionSource.SetException(e);
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
                        string response = Encoding.UTF8.GetString(state.Buffer, 0, state.BytesRead);
                        Console.WriteLine($"Client {state.ClientId} received from server: {response}");
                        MessageReceived?.Invoke(this, response); // Notify subscribers about the received message
                        state.TaskCompletionSource.SetResult(response); // Set the result of the task
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Client {0} ERROR: {1}", state.ClientId, e.Message);
                state.TaskCompletionSource.SetException(e);
            }
        }
    }
}
