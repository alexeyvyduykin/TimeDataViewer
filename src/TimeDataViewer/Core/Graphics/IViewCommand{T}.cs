using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public interface IViewCommand<in T> : IViewCommand where T : OxyInputEventArgs
    {
        // Executes the command on the specified plot.
        void Execute(IView view, IController controller, T args);
    }
}
