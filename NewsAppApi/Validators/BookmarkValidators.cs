using FluentValidation;

public class BookmarkCreateValidator : AbstractValidator<BookmarkCreateDto>
{
    public BookmarkCreateValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
