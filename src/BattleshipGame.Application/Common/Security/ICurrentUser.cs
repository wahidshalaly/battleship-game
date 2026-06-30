namespace BattleshipGame.Application.Common.Security;

/// <summary>
/// Exposes the authenticated caller's identity to the application layer. Implemented in the
/// web layer from the current request's JWT claims, keeping the application provider-agnostic.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The authenticated subject (the token 'sub' claim), or <c>null</c> when unauthenticated.
    /// </summary>
    string? SubjectId { get; }
}
