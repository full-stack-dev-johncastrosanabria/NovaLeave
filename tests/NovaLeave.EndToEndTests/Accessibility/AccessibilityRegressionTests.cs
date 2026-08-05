namespace NovaLeave.EndToEndTests.Accessibility;

public sealed class AccessibilityRegressionTests
{
    [Theory]
    [InlineData("src/NovaLeave.Web/Views/MisSolicitudes/Create.cshtml", "_ValidationSummary", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/MisSolicitudes/Edit.cshtml", "asp-validation-summary", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/Aprobaciones/Detail.cshtml", "aria-label", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/ApproverCapabilities/Capability.cshtml", "aria-labelledby", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/Shared/_Calendar.cshtml", "role=\"grid\"", "aria-label")]
    public void Critical_Razor_Pages_Retain_Accessibility_Hooks(string relativePath, string requiredA, string requiredB)
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

        Assert.Contains(requiredA, content);
        Assert.Contains(requiredB, content);
    }

    [Theory]
    [InlineData("src/NovaLeave.Web/Views/Shared/_StatusBadge.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/Shared/_Calendar.cshtml")]
    public void Status_Indicators_Use_Text_Not_Color_Alone(string relativePath)
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

        Assert.True(
            content.Contains("@Model", StringComparison.Ordinal) ||
            content.Contains("StatusLabel", StringComparison.Ordinal),
            "Status indicators must render a visible text value, not only a CSS color.");
    }

    [Fact]
    public void Login_Error_Feedback_Is_Structured_And_Accessible()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Areas/Identity/Pages/Account/Login.cshtml"));

        Assert.Contains("nl-alert-error", content);
        Assert.Contains("nl-alert-title", content);
        Assert.Contains("aria-live=\"polite\"", content);
        Assert.Contains("tabindex=\"-1\"", content);
    }

    [Theory]
    [InlineData("src/NovaLeave.Web/Views/MisSolicitudes/Balance.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/MisSolicitudes/Create.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Saldos.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Movimientos.cshtml")]
    public void Balance_Summaries_Distinguish_Usable_From_Historical_Days(string relativePath)
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

        Assert.Contains("nl-balance-primary", content);
        Assert.Contains("Acumulado total", content);
        Assert.Contains("Días gozados", content);
        Assert.Contains("Disponible", content);
        Assert.Contains("saldo que realmente puede utilizarse", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Index.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Solicitudes.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/SolicitudDetalle.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Calendario.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Saldos.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Movimientos.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Auditoria.cshtml")]
    public void HR_Read_Views_Use_Descriptive_Context_Without_Repeated_Disclaimers(string relativePath)
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

        Assert.Contains("nl-page-intro", content);
        Assert.Contains("nl-eyebrow", content);
        Assert.DoesNotContain("Solo lectura", content);
    }

    [Fact]
    public void HR_Dashboard_Uses_Task_Oriented_Titles()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/RRHH/Index.cshtml"));

        Assert.Contains("Resumen de vacaciones", content);
        Assert.Contains("Solicitudes que requieren atención", content);
        Assert.Contains("Estado de las solicitudes", content);
        Assert.Contains("Últimos cambios registrados", content);
        Assert.Contains("Consultas frecuentes", content);
    }

    [Fact]
    public void Login_Demo_Section_Shows_Only_User_Identifiers()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Areas/Identity/Pages/Account/Login.cshtml"));

        Assert.Contains("Usuarios disponibles", content);
        Assert.Contains("user@@demo", content);
        Assert.Contains("approver@@demo", content);
        Assert.Contains("hr@@demo", content);
        Assert.Contains("multi@@demo", content);
        Assert.DoesNotContain("Demo123!", content);
        Assert.DoesNotContain("Todos los contextos", content);
        Assert.DoesNotContain("Usuario y", content);
    }

    [Fact]
    public void HR_Navigation_Starts_With_An_Exact_Route_Dashboard_Link()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/Shared/_Layout.cshtml"));

        var panelIndex = content.IndexOf("(\"Panel RRHH\", \"/rrhh\"", StringComparison.Ordinal);
        var requestsIndex = content.IndexOf("(\"Solicitudes\", \"/rrhh/solicitudes\"", StringComparison.Ordinal);

        Assert.True(panelIndex >= 0, "The HR navigation must include Panel RRHH.");
        Assert.True(panelIndex < requestsIndex, "Panel RRHH must be the first HR navigation item.");
        Assert.Contains("target == \"/rrhh\"", content);
    }

    [Fact]
    public void Balance_History_Uses_A_Relaxed_Visual_Hierarchy()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/MisSolicitudes/Balance.cshtml"));

        Assert.Contains("nl-balance-hero", content);
        Assert.Contains("nl-balance-metrics", content);
        Assert.DoesNotContain("Composición del saldo", content);
    }

    [Fact]
    public void User_Request_Detail_Uses_A_Summary_First_Visual_Hierarchy()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/MisSolicitudes/Detail.cshtml"));

        Assert.Contains("nl-request-detail-hero", content);
        Assert.Contains("nl-request-detail-facts", content);
        Assert.Contains("Período solicitado", content);
        Assert.Contains("Fecha de solicitud", content);
        Assert.Contains("Motivo de la solicitud", content);
        Assert.Contains("Seguimiento", content);
        Assert.Contains("_StatusBadge", content);
        Assert.Contains("Motivo del rechazo", content);
        Assert.Contains("Editar solicitud", content);
        Assert.DoesNotContain("<dl class=\"row\">", content);
    }

    [Fact]
    public void Create_Request_Uses_An_Accessible_Insufficient_Balance_Dialog()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/MisSolicitudes/Create.cshtml"));

        Assert.Contains("data-insufficient-balance-dialog", content);
        Assert.Contains("aria-labelledby=\"insufficientBalanceTitle\"", content);
        Assert.Contains("aria-describedby=\"insufficientBalanceDescription\"", content);
        Assert.Contains("No tienes saldo suficiente", content);
        Assert.Contains("Días solicitados", content);
        Assert.Contains("Días faltantes", content);
        Assert.Contains("Ajustar período", content);
    }

    [Fact]
    public void Approver_Queue_Uses_A_Clear_Review_Hierarchy()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/Aprobaciones/Index.cshtml"));

        Assert.Contains("nl-page-intro", content);
        Assert.Contains("nl-request-summary", content);
        Assert.Contains("nl-request-table", content);
        Assert.Contains("Solicitudes por revisar", content);
        Assert.Contains("Disponible actual", content);
        Assert.Contains("Después de aprobar", content);
        Assert.Contains("Revisar solicitud", content);
        Assert.Contains("request.RequesterName", content);
        Assert.DoesNotContain("<strong>@request.RequesterId</strong>", content);
    }

    [Fact]
    public void Approver_Detail_Separates_Request_Balance_And_Decision()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/Aprobaciones/Detail.cshtml"));

        Assert.Contains("nl-approval-detail-hero", content);
        Assert.Contains("nl-balance-impact", content);
        Assert.Contains("Decisión sobre la solicitud", content);
        Assert.Contains("_StatusBadge", content);
        Assert.Contains("Aprobar solicitud", content);
        Assert.Contains("Rechazar solicitud", content);
    }

    [Fact]
    public void User_Request_List_Uses_Context_Summary_And_Explicit_Actions()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/MisSolicitudes/Index.cshtml"));

        Assert.Contains("nl-page-intro", content);
        Assert.Contains("nl-request-summary", content);
        Assert.Contains("nl-request-table", content);
        Assert.Contains("Historial de solicitudes", content);
        Assert.Contains("Período solicitado", content);
        Assert.Contains("Ver detalle", content);
    }

    [Theory]
    [InlineData("src/NovaLeave.Web/Views/Aprobaciones/History.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Auditoria.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/SolicitudDetalle.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/Movimientos.cshtml")]
    public void Timestamp_Views_Display_Costa_Rica_Local_Time(string relativePath)
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

        Assert.Contains("CostaRicaTime.ToLocalDateTime", content);
        Assert.Contains("Costa Rica", content);
        Assert.DoesNotContain(">@item.TimestampUtc<", content);
        Assert.DoesNotContain(">@record.TimestampUtc<", content);
        Assert.DoesNotContain(">@movement.EffectiveAtUtc<", content);
    }

    [Fact]
    public void Layout_Versions_The_Project_Stylesheet()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/Shared/_Layout.cshtml"));

        Assert.Contains("href=\"~/css/novaleave.css\" asp-append-version=\"true\"", content);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "NovaLeave.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
