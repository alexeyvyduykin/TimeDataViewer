using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Timeline.Core;
using Timeline.Spatial;

namespace Timeline.UnitTests.Core.Axises
{
    public class CategoryAxisTests
    {
        private CategoryAxis CreateCategoryAxis(bool hasInversion)
        {           
            return new CategoryAxis()
            {
                Header = "Y",
                Type = AxisType.Y,
                HasInversion = hasInversion,
                IsDynamicLabelEnable = true,
            };
        }

        [Fact]
        public void UpdateWindow_WithWindow_MinMaxPixelsCorrect()
        {
            var axis = CreateCategoryAxis(false);
            var window = new RectI(0, 0, 800, 600);

            axis.UpdateWindow(window);

            Assert.Equal(0, axis.MinPixel);
            Assert.Equal(600, axis.MaxPixel);
        }

        [Fact]
        public void UpdateViewport_WithViewport_MinMaxValuesCorrect()
        {
            var axis = CreateCategoryAxis(false);
            var viewport = new RectD(10.0, 20.0, 86400.0, 100.0);

            axis.UpdateViewport(viewport);

            Assert.Equal(20.0, axis.MinValue);
            Assert.Equal(120.0, axis.MaxValue);
        }

        [Fact]
        public void UpdateClientViewport_WithClientViewport_MinMaxClientValuesCorrect()
        {
            var axis = CreateCategoryAxis(false);
            var clientViewport = new RectD(10.0, 20.0, 800.0, 600.0);

            axis.UpdateClientViewport(clientViewport);

            Assert.Equal(20.0, axis.MinClientValue);
            Assert.Equal(620.0, axis.MaxClientValue);
        }

        [Fact]
        public void UpdateWindow_AddAxisChangedEvent_Raising()
        {
            var axis = CreateCategoryAxis(false);         
            bool raise = false;

            axis.OnAxisChanged += (w, h) => raise = true;

            axis.UpdateWindow(RectI.Empty);

            Assert.True(raise);
        }

        [Fact]
        public void UpdateViewport_AddAxisChangedEvent_Raising()
        {
            var axis = CreateCategoryAxis(false);
            bool raise = false;

            axis.OnAxisChanged += (w, h) => raise = true;

            axis.UpdateViewport(new RectD());

            Assert.True(raise);
        }

        [Fact]
        public void UpdateClientViewport_AddAxisChangedEvent_Raising()
        {
            var axis = CreateCategoryAxis(false);
            bool raise = false;

            axis.OnAxisChanged += (w, h) => raise = true;

            axis.UpdateClientViewport(new RectD());

            Assert.True(raise);
        }

        [Theory]
        [InlineData(false, 30.0)]
        [InlineData(true, 70.0)]
        public void FromAbsoluteToLocal_WithAbsoluteValue_LocalValueCorrect(bool hasInversion, double expectedValue)
        {
            var axis = CreateCategoryAxis(hasInversion);            
            var window = new RectI(0, 0, 800, 600);
            var viewport = new RectD(10.0, 20.0, 86400.0, 60.0);
            axis.UpdateWindow(window);
            axis.UpdateViewport(viewport);

            var local = axis.FromAbsoluteToLocal(100);

            Assert.Equal(expectedValue, local);
        }

        [Theory]
        [InlineData(false, 100)]
        [InlineData(true, 500)]
        public void FromLocalToAbsolute_WithLocalValue_AbsoluteValueCorrect(bool hasInversion, int expectedValue)
        {
            var axis = CreateCategoryAxis(hasInversion);       
            var window = new RectI(0, 0, 800, 600);
            var viewport = new RectD(10.0, 20.0, 86400.0, 60.0);
            axis.UpdateWindow(window);
            axis.UpdateViewport(viewport);

            var pixel = axis.FromLocalToAbsolute(30.0);

            Assert.Equal(expectedValue, pixel);
        }
    }
}
