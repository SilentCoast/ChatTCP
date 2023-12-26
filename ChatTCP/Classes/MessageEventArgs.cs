using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatTCP.Classes
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
