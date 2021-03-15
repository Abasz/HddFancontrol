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
        // extra cross property validation for (e.g. max should be higher than min)

        // min should be lower than max
        [Range(0, int.MaxValue, ErrorMessage = "Required to be greater than 0")]
        public int MinTemp { get; set; } = -1;

        // max should be higher than min
        [Range(0, int.MaxValue, ErrorMessage = "Required to be greater than 0")]
        public int MaxTemp { get; set; } = -1;

        // minStart should be higher than minPwm
        [Range(1, int.MaxValue, ErrorMessage = "Required to be greater than 1")]
        public int MinStart { get; set; } = -1;

        // min should be lower than max
        [Range(0, 255, ErrorMessage = "Required to be between 0 and 255")]
        public int MinPwm { get; set; } = -1;

        // max should be higher than min
        [Range(0, 255, ErrorMessage = "Required to be between 0 and 255")]
        public int MaxPwm { get; set; } = -1;
    }
}