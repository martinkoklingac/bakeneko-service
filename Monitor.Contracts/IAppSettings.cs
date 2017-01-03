namespace Monitor
{
    public interface IAppSettings
    {
        #region PROPERTIES
        string ServiceName { get; }
        string ServiceMachineName { get; }
        #endregion
    }
}
