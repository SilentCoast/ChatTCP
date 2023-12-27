using PropertyChanged;

namespace ChatTCP.Classes.Logger
{
    [AddINotifyPropertyChangedInterface]
    public class Logger : ILogger
    {
        public event ILogger.MessageEventHandler Logged;

        public void Log(string message)
        {
            Logged?.Invoke(this, new MessageEventArgs() { Message = message + "\n" });
        }

    }
}
