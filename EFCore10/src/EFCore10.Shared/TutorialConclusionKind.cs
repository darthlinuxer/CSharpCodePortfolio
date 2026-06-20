namespace EFCore10.Shared;

/// <summary>
/// Describes the semantic weight of a tutorial experiment conclusion.
/// </summary>
public enum TutorialConclusionKind
{
    /// <summary>
    /// A neutral conclusion.
    /// </summary>
    Neutral,

    /// <summary>
    /// A conclusion that confirms the recommended approach.
    /// </summary>
    Success,

    /// <summary>
    /// A conclusion that highlights a caution or trade-off.
    /// </summary>
    Warning,

    /// <summary>
    /// A conclusion that intentionally demonstrates a failure mode.
    /// </summary>
    Failure
}
