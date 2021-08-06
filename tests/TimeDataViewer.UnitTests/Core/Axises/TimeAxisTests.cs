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
    //public class TimeAxisTests
    //{
    //    private TimeAxis CreateTimeAxis(bool hasInversion)
    //    {
    //        return new TimeAxis()
    //        {
    //            Header = "X",
    //            Type = AxisType.X,
    //            HasInversion = hasInversion,
    //            IsDynamicLabelEnable = true,
    //            TimePeriodMode = TimePeriod.Month,
    //            LabelFormatPool = new Dictionary<TimePeriod, string>()
    //            {
    //                { TimePeriod.Hour, @"{0:HH:mm}" },
    //                { TimePeriod.Day, @"{0:HH:mm}" },
    //                { TimePeriod.Week, @"{0:dd/MMM}" },
    //                { TimePeriod.Month, @"{0:dd}" },
    //                { TimePeriod.Year, @"{0:dd/MMM}" },
    //            },
    //            LabelDeltaPool = new Dictionary<TimePeriod, double>()
    //            {
    //                { TimePeriod.Hour, 60.0 * 5 },
    //                { TimePeriod.Day, 3600.0 * 2 },
    //                { TimePeriod.Week, 86400.0 },
    //                { TimePeriod.Month, 86400.0 },
    //                { TimePeriod.Year, 86400.0 * 12 },
    //            }
    //        };
    //    }

    //    [Fact]
    //    public void UpdateWindow_WithWindow_MinMaxPixelsCorrect()
    //    {
    //        var axis = CreateTimeAxis(false);
    //        var window = new RectI(0, 0, 800, 600);

    //        axis.UpdateWindow(window);

    //        Assert.Equal(0, axis.MinPixel);
    //        Assert.Equal(800, axis.MaxPixel);
    //    }

    //    [Fact]
    //    public void UpdateViewport_WithViewport_MinMaxValuesCorrect()
    //    {
    //        var axis = CreateTimeAxis(false);
    //        var viewport = new RectD(10.0, 20.0, 86400.0, 100.0);

    //        axis.UpdateViewport(viewport);

    //        Assert.Equal(10.0, axis.MinValue);
    //        Assert.Equal(86410.0, axis.MaxValue);
    //    }

    //    [Fact]
    //    public void UpdateClientViewport_WithClientViewport_MinMaxClientValuesCorrect()
    //    {
    //        var axis = CreateTimeAxis(false);
    //        var clientViewport = new RectD(10.0, 20.0, 800.0, 600.0);

    //        axis.UpdateClientViewport(clientViewport);

    //        Assert.Equal(10.0, axis.MinClientValue);
    //        Assert.Equal(810.0, axis.MaxClientValue);
    //    }

    //    [Fact]
    //    public void UpdateWindow_AddAxisChangedEvent_Raising()
    //    {
    //        var axis = CreateTimeAxis(false);
    //        bool raise = false;

    //        axis.OnAxisChanged += (w, h) => raise = true;

    //        axis.UpdateWindow(RectI.Empty);

    //        Assert.True(raise);
    //    }
    //    [Fact]
    //    public void UpdateViewport_AddAxisChangedEvent_Raising()
    //    {
    //        var axis = CreateTimeAxis(false);
    //        bool raise = false;

    //        axis.OnAxisChanged += (w, h) => raise = true;

    //        axis.UpdateViewport(new RectD());

    //        Assert.True(raise);
    //    }

    //    [Fact]
    //    public void UpdateClientViewport_AddAxisChangedEvent_Raising()
    //    {
    //        var axis = CreateTimeAxis(false);
    //        bool raise = false;

    //        axis.OnAxisChanged += (w, h) => raise = true;

    //        axis.UpdateClientViewport(new RectD());

    //        Assert.True(raise);
    //    }

    //    [Theory]
    //    [InlineData(false, 10010.0)]
    //    [InlineData(true, 70010.0)]
    //    public void FromAbsoluteToLocal_WithAbsoluteValue_LocalValueCorrect(bool hasInversion, double expectedValue)
    //    {
    //        var axis = CreateTimeAxis(hasInversion);
    //        var window = new RectI(0, 0, 800, 600);
    //        var viewport = new RectD(10.0, 20.0, 80000.0, 60.0);
    //        axis.UpdateWindow(window);
    //        axis.UpdateViewport(viewport);

    //        var local = axis.FromAbsoluteToLocal(100);

    //        Assert.Equal(expectedValue, local);
    //    }

    //    [Theory]
    //    [InlineData(false, 100)]
    //    [InlineData(true, 700)]
    //    public void FromLocalToAbsolute_WithLocalValue_AbsoluteValueCorrect(bool hasInversion, int expectedValue)
    //    {
    //        var axis = CreateTimeAxis(hasInversion);
    //        var window = new RectI(0, 0, 800, 600);
    //        var viewport = new RectD(10.0, 20.0, 80000.0, 60.0);
    //        axis.UpdateWindow(window);
    //        axis.UpdateViewport(viewport);

    //        var pixel = axis.FromLocalToAbsolute(10010.0);

    //        Assert.Equal(expectedValue, pixel);
    //    }
    //}
}
