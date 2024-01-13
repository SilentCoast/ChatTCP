using ChatTCPlib;
using ChatTCPlib.Logger;
using ChatTCPlib.TCP;
using Gtk;

public class MainWindow : Window
{
    ILogger ConsoleLogger { get; set; }
    ILogger MessageLogger { get; set; }

    ClientObject Client { get; set; }
    ServerObject Server { get; set; }

    public bool IsConnected
    {
        get => isConnected; set
        {
            isConnected = value;
            IsConnectedChanged();
        }
    }

    private void IsConnectedChanged()
    {
        btnSendMessage.Sensitive = IsConnected;
        if(IsConnected)
        {
            btnConnect.Label = "Disconnect";
        }
        else
        {
            btnConnect.Label = "Connect";
        }
    }

    private bool isServer;
    public bool IsServer { get => isServer; set { isServer = value; IsServerChanged(); } }
    public string ServerIp { get; set; }

    Label ConsoleLabel;
    Label MessagesLabel;
    CheckButton btnIsHost;
    Button btnConnect;
    Button btnSendMessage;
    TextView txtMessage;
    TextView txtServerIp;
    private bool isConnected;

    public MainWindow() : base("MainWindow")
    {
        var builder = new Builder();
        builder.AddFromFile("../../../Resources/ChatTCP.glade");

        var mainWindow = builder.GetObject("mainWindow") as Gtk.Window;

        builder.Autoconnect(this);

        mainWindow.SetDefaultSize(700, 500);
        mainWindow.Show();
        mainWindow.DeleteEvent += MainWindow_DeleteEvent;

        ConsoleLabel = (Label)builder.GetObject("ConsoleLabel");
        MessagesLabel = (Label)builder.GetObject("MessagesLabel");


        btnIsHost = (CheckButton)builder.GetObject("btnIsHost");
        btnIsHost.Toggled += BtnIsHost_Toggled;

        btnConnect = (Button)builder.GetObject("btnConnect");
        btnConnect.Clicked += BtnConnect_Clicked;

        btnSendMessage = (Button)builder.GetObject("btnSend");
        btnSendMessage.Clicked += BtnSendMessage_Clicked;

        txtMessage = (TextView)builder.GetObject("txtMessage");
        txtServerIp = (TextView)builder.GetObject("txtServerIp");



        ConsoleLogger = new Logger();
        ConsoleLogger.Logged += ConsoleLogger_Logged;
        MessageLogger = new Logger();
        MessageLogger.Logged += MessageLogger_Logged;
        CreateNewClient();
        IsConnectedChanged();
    }

    private void MainWindow_DeleteEvent(object o, DeleteEventArgs args)
    {
        Application.Quit();
        args.RetVal = true;
    }

    private void BtnSendMessage_Clicked(object? sender, EventArgs e)
    {
        SendMessage();
    }

    private void BtnConnect_Clicked(object? sender, EventArgs e)
    {
        Connect();
    }

    private void CreateNewClient()
    {
        Client = new ClientObject(ConsoleLogger, MessageLogger);
        Client.Disconnected += Client_Disconnected;
        Client.TryReconnect += Client_TryRecconect;
        Client.ConnectionLostEvent += Client_ConnectionLostEvent;
    }

    private void Client_ConnectionLostEvent(object? sender, EventArgs e)
    {
        Connect();
        CreateNewClient();
    }

    private void Client_TryRecconect(object? sender, EventArgs e)
    {
        Connect();
    }

    private void Client_Disconnected(object? sender, EventArgs e)
    {
        IsConnected = false;
    }

    private void MessageLogger_Logged(object sender, MessageEventArgs e)
    {
        MessagesLabel.Text += e.Message;
    }

    private void ConsoleLogger_Logged(object sender, MessageEventArgs e)
    {
        ConsoleLabel.Text += e.Message;
    }

    private void Connect()
    {
        if (IsConnected)
        {
            Server?.Disconnect();
            Client.Close();
            IsConnected = false;
        }
        else
        {
            if (IsServer)
            {
                Server = new ServerObject(ConsoleLogger);
            }
            CreateNewClient();
            if (Client.StartClient(ServerIp))
            {
                IsConnected = true;
            }
        }
    }
    private async void SendMessage()
    {
        string MessageToSend = txtMessage.Buffer.Text;
        if (!string.IsNullOrEmpty(MessageToSend))
        {
            await Client.SendMessageAsync(MessageToSend);
            txtMessage.Buffer.Text = string.Empty;
        }
    }

    private void BtnIsHost_Toggled(object? sender, EventArgs e)
    {
        IsServer = btnIsHost.Active;
    }
    private void IsServerChanged()
    {
        if (IsServer)
        {
            ServerIp = NetworkInfo.GetLocalIPAddress();
            txtServerIp.Buffer.Text = ServerIp;
        }
    }
}
