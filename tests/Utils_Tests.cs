using System;
using System.Collections;
using System.Collections.Generic;

using Xunit;

namespace Dackup.Tests
{
    public class Utils_Tests
    {

        [Fact]
        public void ConvertRemoveThresholdToTimeSpan_Parameter_Length_Should_Not_Less_2_Test()
        {
            var input = "a";
            Exception ex = Assert.Throws<InvalidOperationException>(() => Utils.ConvertRemoveThresholdToTimeSpan(input));
            Assert.Equal($"Invalid value for remove_threshold option: '{input}'", ex.Message);
        }
        [Fact]
         public void ConvertRemoveThresholdToTimeSpan_Parameter_Should_End_With_Specific_Values_Test()
        {        
            var allows_unit = new[] { "d", "h", "m", "s", "f", "z" };
            var input = "1a";
            Exception ex = Assert.Throws<InvalidOperationException>(() => Utils.ConvertRemoveThresholdToTimeSpan(input));
            Assert.Equal($"Invalid value for remove_threshold option: '{input}'", ex.Message);
        }
        [Fact]
        public void ConvertRemoveThresholdToTimeSpan_Should_Return_Test()
        {
            int number = 3;
            Assert.Equal(TimeSpan.FromDays(3)
            ,Utils.ConvertRemoveThresholdToTimeSpan( number.ToString() + "d"));
            Assert.Equal(TimeSpan.FromHours(3)
            ,Utils.ConvertRemoveThresholdToTimeSpan( number.ToString() + "h"));
            Assert.Equal(TimeSpan.FromMinutes(3)
            ,Utils.ConvertRemoveThresholdToTimeSpan( number.ToString() + "m"));
            Assert.Equal(TimeSpan.FromSeconds(3)
            ,Utils.ConvertRemoveThresholdToTimeSpan( number.ToString() + "s"));
            Assert.Equal(TimeSpan.FromMilliseconds(3)
            ,Utils.ConvertRemoveThresholdToTimeSpan( number.ToString() + "f"));
            Assert.Equal(TimeSpan.FromTicks(3)
            ,Utils.ConvertRemoveThresholdToTimeSpan( number.ToString() + "z"));
        }
    }
}
