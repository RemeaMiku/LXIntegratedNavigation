using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.WPF.Models;

namespace LXIntegratedNavigation.WPF.Services;

public class LogService
{
    public ILogReceiver? LogReceiver { get; set; }
    public List<Log> Logs { get; } = new();

    public void Send(LogType type, string message)
    {
        var log = new Log(type, message);
        Logs.Add(log);
        LogReceiver?.Receive(log);
    }
}
