using System;
using System.Collections.Generic;

namespace TimeDataViewer.Core
{
    public abstract class SelectableElement : Element
    {
        private Selection selection;

        protected SelectableElement()
        {
            this.Selectable = true;
            this.SelectionMode = SelectionMode.All;
        }

        /// <summary>
        /// Occurs when the selected items is changed.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this element can be selected. The default is <c>true</c>.
        /// </summary>
        public bool Selectable { get; set; }

        /// <summary>
        /// Gets or sets the selection mode of items in this element. The default is <c>SelectionMode.All</c>.
        /// </summary>
        /// <value>The selection mode.</value>
        /// <remarks>This is only used by the select/unselect functionality, not by the rendering.</remarks>
        public SelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Determines whether any part of this element is selected.
        /// </summary>
        /// <returns><c>true</c> if this element is selected; otherwise, <c>false</c>.</returns>
        public bool IsSelected()
        {
            return this.selection != null;
        }

        /// <summary>
        /// Gets the indices of the selected items in this element.
        /// </summary>
        /// <returns>Enumerator of item indices.</returns>
        public IEnumerable<int> GetSelectedItems()
        {
            this.EnsureSelection();
            return this.selection.GetSelectedItems();
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        public void ClearSelection()
        {
            this.selection = null;
            this.OnSelectionChanged();
        }

        /// <summary>
        /// Unselects all items in this element.
        /// </summary>
        public void Unselect()
        {
            this.selection = null;
            this.OnSelectionChanged();
        }

        /// <summary>
        /// Determines whether the specified item is selected.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <returns><c>true</c> if the item is selected; otherwise, <c>false</c>.</returns>
        public bool IsItemSelected(int index)
        {
            if (this.selection == null)
            {
                return false;
            }

            if (index == -1)
            {
                return this.selection.IsEverythingSelected();
            }

            return this.selection.IsItemSelected(index);
        }

        /// <summary>
        /// Selects all items in this element.
        /// </summary>
        public void Select()
        {
            this.selection = Selection.Everything;
            this.OnSelectionChanged();
        }

        public void SelectItem(int index)
        {
            if (this.SelectionMode == SelectionMode.All)
            {
                throw new InvalidOperationException("Use the Select() method when using SelectionMode.All");
            }

            this.EnsureSelection();
            if (this.SelectionMode == SelectionMode.Single)
            {
                this.selection.Clear();
            }

            this.selection.Select(index);
            this.OnSelectionChanged();
        }

        public void UnselectItem(int index)
        {
            if (this.SelectionMode == SelectionMode.All)
            {
                throw new InvalidOperationException("Use the Unselect() method when using SelectionMode.All");
            }

            this.EnsureSelection();
            this.selection.Unselect(index);
            this.OnSelectionChanged();
        }

        private void EnsureSelection()
        {
            if (this.selection == null)
            {
                this.selection = new Selection();
            }
        }

        private void OnSelectionChanged(EventArgs args = null)
        {
            var e = this.SelectionChanged;
            if (e != null)
            {
                e(this, args);
            }
        }
    }
}
