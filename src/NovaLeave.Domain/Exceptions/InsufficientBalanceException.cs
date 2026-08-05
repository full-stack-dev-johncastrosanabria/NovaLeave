namespace NovaLeave.Domain.Exceptions;

public sealed class InsufficientBalanceException : InvalidOperationException
{
    public InsufficientBalanceException(int availableDays, int requestedDays)
        : base("El saldo disponible no es suficiente para reservar los días solicitados.")
    {
        AvailableDays = availableDays;
        RequestedDays = requestedDays;
    }

    public int AvailableDays { get; }

    public int RequestedDays { get; }
}
