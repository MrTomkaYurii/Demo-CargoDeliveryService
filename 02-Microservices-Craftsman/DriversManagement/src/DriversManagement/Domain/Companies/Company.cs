namespace DriversManagement.Domain.Companies;

using System.ComponentModel.DataAnnotations;
using DriversManagement.Domain.Drivers;
using System.ComponentModel.DataAnnotations.Schema;
using Destructurama.Attributed;
using DriversManagement.Exceptions;
using DriversManagement.Domain.Companies.Models;
using DriversManagement.Domain.Companies.DomainEvents;


public class Company : BaseEntity
{
    public string CompanyName { get; private set; }

    public string Address { get; private set; }

    public string Phone { get; private set; }

    public string Email { get; private set; }

    public string ContactEmail { get; private set; }

    public string ContactPerson { get; private set; }

    public string ContactPhone { get; private set; }

    public Driver Driver { get; }

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Company Create(CompanyForCreation companyForCreation)
    {
        var newCompany = new Company();

        newCompany.CompanyName = companyForCreation.CompanyName;
        newCompany.Address = companyForCreation.Address;
        newCompany.Phone = companyForCreation.Phone;
        newCompany.Email = companyForCreation.Email;
        newCompany.ContactEmail = companyForCreation.ContactEmail;
        newCompany.ContactPerson = companyForCreation.ContactPerson;
        newCompany.ContactPhone = companyForCreation.ContactPhone;

        newCompany.QueueDomainEvent(new CompanyCreated(){ Company = newCompany });
        
        return newCompany;
    }

    public Company Update(CompanyForUpdate companyForUpdate)
    {
        CompanyName = companyForUpdate.CompanyName;
        Address = companyForUpdate.Address;
        Phone = companyForUpdate.Phone;
        Email = companyForUpdate.Email;
        ContactEmail = companyForUpdate.ContactEmail;
        ContactPerson = companyForUpdate.ContactPerson;
        ContactPhone = companyForUpdate.ContactPhone;

        QueueDomainEvent(new CompanyUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Company() { } // For EF + Mocking
}
