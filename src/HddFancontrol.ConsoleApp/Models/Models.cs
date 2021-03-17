using System.ComponentModel.DataAnnotations;

using HddFancontrol.Libs.ValidationRules;

namespace HddFancontrol.ConsoleApp.Models
{
    public class GeneralSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = "Required to be greater than 0")]
        public int Interval { get; set; } = 0;

        [Required(ErrorMessage = "Path is required")]
        [DirectoryExists(ErrorMessage = "Directory does not exist")]
        public string DevPath { get; set; } = null!;
    }

    public class PwmSettings
    {
        [Range(0, int.MaxValue, ErrorMessage = "Required to be greater than 0")]
        [ShouldBeLessThan(nameof(MaxTemp))]
        public int MinTemp { get; set; } = -1;

        [Range(0, int.MaxValue, ErrorMessage = "Required to be greater than 0")]
        [ShouldBeGreaterThan(nameof(MinTemp))]
        public int MaxTemp { get; set; } = -1;

        [Range(1, int.MaxValue, ErrorMessage = "Required to be greater than 1")]
        [ShouldBeLessThan(nameof(MaxPwm))]
        [ShouldBeGreaterThan(nameof(MinPwm))]
        public int MinStart { get; set; } = -1;

        [Range(0, 255, ErrorMessage = "Required to be between 0 and 255")]
        [ShouldBeLessThan(nameof(MaxPwm))]
        public int MinPwm { get; set; } = -1;

        [Range(0, 255, ErrorMessage = "Required to be between 0 and 255")]
        [ShouldBeGreaterThan(nameof(MinPwm))]
        public int MaxPwm { get; set; } = -1;
    }
}