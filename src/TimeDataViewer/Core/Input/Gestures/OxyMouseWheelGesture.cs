﻿namespace TimeDataViewer.Core
{
    public class OxyMouseWheelGesture : OxyInputGesture
    {
        public OxyMouseWheelGesture()
        {

        }

        // Indicates whether the current object is equal to another object of the same type.
        public override bool Equals(OxyInputGesture other)
        {
            var mwg = other as OxyMouseWheelGesture;
            return mwg != null;
        }
    }
}
