using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Balances.Queries;
using NovaLeave.Application.Calendars.GetPersonalCalendar;
using NovaLeave.Application.Requests.CreateVacationRequest;
using NovaLeave.Application.Requests.EditPendingRequest;
using NovaLeave.Application.Requests.Queries;
using NovaLeave.Domain.Services;

namespace NovaLeave.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<OverlapPolicy>();
        services.AddScoped<GetMyRequestsQueryHandler>();
        services.AddScoped<GetMyRequestDetailQueryHandler>();
        services.AddScoped<GetMyBalanceQueryHandler>();
        services.AddScoped<GetPersonalCalendarQueryHandler>();
        services.AddScoped<CreateVacationRequestHandler>();
        services.AddScoped<EditPendingRequestHandler>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
