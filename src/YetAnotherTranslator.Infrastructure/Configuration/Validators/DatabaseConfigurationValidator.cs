using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Configuration.Validators;

public class DatabaseConfigurationValidator : AbstractValidator<DatabaseConfiguration>
{
    public DatabaseConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Database.ConnectionString is required");
    }
}
