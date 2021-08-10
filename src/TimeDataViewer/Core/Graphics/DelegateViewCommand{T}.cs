using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class DelegateViewCommand<T> : IViewCommand<T>
         where T : OxyInputEventArgs
    {
        /// <summary>
        /// The handler
        /// </summary>
        private readonly Action<IView, IController, T> handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateViewCommand{T}" /> class.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public DelegateViewCommand(Action<IView, IController, T> handler)
        {
            this.handler = handler;
        }

        /// <summary>
        /// Executes the command on the specified plot.
        /// </summary>
        /// <param name="view">The plot view.</param>
        /// <param name="controller">The plot controller.</param>
        /// <param name="args">The <see cref="OxyInputEventArgs" /> instance containing the event data.</param>
        public void Execute(IView view, IController controller, T args)
        {
            this.handler(view, controller, args);
        }

        /// <summary>
        /// Executes the command on the specified plot.
        /// </summary>
        /// <param name="view">The plot view.</param>
        /// <param name="controller">The plot controller.</param>
        /// <param name="args">The <see cref="OxyInputEventArgs" /> instance containing the event data.</param>
        public void Execute(IView view, IController controller, OxyInputEventArgs args)
        {
            this.handler(view, controller, (T)args);
        }
    }
}
