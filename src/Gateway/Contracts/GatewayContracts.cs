namespace Gateway.Contracts;

// Публичные контракты Gateway (совпадают с OpenAPI-спецификацией варианта 3).

public record CarResponse(
    Guid CarUid,
    string Brand,
    string Model,
    string RegistrationNumber,
    int? Power,
    string? Type,
    int Price,
    bool Available);

public record PaginationResponse(
    int Page,
    int PageSize,
    long TotalElements,
    IReadOnlyList<CarResponse> Items);

public record CarInfo(
    Guid CarUid,
    string Brand,
    string Model,
    string RegistrationNumber);

public record PaymentInfo(
    Guid PaymentUid,
    string Status,
    int Price);

public record RentalResponse(
    Guid RentalUid,
    string Status,
    string DateFrom,
    string DateTo,
    CarInfo Car,
    PaymentInfo Payment);

public record CreateRentalRequest(
    Guid CarUid,
    string DateFrom,
    string DateTo);

public record CreateRentalResponse(
    Guid RentalUid,
    string Status,
    Guid CarUid,
    string DateFrom,
    string DateTo,
    PaymentInfo Payment);

// Внутренние DTO — зеркала ответов внутренних сервисов (Rental, Payment).

public record RentalInternal(
    Guid RentalUid,
    string Username,
    Guid PaymentUid,
    Guid CarUid,
    string DateFrom,
    string DateTo,
    string Status);