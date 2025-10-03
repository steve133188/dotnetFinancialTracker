namespace DotnetFinancialTrackerApp.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> ValidationErrors { get; private set; } = new();

    private ServiceResult() { }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static ServiceResult<T> Failure(string errorMessage)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }

    public static ServiceResult<T> ValidationFailure(List<string> validationErrors)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ValidationErrors = validationErrors,
            ErrorMessage = "Validation failed"
        };
    }
}

public interface IValidationService
{
    ServiceResult<T> Validate<T>(T item) where T : class;
}

public class ValidationService : IValidationService
{
    public ServiceResult<T> Validate<T>(T item) where T : class
    {
        var errors = new List<string>();

        if (item == null)
        {
            errors.Add("Item cannot be null");
            return ServiceResult<T>.ValidationFailure(errors);
        }

        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(item);
            var attributes = property.GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case System.ComponentModel.DataAnnotations.RequiredAttribute required:
                        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                        {
                            errors.Add($"{property.Name} is required");
                        }
                        break;

                    case System.ComponentModel.DataAnnotations.StringLengthAttribute stringLength:
                        if (value is string stringValue &&
                            (stringValue.Length < stringLength.MinimumLength ||
                             stringValue.Length > stringLength.MaximumLength))
                        {
                            errors.Add($"{property.Name} must be between {stringLength.MinimumLength} and {stringLength.MaximumLength} characters");
                        }
                        break;

                    case System.ComponentModel.DataAnnotations.RangeAttribute range:
                        if (value is IComparable comparable)
                        {
                            if (comparable.CompareTo(range.Minimum) < 0 || comparable.CompareTo(range.Maximum) > 0)
                            {
                                errors.Add($"{property.Name} must be between {range.Minimum} and {range.Maximum}");
                            }
                        }
                        break;
                }
            }
        }

        return errors.Count == 0
            ? ServiceResult<T>.Success(item)
            : ServiceResult<T>.ValidationFailure(errors);
    }
}