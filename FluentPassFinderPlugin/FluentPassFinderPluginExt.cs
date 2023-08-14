using FluentPassFinderContracts;
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
        private const string wpfApplicationExeName = "FluentPassFinder.exe";
        private Thread wpfAppThread;
        private IPluginProxy pluginHostProxy;
        private IAppProxy wpfAppHost;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            var pluginHost = host;
            pluginHostProxy = new PluginHostProxy(pluginHost);

            wpfAppHost = GetWpfAppHost();
            wpfAppThread = StartAppAsSeperateThread(pluginHostProxy, wpfAppHost);

            pluginHost.MainWindow.DocumentManager.GetOpenDatabases();

            return true;
        }

        private IAppProxy GetWpfAppHost()
        {
            IAppProxy wpfAppHost = null;
            var pluginAssembly = Assembly.GetAssembly(typeof(FluentPassFinderPluginExt));
            var pluginFileLocation = pluginAssembly.Location;
            var fileInfo = new FileInfo(pluginFileLocation);
            var files = Directory.GetFiles(fileInfo.Directory.FullName, wpfApplicationExeName, SearchOption.AllDirectories);
            if (files.Any())
            {
                var exeFilePath = files[0];
                var wpfAppAssembly = Assembly.LoadFrom(exeFilePath);
                foreach (Type type in wpfAppAssembly.GetTypes())
                {
                    if (type.GetInterfaces().Contains(typeof(IAppProxy)) && type.IsAbstract == false)
                    {
                        wpfAppHost = (IAppProxy)type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                        break;
                    }
                }
            }
            else
            {
                throw new Exception($"{wpfApplicationExeName} was not found.");
            }

            if (wpfAppHost == null)
            {
                throw new Exception($"No implementation of {nameof(IAppProxy)} was found.");
            }

            return wpfAppHost;
        }

        public override void Terminate()
        {
            try
            {
                wpfAppHost.Shutdown();
                wpfAppThread?.Abort();
            }
            catch (Exception)
            {
            }
            base.Terminate();
        }

        private static Thread StartAppAsSeperateThread(IPluginProxy pluginHostProxy, IAppProxy wpfAppHost)
        {
            var wpfAppThread = new Thread(wpfAppHost.Main);
            wpfAppThread.SetApartmentState(ApartmentState.STA);
            wpfAppThread.Start();

            wpfAppHost.Init(pluginHostProxy);
            return wpfAppThread;
        }
    }
}
