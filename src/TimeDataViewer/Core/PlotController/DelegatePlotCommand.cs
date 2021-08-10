using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class DelegatePlotCommand<T> : DelegateViewCommand<T>
        where T : OxyInputEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePlotCommand{T}" /> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public DelegatePlotCommand(Action<IPlotView, IController, T> handler)
            : base((v, c, e) => handler((IPlotView)v, c, e))
        {
        }
    }
}
