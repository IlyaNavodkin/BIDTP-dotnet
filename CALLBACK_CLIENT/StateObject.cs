using System.IO.Pipes;

class StateObject
{
    public NamedPipeClientStream Pipe { get; set; }
    public byte[] Buffer { get; set; }
    public int BytesRead { get; set; }
    public string ClientId { get; set; }
    public TaskCompletionSource<string> TaskCompletionSource { get; set; }
}
