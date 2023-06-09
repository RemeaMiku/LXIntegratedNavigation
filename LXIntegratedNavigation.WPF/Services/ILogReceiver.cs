using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.WPF.Models;

namespace LXIntegratedNavigation.WPF.Services;

public interface ILogReceiver
{
    public void Receive(Log log);
}
