using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TimeDataViewer.Core;

namespace TimeDataViewer.UnitTests.Core
{
    public class CoreFactoryTests
    {
        [Fact]
        public void CreateTimeAxis_ValidateProperties_EqualDefaultValues()
        {           
            var axis = new TimeAxis();

            Assert.Equal(AxisType.X, axis.Type);
            Assert.Equal("X", axis.Header);
            Assert.False(axis.HasInversion);
            Assert.True(axis.IsDynamicLabelEnable);
            Assert.NotNull(axis.LabelFormatPool);
            Assert.NotNull(axis.LabelDeltaPool);
            Assert.Equal(DateTime.MinValue, axis.Epoch0);
            Assert.Equal(TimePeriod.Month, axis.TimePeriodMode);
        }

        [Fact]
        public void CreateCategoryAxis_ValidateProperties_EqualDefaultValues()
        {         
            var axis = new CategoryAxis();

            Assert.Equal(AxisType.Y, axis.Type);
            Assert.Equal("Y", axis.Header);
            Assert.False(axis.HasInversion);
            Assert.True(axis.IsDynamicLabelEnable);
        }
    }
}
