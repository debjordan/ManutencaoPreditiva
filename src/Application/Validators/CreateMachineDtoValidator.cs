using FluentValidation;
using ManutencaoPreditiva.Application.DTOs.Request;

namespace ManutencaoPreditiva.Application.Validators;

public class CreateMachineDtoValidator : AbstractValidator<CreateMachineDto>
{
    public CreateMachineDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .Length(2, 100).WithMessage("Nome deve ter entre 2 e 100 caracteres");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Número de série é obrigatório")
            .Length(3, 50).WithMessage("Número de série deve ter entre 3 e 50 caracteres")
            .Matches("^[A-Za-z0-9-]+$").WithMessage("Número de série deve conter apenas letras, números e hífens");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Modelo é obrigatório")
            .Length(2, 100).WithMessage("Modelo deve ter entre 2 e 100 caracteres");

        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Fabricante é obrigatório")
            .Length(2, 100).WithMessage("Fabricante deve ter entre 2 e 100 caracteres");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Localização é obrigatória")
            .Length(2, 200).WithMessage("Localização deve ter entre 2 e 200 caracteres");

        RuleFor(x => x.InstallationDate)
            .NotEmpty().WithMessage("Data de instalação é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de instalação não pode ser futura");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição não pode ter mais de 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
