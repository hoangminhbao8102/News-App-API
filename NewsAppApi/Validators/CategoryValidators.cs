using FluentValidation;

public class CategoryCreateValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class CategoryUpdateValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}
