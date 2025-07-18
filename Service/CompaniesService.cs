﻿using College2Career.Data;
using College2Career.DTO;
using College2Career.HelperServices;
using College2Career.Models;
using College2Career.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace College2Career.Service
{
    public class CompaniesService : ICompaniesService
    {
        private readonly ICompaniesRepository companiesRepository;
        private readonly ICloudinaryService cloudinaryService;
        private readonly IEmailService emailService;
        private readonly C2CDBContext c2CDBContext;

        
        public CompaniesService(ICompaniesRepository companiesRepository, ICloudinaryService cloudinaryService, IEmailService emailService, C2CDBContext c2CDBContext)
        {
            this.companiesRepository = companiesRepository;
            this.cloudinaryService = cloudinaryService;
            this.emailService = emailService;
            this.c2CDBContext = c2CDBContext;
        }

        public async Task<ServiceResponse<string>> createCompanyProfile(CompaniesDTO companiesDTO, int usersId)
        {
            try
            {
                var response = new ServiceResponse<string>();

                var imageURL = await cloudinaryService.uploadImages(companiesDTO.profilePicture);

                if (imageURL == null)
                {
                    response.data = "0";
                    response.message = "Image upload failed!";
                    response.status = false;
                    return response;
                }

                if (usersId == 0)
                {
                    response.data = "0";
                    response.message = "Unauthorized! Please login again!";
                    response.status = false;
                }

                var existingCompany = await companiesRepository.getCompanyProfileByUsersId(usersId);

                if (existingCompany != null)
                {
                    response.data = "0";
                    response.message = "Your profile is already exists!";
                    response.status = false;
                    return response;
                }

                var newCompany = new Companies
                {
                    usersId = usersId,
                    companyName = companiesDTO.companyName,
                    establishedDate = companiesDTO.establishedDate,
                    contactNumber = companiesDTO.contactNumber,
                    profilePicture = imageURL,
                    industry = companiesDTO.industry,
                    address = companiesDTO.address,
                    city = companiesDTO.city,
                    state = companiesDTO.state,
                    employeeSize = companiesDTO.employeeSize,
                    createdAt = DateTime.Now,
                };

                await companiesRepository.createCompanyProfile(newCompany);
                response.data = "1";
                response.message = "Profile created successfully.";
                response.status = true;

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in createCompanyProfile method: " + ex.Message);
                throw;
            }
        }

        public async Task<ServiceResponse<List<CompaniesDTO>>> getCompanyProfileByUsersId(int userId)
        {
            try
            {
                var response = new ServiceResponse<List<CompaniesDTO>>();

                var existCompany = await companiesRepository.getCompanyProfileByUsersId(userId);
                if (existCompany == null)
                {
                    response.data = new List<CompaniesDTO>();
                    response.message = "No company profile found.";
                    response.status = false;
                }
                else
                {
                    var companyProfile = new CompaniesDTO
                    {
                        companyId = existCompany.companyId,
                        companyName = existCompany.companyName,
                        address = existCompany.address,
                        city = existCompany.city,
                        state = existCompany.state,
                        establishedDate = existCompany.establishedDate,
                        contactNumber = existCompany.contactNumber,
                        profilePictureURL = existCompany.profilePicture,
                        industry = existCompany.industry,
                        employeeSize = existCompany.employeeSize,
                        status = existCompany.status,
                        reasonOfStatus = existCompany.reasonOfStatus,
                        createdAt = existCompany.createdAt
                    };

                    response.data = new List<CompaniesDTO> { companyProfile };
                    response.message = "Company profile found.";
                    response.status = true;
                }
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in getCompaniesProfileByUsersId method: " + ex.Message);
                throw;
            }
        }

        public async Task<ServiceResponse<string>> updateCompanyStatus(int companyId, string status, string statusReason)
        {
            try
            {
                ServiceResponse<string> response = new ServiceResponse<string>();

                var existCompany = await companiesRepository.updateCompanyStatus(companyId, status, statusReason);

                if (existCompany == null)
                {
                    response.data = "0";
                    response.message = "No company found.";
                    response.status = false;
                }
                else
                {
                    int roleIdIs = (int)(existCompany.Users.roleId);
                    if (status == "activated")
                    {
                        string subject = "Profile Verification";
                        string body = emailService.createActivetedEmailBody(existCompany.companyName, (int)(existCompany.Users.roleId));
                        await emailService.sendEmail(existCompany.Users.email, subject, body);
                    }
                    else if (status == "rejected")
                    {
                        string subject = "Profile Verification";
                        string body = emailService.createRejectedEmailBody(existCompany.companyName, existCompany.reasonOfStatus, (int)(existCompany.Users.roleId));
                        await emailService.sendEmail(existCompany.Users.email, subject, body);
                    }
                    else if (status == "deactivated")
                    {
                        string subject = "Profile Verification";
                        string body = emailService.createDeactivatedEmailBody(existCompany.companyName, existCompany.reasonOfStatus, (int)(existCompany.Users.roleId));
                        await emailService.sendEmail(existCompany.Users.email, subject, body);
                    }

                    response.data = "1";
                    response.message = "Company status updated.";
                    response.status = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in updateCompanyStatus method: " + ex.Message);
                throw;
            }
        }

        public async Task<ServiceResponse<List<CompaniesDTO>>> getAllCompanies()
        {
            try
            {
                var response = new ServiceResponse<List<CompaniesDTO>>();

                var existCompanies = await companiesRepository.getAllCompanies();

                if (existCompanies == null)
                {
                    response.data = new List<CompaniesDTO>();
                    response.message = "No companies found.";
                    response.status = false;
                }
                else
                {
                    var allCompannies = existCompanies.Select(c => new CompaniesDTO
                    {
                        companyId = c.companyId,
                        companyName = c.companyName,
                        email = c.Users.email,
                        address = c.address,
                        city = c.city,
                        state = c.state,
                        establishedDate = c.establishedDate,
                        contactNumber = c.contactNumber,
                        profilePictureURL = c.profilePicture,
                        industry = c.industry,
                        employeeSize = c.employeeSize,
                        status = c.status,
                        reasonOfStatus = c.reasonOfStatus,
                        usersId = c.usersId
                    }).ToList();

                    response.data = allCompannies;
                    response.message = "All companies found.";
                    response.status = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in getAllCompanies method: " + ex.Message);
                throw;
            }
        }
        public async Task<ServiceResponse<List<CompaniesDTO>>> getCompaniesByPendingStatus()
        {
            try
            {
                var response = new ServiceResponse<List<CompaniesDTO>>();

                var pendingCompanies = await companiesRepository.getCompaniesByPendingStatus();

                if (pendingCompanies == null || !pendingCompanies.Any())
                {
                    response.data = new List<CompaniesDTO>();
                    response.message = "No pending companies found.";
                    response.status = false;
                }
                else
                {
                    var pendingCompany = pendingCompanies.Select(c => new CompaniesDTO
                    {
                        companyId = c.companyId,
                        companyName = c.companyName,
                        email = c.Users.email,
                        address = c.address,
                        city = c.city,
                        state = c.state,
                        establishedDate = c.establishedDate,
                        contactNumber = c.contactNumber,
                        profilePictureURL = c.profilePicture,
                        industry = c.industry,
                        employeeSize = c.employeeSize,
                        status = c.status,
                        reasonOfStatus = c.reasonOfStatus,
                        usersId = c.usersId
                    }).ToList();

                    response.data = pendingCompany;
                    response.message = "Pending companies found.";
                    response.status = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in getCompaniesByPendingStatus method: " + ex.Message);
                throw;
            }
        }

        public async Task<ServiceResponse<List<CompaniesDTO>>> getCompaniesByActivatedStatus()
        {
            try
            {
                var response = new ServiceResponse<List<CompaniesDTO>>();

                var activatedCompanies = await companiesRepository.getCompaniesByActivatedStatus();

                if (activatedCompanies == null || !activatedCompanies.Any())
                {
                    response.data = new List<CompaniesDTO>();
                    response.message = "No activated companies found.";
                    response.status = false;
                }
                else
                {
                    var activatedCompany = activatedCompanies.Select(c => new CompaniesDTO
                    {
                        companyId = c.companyId,
                        companyName = c.companyName,
                        email = c.Users.email,
                        address = c.address,
                        city = c.city,
                        state = c.state,
                        establishedDate = c.establishedDate,
                        contactNumber = c.contactNumber,
                        profilePictureURL = c.profilePicture,
                        industry = c.industry,
                        employeeSize = c.employeeSize,
                        status = c.status,
                        reasonOfStatus = c.reasonOfStatus,
                        usersId = c.usersId
                    }).ToList();

                    response.data = activatedCompany;
                    response.message = "Activated companies found.";
                    response.status = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in getCompaniesByActivatedStatus method: " + ex.Message);
                throw;
            }
        }

        public async Task<ServiceResponse<List<CompaniesDTO>>> getCompaniesByRejectedStatus()
        {
            try
            {
                var response = new ServiceResponse<List<CompaniesDTO>>();

                var rejectedCompanies = await companiesRepository.getCompaniesByRejectedStatus();

                if (rejectedCompanies == null || !rejectedCompanies.Any())
                {
                    response.data = new List<CompaniesDTO>();
                    response.message = "No rejected companies found.";
                    response.status = false;
                }
                else
                {
                    var rejectedCompany = rejectedCompanies.Select(c => new CompaniesDTO
                    {
                        companyId = c.companyId,
                        companyName = c.companyName,
                        email = c.Users.email,
                        address = c.address,
                        city = c.city,
                        state = c.state,
                        establishedDate = c.establishedDate,
                        contactNumber = c.contactNumber,
                        profilePictureURL = c.profilePicture,
                        industry = c.industry,
                        employeeSize = c.employeeSize,
                        status = c.status,
                        reasonOfStatus = c.reasonOfStatus,
                        usersId = c.usersId
                    }).ToList();

                    response.data = rejectedCompany;
                    response.message = "Rejected companies found.";
                    response.status = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in getCompaniesByRejectedStatus method: " + ex.Message);
                throw;
            }
        }

        public async Task<ServiceResponse<List<CompaniesDTO>>> getCompaniesByDeactivatedStatus()
        {
            try
            {
                var response = new ServiceResponse<List<CompaniesDTO>>();

                var deactivatedCompanies = await companiesRepository.getCompaniesByDeactivatedStatus();

                if (deactivatedCompanies == null || !deactivatedCompanies.Any())
                {
                    response.data = new List<CompaniesDTO>();
                    response.message = "No deactivated companies found.";
                    response.status = false;
                }
                else
                {
                    var deactivatedCompany = deactivatedCompanies.Select(c => new CompaniesDTO
                    {
                        companyId = c.companyId,
                        companyName = c.companyName,
                        email = c.Users.email,
                        address = c.address,
                        city = c.city,
                        state = c.state,
                        establishedDate = c.establishedDate,
                        contactNumber = c.contactNumber,
                        profilePictureURL = c.profilePicture,
                        industry = c.industry,
                        employeeSize = c.employeeSize,
                        status = c.status,
                        reasonOfStatus = c.reasonOfStatus,
                        usersId = c.usersId
                    }).ToList();

                    response.data = deactivatedCompany;
                    response.message = "Deactivated companies found.";
                    response.status = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in company service in getCompaniesByDeactivatedStatus method: " + ex.Message);
                throw;
            }
        }

        public async Task<CompanyDashboardStatsDTO> getCompanyDashboardStats(int usersId)
        {
            var company = await c2CDBContext.Companies.FirstOrDefaultAsync(c => c.usersId == usersId);
            if (company == null) return null;

            var companyId = company.companyId;

            var stats = new CompanyDashboardStatsDTO
            {
                totalVacancies = await c2CDBContext.Vacancies.CountAsync(v => v.companyId == companyId),
                hiringVacancies = await c2CDBContext.Vacancies.CountAsync(v => v.companyId == companyId && v.status == "hiring"),
                hiredVacancies = await c2CDBContext.Vacancies.CountAsync(v => v.companyId == companyId && v.status == "hired"),
                interviewScheduledApplications = await c2CDBContext.Applications.CountAsync(a => a.Vacancies.companyId == companyId && a.status == "interviewScheduled"),
                offeredApplications = await c2CDBContext.Applications.CountAsync(a => a.Vacancies.companyId == companyId && a.status == "offered"),
                offerAcceptedApplications = await c2CDBContext.Applications.CountAsync(a => a.Vacancies.companyId == companyId && a.status == "offerAccepted"),
                completedInterviews = await c2CDBContext.Interviews.CountAsync(i => i.Applications.Vacancies.companyId == companyId && i.interviewStatus == "completed"),
                offeredInterviews = await c2CDBContext.Interviews.CountAsync(i => i.Applications.Vacancies.companyId == companyId && i.interviewStatus == "offered"),
            };

            return stats;
        }

    }
}
