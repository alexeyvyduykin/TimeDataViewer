﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Models
{
    public record TimeInterval(double Begin, double End);

    public record Interval(double Left, double Right);
}