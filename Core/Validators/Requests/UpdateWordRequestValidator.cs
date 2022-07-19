﻿using FluentValidation;
using OhMyWord.Core.Requests.Words;

namespace OhMyWord.Core.Validators.Requests;

public class UpdateWordRequestValidator : AbstractValidator<UpdateWordRequest>
{
    public UpdateWordRequestValidator()
    {
        RuleFor(request => request.Value).NotEmpty();
        RuleFor(request => request.Definition).NotEmpty();
    }
}