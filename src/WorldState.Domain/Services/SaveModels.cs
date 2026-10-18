using WorldState.Domain.Models;

namespace WorldState.Domain.Services;

public enum SaveErrorCode
{
    None = 0,
    FileNotFound,
    CorruptedSaveData,
    SaveTooNew,
    IoError,
    Unknown
}

public record SaveSlotMetadata(
    string SlotId,
    string DisplayName,
    string CountryId,
    string CountryName,
    DateOnly InGameDate,
    DateTime RealTimeSavedUtc,
    int SchemaVersion,
    bool IsCorrupted = false,
    string? ErrorMessage = null,
    long FileSizeBytes = 0
);

public record SaveSlotResult(
    bool IsSuccess,
    string SlotId,
    SaveErrorCode ErrorCode = SaveErrorCode.None,
    string? ErrorMessage = null,
    SaveSlotMetadata? Metadata = null
)
{
    public static SaveSlotResult Success(string slotId, SaveSlotMetadata metadata) =>
        new(true, slotId, SaveErrorCode.None, null, metadata);

    public static SaveSlotResult Failure(string slotId, SaveErrorCode code, string message) =>
        new(false, slotId, code, message);
}

public record LoadSlotResult(
    bool IsSuccess,
    string SlotId,
    GameSession? Session = null,
    SaveErrorCode ErrorCode = SaveErrorCode.None,
    string? ErrorMessage = null
)
{
    public static LoadSlotResult Success(string slotId, GameSession session) =>
        new(true, slotId, session, SaveErrorCode.None);

    public static LoadSlotResult Failure(string slotId, SaveErrorCode code, string message) =>
        new(false, slotId, null, code, message);
}
