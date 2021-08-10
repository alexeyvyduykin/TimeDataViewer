using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class OxyMouseDownGesture : OxyInputGesture
    {
        public OxyMouseDownGesture(OxyMouseButton mouseButton)
        {
            this.MouseButton = mouseButton;
        }

        // Gets the mouse button.    
        public OxyMouseButton MouseButton { get; private set; }

        // Indicates whether the current object is equal to another object of the same type.
        public override bool Equals(OxyInputGesture other)
        {
            var mg = other as OxyMouseDownGesture;
            return mg != null && mg.MouseButton == this.MouseButton;
        }
    }
}
