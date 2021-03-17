using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using HddFancontrol.Libs.ValidationRules;

using Xunit;

namespace HddFancontrol.ConsoleApp.Tests
{
    public class ShouldBeGreaterThanAttributeTests
    {
        [Theory]
        [InlineData(0, 10)]
        [InlineData(null, null)]
        [InlineData(null, 0)]
        [InlineData(0, null)]
        public void ShouldBeValid(int? smaller, int? higher)
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new ShouldBeGreaterThanTestModel
            {
                Higher = higher,
                Smaller = smaller
            };

            var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

            Assert.True(isValid);
            Assert.Empty(validationErrors);
        }

        [Fact]
        public void ShouldBeInvalid()
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new ShouldBeGreaterThanTestModel
            {
                Higher = 0,
                Smaller = 10
            };

            var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

            Assert.False(isValid);
            Assert.Collection(
                validationErrors,
                error =>
                {
                    Assert.Contains($"Value should be greater than {nameof(sut.Smaller)}", error.ErrorMessage);
                }
            );
        }

        [Theory]
        [InlineData("aaa", "aaa")]
        [InlineData(10, "aaa")]
        [InlineData("aaa", 10)]
        public void ShouldThrowIfNotInt(object smaller, object higher)
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new ShouldBeGreaterThanInvalidTypeTestModel
            {
                Higher = higher,
                Smaller = smaller
            };

            Assert.Throws<InvalidCastException>(
                () => Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true)
            );
        }

        [Fact]
        public void ShouldThrowIfComparerPropertyDoesNotExist()
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new ShouldBeGreaterThanInvalidPropertyTestModel
            {
                Higher = 10,
                Smaller = 0
            };

            Assert.Throws<ArgumentException>(
                () => Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true)
            );
        }

        [Fact]
        public void ShouldSetMemberNameOnValidationError()
        {
            var validationErrors = new List<ValidationResult>();
            var sut = new ShouldBeGreaterThanTestModel
            {
                Higher = 0,
                Smaller = 10
            };

            var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

            Assert.False(isValid);
            Assert.Collection(
                validationErrors,
                error =>
                {
                    Assert.NotNull(error.MemberNames);
                    Assert.NotEmpty(error.MemberNames);
                }
            );
        }

        private class ShouldBeGreaterThanTestModel
        {
            public int? Smaller { get; set; }

            [ShouldBeGreaterThan(nameof(Smaller))]
            public int? Higher { get; set; }
        }

        private class ShouldBeGreaterThanInvalidPropertyTestModel
        {
            public object Smaller { get; set; }

            [ShouldBeGreaterThan("NoSuchProperty")]
            public object Higher { get; set; }
        }
        private class ShouldBeGreaterThanInvalidTypeTestModel
        {
            public object Smaller { get; set; }

            [ShouldBeGreaterThan(nameof(Smaller))]
            public object Higher { get; set; }
        }
    }
}