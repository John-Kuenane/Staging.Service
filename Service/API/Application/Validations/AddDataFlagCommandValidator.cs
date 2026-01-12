using FluentValidation;
using Staging.API.Application.Commands.PackageAggregate;

namespace eStaging.API.Application.Validations;
public class AddDataFlagCommandValidator : AbstractValidator<AddDataFlagCommand>
{
    public AddDataFlagCommandValidator(ILogger<AddDataFlagCommandValidator> logger)
    {
        RuleFor(command => command.RequesterName)
            .NotEmpty()
            .Matches(@"[a-zA-Z0-9 ]")
            .WithMessage("Description contains invalid characters (Enter A-Z, a-z, 0-9, space)");

        RuleFor(command => command.RequesterEmail)
            .NotEmpty();

        RuleFor(command => command.Subject)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"[a-zA-Z0-9 ',.]")
            .WithMessage("Subject contains invalid characters (Enter A-Z, a-z, 0-9, space, apostrophe, comma, period)");

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(750)
            .Matches(@"[a-zA-Z0-9 ',.]")
            .WithMessage("Description contains invalid characters (Enter A-Z, a-z, 0-9, space, apostophe, comma, period)");

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("INSTANCE CREATED - {ClassName}", GetType().Name);
        }
    }
}

