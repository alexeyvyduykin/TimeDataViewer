﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public abstract partial class Model
    {
        // internal static readonly OxyColor DefaultSelectionColor = OxyColors.Yellow;

        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly object syncRoot = new object();

        protected Model()
        {

        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="Model" />.
        /// </summary>
        /// <value>A synchronization object.</value>
        /// <remarks>This property can be used when modifying the <see cref="Model" /> on a separate thread (not the thread updating or rendering the model).</remarks>
        public object SyncRoot
        {
            get { return this.syncRoot; }
        }

        /// <summary>
        /// Returns the elements that are hit at the specified position.
        /// </summary>
        /// <param name="args">The hit test arguments.</param>
        /// <returns>
        /// A sequence of hit results.
        /// </returns>
        public IEnumerable<HitTestResult> HitTest(HitTestArguments args)
        {
            // Revert the order to handle the top-level elements first
            foreach (var element in this.GetElements().Reverse())
            {
                var result = element.HitTest(args);
                if (result != null)
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Gets all elements of the model, top-level elements first.
        /// </summary>
        /// <returns>An enumerator of the elements.</returns>
        public abstract IEnumerable<UIElement> GetElements();
    }
}