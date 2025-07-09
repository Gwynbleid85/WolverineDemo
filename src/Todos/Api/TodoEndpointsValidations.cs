using FluentValidation;
using Todos.Api.Models.Requests;

namespace Todos.Api;

public class TodoEndpointsValidations
{
    public class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
    {
        public CreateTodoRequestValidator()
        {
            RuleFor(x => x.Title).NotNull().MinimumLength(3).MaximumLength(10);
            RuleFor(x => x.Description).NotNull().MinimumLength(3).MaximumLength(32);
            RuleFor(x => x.IsCompleted).NotNull();
        }
    }

    public class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
    {
        public UpdateTodoRequestValidator()
        {
            RuleFor(x => x.Title).NotNull().MinimumLength(3).MaximumLength(10);
            RuleFor(x => x.Description).NotNull().MinimumLength(3).MaximumLength(32);
            RuleFor(x => x.IsCompleted).NotNull();
        }
    }
}