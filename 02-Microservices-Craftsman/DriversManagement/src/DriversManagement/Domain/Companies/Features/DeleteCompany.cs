namespace DriversManagement.Domain.Companies.Features;

using DriversManagement.Domain.Companies.Services;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteCompany
{
    public sealed record Command(Guid CompanyId) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteCompany);

            var recordToDelete = await _companyRepository.GetById(request.CompanyId, cancellationToken: cancellationToken);
            _companyRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}