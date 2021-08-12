using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public interface IStackableSeries
    {
        /// <summary>
        /// Gets a value indicating whether this series is stacked.
        /// </summary>
        bool IsStacked { get; }

        /// <summary>
        /// Gets the stack group.
        /// </summary>
        /// <value>The stack group.</value>
        string StackGroup { get; }
    }
}
