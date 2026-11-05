namespace PaymentService.Contracts;

public record PaymentResponse(
    Guid PaymentUid,
    string Status,
    int Price);

public record CreatePaymentRequest(int Price);