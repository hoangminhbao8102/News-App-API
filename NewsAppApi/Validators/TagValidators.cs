using FluentValidation;
using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Validators;

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
