﻿using System;
using College2Career.DTO;
using College2Career.Models;

namespace College2Career.Repository
{
    public interface IApplicationsRepository
    {
        Task<bool> alreadyAppliedForVacancy(int studentId);
        Task<bool> isOfferAccepted(int studentId);
        Task newApplications(Applications applications);
        Task<List<Applications>> getAllAppliedApplicationsByCompanyId(int companyId);
        Task<Applications> isApplicationsExist(int applicationId);
        Task updateApplicationsStatusByCompany(Applications applications);
        Task<List<Applications>> getAllAppliedApplicationsByStudentId(int studentId);
        Task<Applications> getApplicationDetailsById(int applicationId);
        Task<Applications> updateStatusToOfferedByStudentId(int studentId);
        Task<Applications> updateStatusToOfferAcceptedStudentId(int applicationId);
        Task<Applications> updateStatusToOfferRejectedStudentId(int applicationId, ApplicationsDTO applicationsDTO);
    }
}