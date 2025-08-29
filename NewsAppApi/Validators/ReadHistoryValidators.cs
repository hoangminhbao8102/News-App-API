using FluentValidation;
using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Validators;

public class ReadHistoryCreateValidator : AbstractValidator<ReadHistoryCreateDto>
{
    public ReadHistoryCreateValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}

public class ReadHistoryFilterValidator : AbstractValidator<ReadHistoryFilter>
{
    public ReadHistoryFilterValidator()
    {
        When(f => f.ReadFrom.HasValue && f.ReadTo.HasValue, () =>
        {
            RuleFor(f => f).Must(f => f.ReadFrom <= f.ReadTo)
                .WithMessage("ReadFrom must be <= ReadTo");
        });
    }
}
