using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.WPF.Models;

namespace LXIntegratedNavigation.WPF.Services;

public class LogService
{
    #region Public Properties

    public ILogReceiver? LogReceiver { get; set; }
    public List<Log> Logs { get; } = new();

    #endregion Public Properties

    #region Public Methods

    public void Send(LogType type, string message)
    {
        var log = new Log(type, message);
        Logs.Add(log);
        LogReceiver?.Receive(log);
    }

    #endregion Public Methods
}
