using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Services;

public record GameResult(GameId GameId, BoardSide Winner, GameState FinalState, bool IsGameOver);
