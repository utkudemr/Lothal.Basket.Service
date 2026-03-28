namespace Lothal.BuildingBlocks.Common;

public record PagedResult<T>(IEnumerable<T> Items, long TotalCount);
