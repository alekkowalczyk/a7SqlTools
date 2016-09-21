using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace a7SqlTools.DbComparer.Struct
{
    public interface a7IDbDifference
    {
        string ButtonCaption { get; }
        string Text { get; }
        ICommand CopyTypeToA { get; }
        ICommand CopyTypeToB { get; }
        Visibility Button2Visibility { get; }
        string Button2Caption { get; }
    }
}
