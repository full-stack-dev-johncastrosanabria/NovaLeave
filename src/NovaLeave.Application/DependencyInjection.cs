using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Approvals.ApproveRequest;
using NovaLeave.Application.Approvals.DeactivateApprovedRequest;
using NovaLeave.Application.Approvals.History;
using NovaLeave.Application.Approvals.Queries;
using NovaLeave.Application.Approvals.RejectRequest;
using NovaLeave.Application.Balances.Queries;
using NovaLeave.Application.Calendars.GetPersonalCalendar;
using NovaLeave.Application.Calendars.GetApproverCalendar;
using NovaLeave.Application.HR.Audit;
using NovaLeave.Application.HR.ApproverCapabilities.ListApproverCapabilities;
using NovaLeave.Application.HR.ApproverCapabilities.ToggleApproverCapability;
using NovaLeave.Application.HR.Balances;
using NovaLeave.Application.HR.Calendar;
using NovaLeave.Application.HR.Requests;
using NovaLeave.Application.Requests.CreateVacationRequest;
using NovaLeave.Application.Requests.EditPendingRequest;
using NovaLeave.Application.Requests.Queries;
using NovaLeave.Application.Audit;
using NovaLeave.Application.System.CancelTimedOutRequests;
using NovaLeave.Application.System.ExecuteMonthlyAccrual;
using NovaLeave.Application.Observability;
using NovaLeave.Domain.Services;

namespace NovaLeave.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<OverlapPolicy>();
        services.AddScoped<MonthlyAccrualPolicy>();
        services.AddScoped<GetMyRequestsQueryHandler>();
        services.AddScoped<GetMyRequestDetailQueryHandler>();
        services.AddScoped<GetMyBalanceQueryHandler>();
        services.AddScoped<GetPersonalCalendarQueryHandler>();
        services.AddScoped<GetApproverCalendarQueryHandler>();
        services.AddScoped<GetApproverQueueQueryHandler>();
        services.AddScoped<GetApproverRequestDetailQueryHandler>();
        services.AddScoped<GetApproverHistoryQueryHandler>();
        services.AddScoped<GetHRRequestListQueryHandler>();
        services.AddScoped<GetHRRequestDetailQueryHandler>();
        services.AddScoped<GetHRCalendarQueryHandler>();
        services.AddScoped<GetHRBalancesQueryHandler>();
        services.AddScoped<GetHRBalanceMovementsQueryHandler>();
        services.AddScoped<GetHRAuditLogQueryHandler>();
        services.AddScoped<ListApproverCapabilitiesQueryHandler>();
        services.AddScoped<GetApproverCapabilityQueryHandler>();
        services.AddScoped<ToggleApproverCapabilityCommandHandler>();
        services.AddScoped<ApproveRequestHandler>();
        services.AddScoped<DeactivateApprovedRequestHandler>();
        services.AddScoped<RejectRequestHandler>();
        services.AddScoped<SystemAuditWriter>();
        services.AddScoped<CancelTimedOutRequestsHandler>();
        services.AddScoped<ExecuteMonthlyAccrualHandler>();
        services.AddScoped<CreateVacationRequestHandler>();
        services.AddScoped<EditPendingRequestHandler>();
        services.AddScoped<IBusinessInvariantMonitor, BusinessInvariantMonitor>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
