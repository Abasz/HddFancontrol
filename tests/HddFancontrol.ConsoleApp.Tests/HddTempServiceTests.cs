using System;
using System.Linq;

using HddFancontrol.ConsoleApp.Services.Classes;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace HddFancontrol.ConsoleApp.Tests
{
    public class HddTempServiceTests
    {
        private readonly HddTempService _hddTempService;

        public HddTempServiceTests()
        {
            _hddTempService = new HddTempService(new NullLogger<HddTempService>());
        }

        [Fact]
        public async void ShouldGetHddTempForSpecificDrive()
        {
            var temp = await _hddTempService.GetHddTempAsync("/dev/sda");

            Assert.NotEqual(0, temp);
        }

        [Fact]
        public async void ShouldReturnNullWhenTempIsNotAvailableForSpecificDrive()
        {
            var temp = await _hddTempService.GetHddTempAsync("/dev/sdf");

            Assert.Null(temp);
        }

        [Fact]
        public async void ShouldThrowArgumentExceptionWhenDrivePathIsInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _hddTempService.GetHddTempAsync("/invalid/path"));
        }

        [Fact]
        public async void ShouldGetTempForAllDrivesWhereAvailable()
        {
            var temps = await _hddTempService.GetAllHddTempsAsync();

            Assert.True(temps.Any());
        }
    }
}