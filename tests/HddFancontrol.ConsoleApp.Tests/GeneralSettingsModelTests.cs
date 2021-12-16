namespace HddFancontrol.ConsoleApp.Tests;

public class GeneralSettingsModelTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ShouldHaveInvalidIntervalRange(int interval)
    {
        var validationErrors = new List<ValidationResult>();
        var sut = new GeneralSettings
        {
            Interval = interval,
            DevPath = "./"
        };

        var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

        Assert.False(isValid);
        Assert.True(validationErrors.Any());
    }

    [Theory]
    [InlineData(10)]
    public void ShouldHaveValidIntervalRange(int interval)
    {
        var validationErrors = new List<ValidationResult>();
        var sut = new GeneralSettings
        {
            Interval = interval,
            DevPath = "./"
        };

        var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

        Assert.True(isValid);
        Assert.False(validationErrors.Any());
    }

    [Theory]
    [InlineData(null, "required")]
    [InlineData("", "required")]
    [InlineData("./not/valid/path", "Directory")]
    public void ShouldHaveInvalidDevPath(string devPath, string errorMessage)
    {
        var validationErrors = new List<ValidationResult>();
        var sut = new GeneralSettings
        {
            Interval = 10,
            DevPath = devPath
        };

        var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

        Assert.False(isValid);
        Assert.Collection(validationErrors, error =>
        {
            Assert.Contains(errorMessage, error.ErrorMessage);
        });
    }

    [Theory]
    [InlineData("./")]
    [InlineData("/var/")]
    [InlineData("/var")]
    public void ShouldHaveValidDevPath(string devPath)
    {
        var validationErrors = new List<ValidationResult>();
        var sut = new GeneralSettings
        {
            Interval = 10,
            DevPath = devPath
        };

        var isValid = Validator.TryValidateObject(sut, new ValidationContext(sut), validationErrors, true);

        Assert.True(isValid);
    }
}