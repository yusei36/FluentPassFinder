namespace FluentPassFinderContracts
{
    public interface IAppProxy
    {
        void Main();
        void Shutdown();
        void Init(IPluginProxy pluginHostProxy);
    }
}
