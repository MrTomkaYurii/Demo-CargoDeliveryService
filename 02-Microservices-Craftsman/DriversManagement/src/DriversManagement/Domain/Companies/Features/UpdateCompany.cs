namespace DriversManagement.Domain.Companies.Features;

using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.Domain.Companies.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Companies.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateCompany
{
    public sealed record Command(Guid CompanyId, CompanyForUpdateDto UpdatedCompanyData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateCompany);

            var companyToUpdate = await _companyRepository.GetById(request.CompanyId, cancellationToken: cancellationToken);
            var companyToAdd = request.UpdatedCompanyData.ToCompanyForUpdate();
            companyToUpdate.Update(companyToAdd);

            _companyRepository.Update(companyToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}