﻿using System;

namespace TimeDataViewer.Core
{
    public abstract class OxyInputGesture : IEquatable<OxyInputGesture>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.</returns>
        public abstract bool Equals(OxyInputGesture other);
    }
}
