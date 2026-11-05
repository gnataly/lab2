namespace RentalService.Contracts;

public record RentalResponse(
    Guid RentalUid,
    string Username,
    Guid PaymentUid,
    Guid CarUid,
    string DateFrom,
    string DateTo,
    string Status);

public record CreateRentalRequest(
    string Username,
    Guid PaymentUid,
    Guid CarUid,
    string DateFrom,
    string DateTo);