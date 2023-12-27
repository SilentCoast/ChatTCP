namespace ChatTCP.Classes.Logger
{
    public interface ILogger
    {
        void Log(string message);
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public event MessageEventHandler Logged;
    }
}
