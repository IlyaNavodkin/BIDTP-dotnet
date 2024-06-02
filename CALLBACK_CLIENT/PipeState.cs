using System.IO.Pipes;

    public class PipeState
    {
        public NamedPipeClientStream Pipe { get; set; }
        public byte[] Buffer { get; set; }
        public string ClientId { get; set; }
        public TaskCompletionSource<string> TaskCompletionSource { get; set; } // Добавленное свойство

        // Дополнительные свойства и методы класса...
    }
