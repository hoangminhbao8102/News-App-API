using FluentValidation;
using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Validators;

public class BookmarkCreateValidator : AbstractValidator<BookmarkCreateDto>
{
    public BookmarkCreateValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}

public class BookmarkFilterValidator : AbstractValidator<BookmarkFilter>
{
    public BookmarkFilterValidator()
    {
        When(f => f.CreatedFrom.HasValue && f.CreatedTo.HasValue, () =>
        {
            RuleFor(f => f).Must(f => f.CreatedFrom <= f.CreatedTo)
                .WithMessage("CreatedFrom must be <= CreatedTo");
        });
    }
}
