﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public partial class PlotModel
    {
        // Renders the plot with the specified rendering context.
        public void Render(double width, double height)
        {
            RenderOverride(width, height);
        }

        // Renders the plot with the specified rendering context.
        protected virtual void RenderOverride(double width, double height)
        {
            lock (SyncRoot)
            {
                try
                {
                    Width = width;
                    Height = height;

                    PlotArea = new RectD(0, 0, Width, Height);

                    //UpdateAxisTransforms();
                    //UpdateIntervals();

                    //foreach (var a in Axes)
                    //{
                    //    a.ResetCurrentValues();
                    //}

                    RenderSeries();
                    //RenderAxises();
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }
        }

        //private void RenderAxises()
        //{
        //    foreach (var item in Axes)
        //    {
        //        item.MyOnRender(this);
        //    }
        //}

        private void RenderSeries()
        {
            foreach (var s in Series/*.Where(s => s.IsVisible)*/)
            {
                s.Render();
                s.MyOnRender();
            }
        }
    }
}
