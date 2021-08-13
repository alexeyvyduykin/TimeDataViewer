using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class TrackerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the hit result.
        /// </summary>
        /// <value>The hit result.</value>
        public TrackerHitResult HitResult { get; set; }
    }
}
