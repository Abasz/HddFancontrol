namespace HddFancontrol.Libs.ValidationRules;

public class DirectoryExistsAttribute : ValidationAttribute
{
    public DirectoryExistsAttribute() : base(() => "Invalid directory path") {}

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var path = (string?)value;

        if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
            return new ValidationResult("", new List<string>() { validationContext.DisplayName });

        return ValidationResult.Success;
    }
}