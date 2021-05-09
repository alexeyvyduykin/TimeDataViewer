using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TimeDataViewer.Core;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.UnitTests.Core.Axises
{
    public class CategoryAxisTests
    {
        private CategoryAxis CreateCategoryAxis()
        {           
            return new CategoryAxis()
            {
                Header = "Y",
                Type = AxisType.Y,
                HasInversion = true,
                IsDynamicLabelEnable = true,
            };
        }

        [Fact]
        public void UpdateWindow_WithWindow_MinMaxPixelsCorrect()
        {
            var axis = CreateCategoryAxis();
            var window = new RectI(0, 0, 800, 600);

            axis.UpdateWindow(window);

            Assert.Equal(0, axis.MinPixel);
            Assert.Equal(600, axis.MaxPixel);
        }

        [Fact]
        public void UpdateViewport_WithViewport_MinMaxValuesCorrect()
        {
            var axis = CreateCategoryAxis();
            var viewport = new RectD(10.0, 20.0, 86400.0, 100.0);

            axis.UpdateViewport(viewport);

            Assert.Equal(20.0, axis.MinValue);
            Assert.Equal(120.0, axis.MaxValue);
        }

        [Fact]
        public void UpdateScreen_WithScreen_MinMaxScreenValuesCorrect()
        {
            var axis = CreateCategoryAxis();
            var screen = new RectD(10.0, 20.0, 800.0, 600.0);

            axis.UpdateScreen(screen);

            Assert.Equal(20.0, axis.MinScreenValue);
            Assert.Equal(620.0, axis.MaxScreenValue);
        }

        [Fact]
        public void UpdateWindow_AddAxisChangedEvent_Raising()
        {
            var axis = CreateCategoryAxis();         
            bool raise = false;

            axis.OnAxisChanged += (w, h) => raise = true;

            axis.UpdateWindow(RectI.Empty);

            Assert.True(raise);
        }

        [Fact]
        public void UpdateViewport_AddAxisChangedEvent_Raising()
        {
            var axis = CreateCategoryAxis();
            bool raise = false;

            axis.OnAxisChanged += (w, h) => raise = true;

            axis.UpdateViewport(new RectD());

            Assert.True(raise);
        }

        [Fact]
        public void UpdateScreen_AddAxisChangedEvent_Raising()
        {
            var axis = CreateCategoryAxis();
            bool raise = false;

            axis.OnAxisChanged += (w, h) => raise = true;

            axis.UpdateScreen(new RectD());

            Assert.True(raise);
        }
    }
}
