namespace BattleshipGame.Infrastructure.Persistence.Entities;

internal class PlayerEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string IdentitySubject { get; set; } = string.Empty;
}
