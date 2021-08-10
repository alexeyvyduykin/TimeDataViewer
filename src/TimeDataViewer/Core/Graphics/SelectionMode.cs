using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public enum SelectionMode
    {
        /// <summary>
        /// All the elements will be selected
        /// </summary>
        All,

        /// <summary>
        /// A single element will be selected
        /// </summary>
        Single,

        /// <summary>
        /// Multiple elements can be selected
        /// </summary>
        Multiple
    }
}
