using System.Collections;

namespace HddFancontrol.ConsoleApp.Libs.ServiceExtentions;

public static class OptionsBuilderValidationExtensions
{
    public static OptionsBuilder<TOptions> ValidateConfiguration<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)where TOptions : class
    {
        optionsBuilder.PostConfigure(x =>
        {
            var validationResults = new List<ValidationResult>();

            if (x is IEnumerable optionList)
            {
                validationResults.AddRange(ValidateOptions(optionList));
                if (!optionList.Cast<object>().Any())
                    validationResults.Add(
                        new ValidationResult("No settings were added",
                            new List<string>()
                            {
                                "List"
                            })
                    );
            }
            else
            {
                validationResults.AddRange(ValidateOptions(x));
            }

            if (validationResults.Count == 0)
                return;

            var optionsName = x.GetType().IsGenericType ? x.GetType().GetGenericArguments()[0].Name : x.GetType().Name;

            var failureMessages = validationResults.Select(r => $"{r.MemberNames.ElementAt(0)}|{r.ErrorMessage}");

            throw new OptionsValidationException(optionsName, x.GetType(), failureMessages);
        });
        return optionsBuilder;
    }

    private static List<ValidationResult> ValidateOptions<TOptions>(TOptions x)where TOptions : class
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(x);
        Validator.TryValidateObject(x, context, validationResults, true);

        return validationResults;
    }

    private static List<ValidationResult> ValidateOptions(IEnumerable enumerable)
    {
        var validationResults = new List<ValidationResult>();
        var i = 0;
        foreach (var setting in enumerable)
        {
            var errors = new List<ValidationResult>();
            var context = new ValidationContext(setting);
            var valid = Validator.TryValidateObject(setting, context, errors, true);
            if (!valid)
            {
                validationResults.AddRange(
                    errors.Select(
                        error => new ValidationResult(
                            error.ErrorMessage,
                            error.MemberNames.Select(name => $"{i}.{name}").ToList()
                        )
                    )
                );
            }
            i++;
        }

        return validationResults;
    }
}