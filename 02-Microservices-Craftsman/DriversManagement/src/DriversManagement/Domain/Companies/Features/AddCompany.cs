namespace DriversManagement.Domain.Companies.Features;

using DriversManagement.Domain.Companies.Services;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.Domain.Companies.Models;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddCompany
{
    public sealed record Command(CompanyForCreationDto CompanyToAdd) : IRequest<CompanyDto>;

    public sealed class Handler : IRequestHandler<Command, CompanyDto>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _companyRepository = companyRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<CompanyDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddCompany);

            var companyToAdd = request.CompanyToAdd.ToCompanyForCreation();
            var company = Company.Create(companyToAdd);

            await _companyRepository.Add(company, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return company.ToCompanyDto();
        }
    }
}