using FluentValidation;
using SwaggerExamples.Api.Requests;

namespace SwaggerExamples.Api;

public class SwaggerDemoValidations
{
    public class SwaggerDemoRequestValidator : AbstractValidator<SwaggerDemoRequest>
    {
        public SwaggerDemoRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(50);
            RuleFor(x => x.Age).InclusiveBetween(0, 120);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.DateOfBirth).LessThan(DateTime.Now);
            RuleFor(x => x.Address).NotEmpty().MinimumLength(10).MaximumLength(100);
            RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[1-9]\d{1,14}$"); // E.164 format
            RuleFor(x => x.Notes).MaximumLength(200); // Optional field with max length
            RuleFor(x => x.Tags)
                .NotEmpty().Must(x => x.Count > 2);

            RuleForEach(x => x.Tags).MinimumLength(2)
                .MaximumLength(20); // Each tag must not be empty and max length of 20
        }
    }
}