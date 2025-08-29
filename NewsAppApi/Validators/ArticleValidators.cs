using FluentValidation;
using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Validators;

public class ArticleCreateValidator : AbstractValidator<ArticleCreateDto>
{
    public ArticleCreateValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ImageUrl).MaximumLength(255);
        RuleFor(x => x.AuthorId).NotEmpty().WithMessage("AuthorId is required");
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

public class ArticleFilterValidator : AbstractValidator<ArticleFilter>
{
    public ArticleFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.SortDir).Must(s => s is "asc" or "desc");

        When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(f => f.CreatedFrom <= f.CreatedTo)
                .WithMessage("CreatedFrom must be <= CreatedTo");
        });
    }
}
