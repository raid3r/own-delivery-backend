namespace OwnDeliveryApiP33.Application.DTOs;

/// <summary>Paginated response envelope.</summary>
/// <param name="Items">Page items.</param>
/// <param name="Total">Total number of matching records.</param>
/// <param name="Skip">Records skipped.</param>
/// <param name="Take">Page size used.</param>
/// <param name="HasMore">Whether more records exist after this page.</param>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Skip,
    int Take,
    bool HasMore);
