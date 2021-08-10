﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class InputCommandBinding
    {
        public InputCommandBinding(OxyInputGesture gesture, IViewCommand command)
        {
            this.Gesture = gesture;
            this.Command = command;
        }

        // Gets the gesture. 
        public OxyInputGesture Gesture { get; private set; }

        // Gets the command.   
        public IViewCommand Command { get; private set; }
    }
}