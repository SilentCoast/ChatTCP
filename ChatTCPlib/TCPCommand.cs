namespace ChatTCPlib
{
    //TODO: Use PascalCase
    public enum TCPCommand
    {
        message,
        disconnect,
        /// <summary>
        /// called on Client to server to request connection status
        /// </summary>
        connectionCheck
    }
}
