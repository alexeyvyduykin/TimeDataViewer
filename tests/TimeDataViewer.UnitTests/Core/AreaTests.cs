using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TimeDataViewer.Core;

namespace TimeDataViewer.UnitTests.Core
{
    public class AreaTests
    {
        private Area CreateArea()
        {
            CoreFactory factory = new CoreFactory();
            return factory.CreateArea();
        }

        [Fact]
        public void UpdateViewport_AxisesChanged_Raising()
        {
            var area = CreateArea();
            bool raiseX = false;
            bool raiseY = false;
            area.AxisX.OnAxisChanged += (w, h) => raiseX = true;
            area.AxisY.OnAxisChanged += (w, h) => raiseY = true;

            area.UpdateViewport(3600.0, 0.0, 86400.0, 100.0);

            Assert.True(raiseX);
            Assert.True(raiseY);
        }

        [Fact]
        public void UpdateViewport_AxisXMinMaxValues_EqualExpected()
        {
            var area = CreateArea();

            area.UpdateViewport(3600.0, 0.0, 80000.0, 100.0);

            var minValue = area.AxisX.MinValue;
            var maxValue = area.AxisX.MaxValue;

            Assert.Equal(3600.0, minValue);
            Assert.Equal(83600.0, maxValue);
        }

        [Fact]
        public void UpdateViewport_AxisYMinMaxValues_EqualExpected()
        {
            var area = CreateArea();

            area.UpdateViewport(3600.0, 1.0, 86400.0, 2.0);

            var minValue = area.AxisY.MinValue;
            var maxValue = area.AxisY.MaxValue;

            Assert.Equal(1.0, minValue);
            Assert.Equal(3.0, maxValue);
        }

        [Fact]
        public void UpdateSize_AxisesChanged_Raising()
        {
            var area = CreateArea();
            bool raiseX = false;
            bool raiseY = false;
            area.AxisX.OnAxisChanged += (w, h) => raiseX = true;
            area.AxisY.OnAxisChanged += (w, h) => raiseY = true;

            area.UpdateSize(400, 200);

            Assert.True(raiseX);
            Assert.True(raiseY);
        }

        [Fact]
        public void UpdateSize_AxisXMinMaxPixels_EqualExpected()
        {
            var area = CreateArea();

            area.UpdateSize(400, 200);

            var minValue = area.AxisX.MinPixel;
            var maxValue = area.AxisX.MaxPixel;

            Assert.Equal(0, minValue);
            Assert.Equal(400, maxValue);
        }

        [Fact]
        public void UpdateSize_AxisYMinMaxPixels_EqualExpected()
        {
            var area = CreateArea();

            area.UpdateSize(400, 200);

            var minValue = area.AxisY.MinPixel;
            var maxValue = area.AxisY.MaxPixel;

            Assert.Equal(0, minValue);
            Assert.Equal(200, maxValue);
        }

        [Fact]
        public void Zoom_DefaultValue_EqualExpected()
        {
            var area = CreateArea();

            var expected = area.Zoom;

            Assert.Equal(0, expected);
        }

        [Fact]
        public void Zoom_SetNotEqualValue_ZoomChanged()
        {
            var area = CreateArea();
            bool raise = false;
            area.Zoom = 3;
            area.OnZoomChanged += (s, e) => raise = true;

            area.Zoom = 5;

            Assert.True(raise);
        }

        [Fact]
        public void Zoom_SetEqualValue_ZoomChanged()
        {
            var area = CreateArea();
            bool raise = false;
            area.Zoom = 3;
            area.OnZoomChanged += (s, e) => raise = true;

            area.Zoom = 3;

            Assert.False(raise);
        }

        [Fact]
        public void Zoom_SetGreaterThanMaxValue_EqualMaxValue()
        {
            var area = CreateArea();
            var max = area.MaxZoom;
            
            area.Zoom = max + 10;
    
            Assert.Equal(max, area.Zoom);
        }

        [Fact]
        public void Zoom_SetLessThanMinValue_EqualMinValue()
        {
            var area = CreateArea();
            var min = area.MinZoom;

            area.Zoom = min - 10;

            Assert.Equal(min, area.Zoom);
        }
    }   
}
