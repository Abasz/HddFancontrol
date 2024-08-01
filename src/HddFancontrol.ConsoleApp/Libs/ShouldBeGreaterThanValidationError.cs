namespace HddFancontrol.Libs.ValidationRules;

public class ShouldBeGreaterThanAttribute(string comparisonProperty) : ValidationAttribute()
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(comparisonProperty) ?? throw new ArgumentException($"Property with name ${comparisonProperty} was not found");
        var comparisonValue = property.GetValue(validationContext.ObjectInstance);

        if (comparisonValue is null || value is null)
            return ValidationResult.Success;

        if ((int)value > (int)comparisonValue)
            return ValidationResult.Success;

        ErrorMessage ??= $"Value should be greater than {comparisonProperty}";

        return new ValidationResult("", [validationContext.DisplayName]);
    }
}