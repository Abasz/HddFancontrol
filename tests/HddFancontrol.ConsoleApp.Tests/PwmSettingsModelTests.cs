using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HddFancontrol.ConsoleApp.Models;

using Xunit;

namespace HddFancontrol.ConsoleApp.Tests
{
    public class PwmSettingsModelTests
    {
        [Theory]
        [InlineData(-1, 2, 3, 1, 2)]
        [InlineData(1, -1, 3, 1, 2)]
        [InlineData(1, 2, -1, 1, 2)]
        [InlineData(1, 2, 3, -1, 2)]
        [InlineData(1, 2, 3, 1, 0)]
        public void ShouldHaveInvalidIntervalRange(int minTemp, int maxTemp, int maxPwm, int minPwm, int minStart)
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new PwmSettings
            {
                MinTemp = minTemp,
                MaxTemp = maxTemp,
                MaxPwm = maxPwm,
                MinPwm = minPwm,
                MinStart = minStart,
            };

            var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

            Assert.False(isValid);
            Assert.Contains(
                validationErrors,
                error => error.ErrorMessage.Contains("Required to be")
            );
        }

        [Theory]
        [InlineData(3, 1, 3, 1, 2)]
        [InlineData(1, 2, 2, 3, 2)]
        [InlineData(1, 2, 4, 2, 1)]
        public void ShouldHaveInvalidGreaterThan(int minTemp, int maxTemp, int maxPwm, int minPwm, int minStart)
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new PwmSettings
            {
                MinTemp = minTemp,
                MaxTemp = maxTemp,
                MaxPwm = maxPwm,
                MinPwm = minPwm,
                MinStart = minStart,
            };

            var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

            Assert.False(isValid);
            Assert.Contains(
                validationErrors,
                error => error.ErrorMessage.Contains("Value should be greater")
            );
        }

        [Theory]
        [InlineData(3, 1, 3, 1, 2)]
        [InlineData(1, 2, 2, 3, 2)]
        [InlineData(1, 2, 4, 2, 5)]
        public void ShouldHaveInvalidLessThan(int minTemp, int maxTemp, int maxPwm, int minPwm, int minStart)
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new PwmSettings
            {
                MinTemp = minTemp,
                MaxTemp = maxTemp,
                MaxPwm = maxPwm,
                MinPwm = minPwm,
                MinStart = minStart,
            };

            var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

            Assert.False(isValid);
            Assert.Contains(
                validationErrors,
                error => error.ErrorMessage.Contains("Value should be less")
            );
        }

        [Fact]
        public void ShouldBeValid()
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new PwmSettings
            {
                MinTemp = 1,
                MaxTemp = 10,
                MaxPwm = 255,
                MinPwm = 2,
                MinStart = 10,
            };

            var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

            Assert.True(isValid);
        }
    }
}