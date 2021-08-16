namespace TimeDataViewer.Core
{
    public abstract class ManipulatorBase<T> where T : OxyInputEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManipulatorBase{T}" /> class.
        /// </summary>
        /// <param name="view">The view.</param>
        protected ManipulatorBase(IView view)
        {
            this.View = view;
        }

        /// <summary>
        /// Gets the plot view where the event was raised.
        /// </summary>
        /// <value>The plot view.</value>
        public IView View { get; private set; }

        /// <summary>
        /// Occurs when a manipulation is complete.
        /// </summary>
        /// <param name="e">The <see cref="OxyInputEventArgs" /> instance containing the event data.</param>
        public virtual void Completed(T e)
        {
        }

        /// <summary>
        /// Occurs when the input device changes position during a manipulation.
        /// </summary>
        /// <param name="e">The <see cref="OxyInputEventArgs" /> instance containing the event data.</param>
        public virtual void Delta(T e)
        {
        }

        /// <summary>
        /// Occurs when an input device begins a manipulation on the plot.
        /// </summary>
        /// <param name="e">The <see cref="OxyInputEventArgs" /> instance containing the event data.</param>
        public virtual void Started(T e)
        {
        }
    }
}
