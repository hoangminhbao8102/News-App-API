using FluentValidation;

public class ArticleCreateValidator : AbstractValidator<ArticleCreateDto>
{
    public ArticleCreateValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ImageUrl).MaximumLength(255);
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("CategoryId is required");
        RuleForEach(x => x.TagIds).GreaterThan(0);
    }
}

public class ArticleUpdateValidator : AbstractValidator<ArticleUpdateDto>
{
    public ArticleUpdateValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ImageUrl).MaximumLength(255);
        RuleForEach(x => x.TagIds).GreaterThan(0);
    }
}
