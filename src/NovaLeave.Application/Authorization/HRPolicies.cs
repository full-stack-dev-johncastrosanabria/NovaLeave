using NovaLeave.Application.Common.Errors;

namespace NovaLeave.Application.Authorization;

public static class HRPolicies
{
    public const string RequireActiveHR = "RequireActiveHR";

    public static Error? CanReadOrganizationData(bool isActiveHR)
    {
        return isActiveHR ? null : new Error(ErrorCodes.Forbidden, "RRHH activo requerido.");
    }
}
