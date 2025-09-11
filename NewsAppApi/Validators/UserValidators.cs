using FluentValidation;

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
