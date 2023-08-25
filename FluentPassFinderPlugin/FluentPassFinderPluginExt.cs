using FluentPassFinder.Contracts.Public;
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
        private IPluginProxy pluginProxy;
        private IAppProxy appProxy;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            var pluginHost = host;
            pluginProxy = new PluginProxy(pluginHost);

            appProxy = LoadAppProxy();
            appThread = StartAppAsSeperateThread(pluginProxy, appProxy);

            pluginHost.MainWindow.DocumentManager.GetOpenDatabases();

            return true;
        }

        private IAppProxy LoadAppProxy()
        {
            IAppProxy appProxy = null;
            var pluginAssembly = Assembly.GetAssembly(typeof(FluentPassFinderPluginExt));
            var pluginFileLocation = pluginAssembly.Location;
            var fileInfo = new FileInfo(pluginFileLocation);
            var files = Directory.GetFiles(fileInfo.Directory.FullName, applicationExeName, SearchOption.AllDirectories);
            if (files.Any())
            {
                var exeFilePath = files[0];
                var appAssembly = Assembly.LoadFrom(exeFilePath);
                foreach (Type type in appAssembly.GetTypes())
                {
                    if (type.GetInterfaces().Contains(typeof(IAppProxy)) && type.IsAbstract == false)
                    {
                        appProxy = (IAppProxy)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                        break;
                    }
                }
            }
            else
            {
                throw new Exception($"{applicationExeName} was not found.");
            }

            if (appProxy == null)
            {
                throw new Exception($"No implementation of {nameof(IAppProxy)} was found.");
            }

            return appProxy;
        }

        public override void Terminate()
        {
            try
            {
                appProxy.Shutdown();
                appThread?.Abort();
            }
            catch (Exception)
            {
            }
            base.Terminate();
        }

        private static Thread StartAppAsSeperateThread(IPluginProxy pluginHostProxy, IAppProxy appHost)
        {
            var appThread = new Thread(appHost.Main);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Priority = ThreadPriority.Highest;
            appThread.Start();

            appHost.WaitForAppCreation();
            appHost.Init(pluginHostProxy);
            return appThread;
        }
    }
}
