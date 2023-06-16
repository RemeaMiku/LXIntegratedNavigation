using LXIntegratedNavigation.WPF.Models;

namespace LXIntegratedNavigation.WPF.Services;

public interface ILogReceiver
{
    #region Public Methods

    public void Receive(Log log);

    #endregion Public Methods
}
