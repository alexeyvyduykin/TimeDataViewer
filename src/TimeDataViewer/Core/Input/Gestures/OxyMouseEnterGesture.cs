﻿namespace TimeDataViewer.Core
{
    public class OxyMouseEnterGesture : OxyInputGesture
    {
        public OxyMouseEnterGesture()
        {

        }

        // Indicates whether the current object is equal to another object of the same type.
        public override bool Equals(OxyInputGesture other)
        {
            var mg = other as OxyMouseEnterGesture;
            return mg != null;
        }
    }
}
