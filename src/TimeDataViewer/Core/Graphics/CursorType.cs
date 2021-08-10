using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public enum CursorType
    {
        /// <summary>
        /// The default cursor
        /// </summary>
        Default = 0,

        /// <summary>
        /// The pan cursor
        /// </summary>
        Pan,

        /// <summary>
        /// The zoom rectangle cursor
        /// </summary>
        ZoomRectangle,

        /// <summary>
        /// The horizontal zoom cursor
        /// </summary>
        ZoomHorizontal,

        /// <summary>
        /// The vertical zoom cursor
        /// </summary>
        ZoomVertical
    }
}
