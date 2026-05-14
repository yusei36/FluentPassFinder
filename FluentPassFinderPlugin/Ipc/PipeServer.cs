using FluentPassFinder.Contracts.Public.Ipc;
using System;
using System.IO.Pipes;
using System.Threading;

namespace FluentPassFinderPlugin.Ipc
{
    internal class PipeServer : IDisposable
    {
        private readonly string pipeName;
        private readonly PluginRequestHandler handler;
        private NamedPipeServerStream serverStream;
        private Thread readerThread;
        private volatile bool running;

        public PipeServer(string pipeName, PluginRequestHandler handler)
        {
            this.pipeName = pipeName;
            this.handler = handler;
        }

        public void Start()
        {
            running = true;
            serverStream = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.None);

            readerThread = new Thread(ReadLoop) { IsBackground = true, Name = "FluentPassFinder.PipeServer" };
            readerThread.Start();
        }

        private void ReadLoop()
        {
            try
            {
                serverStream.WaitForConnection();

                while (running && serverStream.IsConnected)
                {
                    var request = PipeProtocol.ReadRequest(serverStream);
                    if (request == null) break;

                    var response = handler.Handle(request);
                    PipeProtocol.WriteResponse(serverStream, response);
                }
            }
            catch (Exception)
            {
                // Connection closed or plugin shutting down — exit silently
            }
        }

        public void Dispose()
        {
            running = false;
            try { serverStream?.Dispose(); } catch { }
        }
    }
}
