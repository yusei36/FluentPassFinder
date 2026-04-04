using FluentPassFinder.Contracts.Public;
using FluentPassFinderPlugin.Ipc;
using KeePass.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FluentPassFinderPlugin
{
    public sealed class FluentPassFinderPluginExt : Plugin
    {
        private const string applicationExeName = "FluentPassFinder.exe";
        private Thread appThread;
        private IAppProxy appProxy;
        private PipeServer pipeServer;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;

            var handler = new PluginRequestHandler(host);
            var pipeName = "FluentPassFinder_" + Guid.NewGuid().ToString("N");

            pipeServer = new PipeServer(pipeName, handler);
            pipeServer.Start();

            appProxy = LoadAppProxy();
            appThread = StartAppAsSeperateThread(appProxy, pipeName);

            return true;
        }

        public override void Terminate()
        {
            try
            {
                appProxy?.Shutdown();
                appThread?.Abort();
            }
            catch (Exception) { }

            pipeServer?.Dispose();

            base.Terminate();
        }

        private IAppProxy LoadAppProxy()
        {
            var pluginAssembly = Assembly.GetAssembly(typeof(FluentPassFinderPluginExt));
            var fileInfo = new FileInfo(pluginAssembly.Location);
            var files = Directory.GetFiles(fileInfo.Directory.FullName, applicationExeName, SearchOption.AllDirectories);

            if (!files.Any())
                throw new Exception($"{applicationExeName} was not found.");

            var appAssembly = Assembly.LoadFrom(files[0]);
            foreach (Type type in appAssembly.GetTypes())
            {
                if (type.GetInterfaces().Contains(typeof(IAppProxy)) && !type.IsAbstract)
                    return (IAppProxy)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
            }

            throw new Exception($"No implementation of {nameof(IAppProxy)} was found.");
        }

        private static Thread StartAppAsSeperateThread(IAppProxy appProxy, string pipeName)
        {
            var appThread = new Thread(appProxy.Main);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Priority = ThreadPriority.Highest;
            appThread.Start();

            appProxy.WaitForAppCreation();
            appProxy.Init(pipeName);
            return appThread;
        }
    }
}
