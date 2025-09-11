using FluentValidation;

public class TagCreateValidator : AbstractValidator<TagCreateDto>
{
    public TagCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class TagUpdateValidator : AbstractValidator<TagUpdateDto>
{
    public TagUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
