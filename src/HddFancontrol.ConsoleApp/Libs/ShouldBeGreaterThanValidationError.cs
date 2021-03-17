using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HddFancontrol.Libs.ValidationRules
{
    public class ShouldBeGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public ShouldBeGreaterThanAttribute(string comparisonProperty) : base()
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property is null)
                throw new ArgumentException($"Property with name ${_comparisonProperty} was not found");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);

            if (comparisonValue is null || value is null)
                return ValidationResult.Success;

            if ((int)value > (int)comparisonValue)
                return ValidationResult.Success;

            if (ErrorMessage is null)
                ErrorMessage = $"Value should be greater than {_comparisonProperty}";

            return new ValidationResult("", new List<string>() { validationContext.DisplayName });
        }
    }
}