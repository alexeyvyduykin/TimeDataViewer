﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public abstract class Element
    {
        public Model Parent { get; internal set; }
    }
}
