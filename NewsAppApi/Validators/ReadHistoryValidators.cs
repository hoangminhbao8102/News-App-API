using FluentValidation;

public class ReadHistoryCreateValidator : AbstractValidator<ReadHistoryCreateDto>
{
    public ReadHistoryCreateValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
