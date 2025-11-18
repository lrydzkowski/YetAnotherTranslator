using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Persistence;

internal class DatabaseOptionsValidator : AbstractValidator<DatabaseOptions>
{
    public DatabaseOptionsValidator()
    {
        DefineConnectionStringValidator();
    }

    private void DefineConnectionStringValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithName(nameof(DatabaseOptions.ConnectionString))
            .OverridePropertyName($"{DatabaseOptions.Position}.{nameof(DatabaseOptions.ConnectionString)}");
    }
}
