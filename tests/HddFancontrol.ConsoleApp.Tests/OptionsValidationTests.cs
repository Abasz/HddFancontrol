using System.Collections.Generic;

using HddFancontrol.ConsoleApp.Libs.ServiceExtentions;
using HddFancontrol.ConsoleApp.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;
using Xunit.Sdk;

namespace HddFancontrol.ConsoleApp.Tests
{
    public class OptionsBuilderValidationExtensionsTests
    {
        private List<KeyValuePair<string, string>> _settings;
        private readonly ServiceCollection _services;
        private readonly IConfigurationBuilder _configurationBuilder;

        public OptionsBuilderValidationExtensionsTests()
        {
            _settings = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("interval", "10"),
                new KeyValuePair<string, string>("devPath", "./")
            };

            _services = new ServiceCollection();
            _configurationBuilder = new ConfigurationBuilder();
        }

        [Fact]
        public void ShouldBuildSettings()
        {
            _services.AddOptions<GeneralSettings>()
                .Bind(_configurationBuilder.AddInMemoryCollection(_settings).Build())
                .ValidateConfiguration();

            var testOptionsValue = _services.BuildServiceProvider().GetService<IOptions<GeneralSettings>>().Value;

            Assert.Equal(int.Parse(_settings[0].Value), testOptionsValue.Interval);
        }

        [Fact]
        public void ShouldThrowValidationError()
        {
            _settings = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("interval", "10"),
            };
            _services.AddOptions<GeneralSettings>()
                .Bind(_configurationBuilder.AddInMemoryCollection(_settings).Build())
                .ValidateConfiguration();

            var testOptions = _services.BuildServiceProvider().GetService<IOptions<GeneralSettings>>();

            Assert.Throws<OptionsValidationException>(() =>
                testOptions.Value
            );
        }

        [Theory]
        [MemberData(nameof(TestNonEmptySettings))]
        public void ShouldSetCorrectOptionsNameInValidationError<T>(T settingsType, List<KeyValuePair<string, string>> settings)where T : class
        {
            _services.AddOptions<T>()
                .Bind(_configurationBuilder.AddInMemoryCollection(settings).Build())
                .ValidateConfiguration();

            try
            {
                var testOptions = _services.BuildServiceProvider().GetService<IOptions<T>>().Value;
                throw new XunitException("OptionsValidationException was not thrown");
            }
            catch (OptionsValidationException e)
            {
                var optionsName = settingsType.GetType().IsGenericType ? settingsType.GetType().GetGenericArguments()[0].Name : settingsType.GetType().Name;

                Assert.Equal(optionsName, e.OptionsName);
            }
        }

        [Theory]
        [MemberData(nameof(TestNonEmptySettings))]
        public void ShouldSetCorrectErrorMessageInValidationError<T>(T settingsType, List<KeyValuePair<string, string>> settings)where T : class
        {
            _services.AddOptions<T>()
                .Bind(_configurationBuilder.AddInMemoryCollection(settings).Build())
                .ValidateConfiguration();

            try
            {
                var testOptions = _services.BuildServiceProvider().GetService<IOptions<T>>().Value;
                throw new XunitException("OptionsValidationException was not thrown");
            }
            catch (OptionsValidationException e)
            {
                Assert.All(e.Failures, message =>
                {
                    var properties = settingsType.GetType().IsGenericType ? settingsType.GetType().GetGenericArguments()[0].GetProperties() : settingsType.GetType().GetProperties();

                    Assert.Contains(properties, property => message.Contains($"{property.Name}|"));
                });
            }
        }

        [Theory]
        [MemberData(nameof(TestEmptyEnumerableSettings))]
        public void ShouldSetCorrectErrorMessageWhenEnumerableConfigIsEmpty<T>(T settingsType, List<KeyValuePair<string, string>> settings)where T : class
        {
            _services.AddOptions<T>()
                .Bind(_configurationBuilder.AddInMemoryCollection(settings).Build())
                .ValidateConfiguration();
            _ = settingsType;

            try
            {
                var testOptions = _services.BuildServiceProvider().GetService<IOptions<T>>().Value;
                throw new XunitException("OptionsValidationException was not thrown");
            }
            catch (OptionsValidationException e)
            {
                Assert.All(e.Failures, message =>
                {
                    Assert.Contains($"List|", message);
                });
            }
        }

        public static IEnumerable<object[]> TestNonEmptySettings()
        {
            yield return new object[]
            {
                new GeneralSettings(),
                    new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Interval", "10")
                    }
            };
            yield return new object[]
            {
                new GeneralSettings(),
                    new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Interval", "10"),
                            new KeyValuePair<string, string>("DevPath", "/path/should/not/exist")
                    }
            };
            yield return new object[]
            {
                new List<PwmSettings>(),
                    new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("List:0:MaxTemp", "10"),
                            new KeyValuePair<string, string>("List:0:MinTemp", "11"),
                            new KeyValuePair<string, string>("List:1:MaxTemp", "20")
                    }
            };
            yield return new object[]
            {
                new List<PwmSettings>(),
                    new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("List:0:MaxTemp", "10"),
                            new KeyValuePair<string, string>("List:0:MinTemp", "1"),
                            new KeyValuePair<string, string>("List:1:MaxTemp", "20")
                    }
            };
        }

        public static IEnumerable<object[]> TestEmptyEnumerableSettings()
        {
            yield return new object[]
            {
                new List<PwmSettings>(),
                    new List<KeyValuePair<string, string>>() {}
            };
        }
    }
}