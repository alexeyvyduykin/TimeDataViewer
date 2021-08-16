﻿using System;

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