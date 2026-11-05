namespace CarsService.Contracts;

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