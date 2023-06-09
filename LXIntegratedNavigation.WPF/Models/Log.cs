using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXIntegratedNavigation.WPF.Models;

public class Log
{
    public LogType Type { get; set; }
    public string Message { get; set; }
    public UtcTime TimeStamp { get; set; }
    public Log(LogType type, string message)
    {
        Type = type;
        Message = message;
        TimeStamp = UtcTime.Now;
    }
}
