using FluentValidation;
using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Validators;

public class UserCreateValidator : AbstractValidator<UserCreateDto>
{
    public UserCreateValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);

        RuleFor(x => x.FullName)
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .Must(r => r == "Admin" || r == "User")
            .WithMessage("Role must be 'Admin' or 'User'");
    }
}

public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.FullName).MaximumLength(100);
        When(x => x.Role != null, () =>
        {
            RuleFor(x => x.Role!)
                .Must(r => r == "Admin" || r == "User")
                .WithMessage("Role must be 'Admin' or 'User'");
        });
    }
}

public class UserLoginValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class UserFilterValidator : AbstractValidator<UserFilter>
{
    public UserFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.SortDir).Must(s => s is "asc" or "desc");
        When(x => x.Role != null, () =>
            RuleFor(x => x.Role!).Must(r => r == "Admin" || r == "User"));
        When(x => x.CreatedTo.HasValue && x.CreatedFrom.HasValue, () =>
            RuleFor(x => x).Must(x => x.CreatedFrom <= x.CreatedTo)
                .WithMessage("CreatedFrom must be <= CreatedTo"));
    }
}
