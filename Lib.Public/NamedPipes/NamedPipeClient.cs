using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPublic.NamedPipes
{

    /// <summary>
    /// these need to be cleaned up and finished. Named pipes can only handle 1 connection so
    /// to handle multitenant it would be a good idea to use the default named pipe in host
    /// and on first connect the host send a reconnect to a new spawned instance and then restarts
    /// the original.
    /// </summary>
    public class NamedPipeClient
    {
        private readonly string Name;
        private Task Listener = null;
        public delegate void OnDataRecieved(string data);
        public event OnDataRecieved OnRecievedData;
        public NamedPipeClient(string name)
        {
            Name = name;
        }
        public void Connect()
        {
            if(Listener == null)
            {
                Listener = Listen();
            }
           
        }
        private async Task Listen()
        {
            var client = new NamedPipeClientStream(Name);
            await client.ConnectAsync();
            var reader = new StreamReader(client);
            var writer = new StreamWriter(client);
            while (true)
            {
                var data = await reader.ReadLineAsync();
                OnRecievedData?.Invoke(data);
            }
        }
    }
}
