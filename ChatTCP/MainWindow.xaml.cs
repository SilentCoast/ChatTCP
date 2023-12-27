using ChatTCP.Classes.Logger;
using ChatTCP.ViewModels;
using System.Windows;

namespace ChatTCP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            Logger errorLogger = new Logger();
            errorLogger.Logged += Logger_Logged;
            vm = new MainViewModel(errorLogger);
            DataContext = vm;
            vm.ScrollToBottomRequested += Vm_ScrollToBottomRequested;
        }

        private void Vm_ScrollToBottomRequested(object? sender, EventArgs e)
        {
            scrollConsole.ScrollToBottom();
            scrollMessages.ScrollToBottom();
        }

        private void Logger_Logged(object sender, MessageEventArgs e)
        {
            MessageBox.Show(e.Message);
        }
    }
}