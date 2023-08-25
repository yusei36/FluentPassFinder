namespace FluentPassFinder.Contracts.Public
{
    public interface IAppProxy
    {
        void Main();
        void Shutdown();
        void WaitForAppCreation();
        void Init(IPluginProxy pluginHostProxy);
    }
}
