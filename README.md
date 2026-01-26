> **About this project:**
> This Battleship Game is more than a coding exercise, it's an ongoing experiment and learning playground. The real purpose behind this project is to deepen my hands-on experience with Domain-Driven Design (DDD), Engineering acceleration with AI, and explore how to build practical AI-powered applications with GenAI.
> I expect to add more design patterns and architecture styles in the future, but based on need and my limited time.
>

# Battleship Game Documentation

This a comprehensive documentation for the Battleship Game project, providing detailed insights into the system architecture, domain analysis, and design decisions.


## Documentation Structure

### 📋 [analysis.md](./analysis.md)
**Domain Analysis & Business Requirements**

Provides a comprehensive analysis of the Battleship game domain model, detailing the core entities, value objects, and business rules that govern the game logic. This document serves as the foundation for understanding the business requirements and domain complexity.

**Key Topics:**
- Domain-Driven Design (DDD) concepts
- Core entities and aggregates
- Business rules and constraints
- Domain events pattern
- Error handling strategy

### � [bounded-context-analysis.md](./bounded-context-analysis.md)
**Bounded Context Alignment Assessment**

Analyzes the Battleship Game Bounded Context diagram and compares it with the current codebase implementation. This document identifies alignment, gaps, and provides actionable recommendations for bringing the implementation in line with the domain design.

**Key Topics:**
- Bounded context diagram analysis
- Current vs. expected implementation comparison
- Domain events gap analysis
- API endpoint coverage assessment
- Implementation roadmap and priorities

### �🏗️ [architecture.md](./architecture.md)
**High-Level System Architecture**

Offers an executive overview of the system architecture, technology stack, and architectural principles. This document is ideal for stakeholders, new team members, and architects who need to understand the overall system design.

**Key Topics:**
- Clean Architecture principles
- Technology stack and tools
- Layer responsibilities and dependencies
- Design patterns and best practices
- Performance and security considerations
- Deployment strategy

### 🎨 [design.md](./design.md)
**Detailed System Design**

Contains detailed class diagrams, component relationships, and technical design specifications. This document is essential for developers who need to understand the implementation details and code structure.

**Key Topics:**
- Domain model class diagrams
- Application layer design
- Web API layer structure
- SOLID principles implementation
- Configuration and constraints
- Error handling patterns## How to Use This Documentation

## Architecture Principles

The system follows these key principles:

- **Clean Architecture**: Clear separation of concerns with dependency inversion
- **Domain-Driven Design**: Business logic at the center with ubiquitous language
- **SOLID Principles**: Maintainable and extensible code structure

## Technology Stack

- **.NET 8.0**: Modern C# with latest features
- **ASP.NET Core**: Web API framework
- **xUnit + FluentAssertions**: Testing framework
- **Swagger/OpenAPI**: API documentation

## Quick Reference

### Domain Entities
- **Game**: Aggregate root managing the entire game lifecycle
- **Board**: Contains cells and ships for each player
- **Ship**: Represents individual ships with hit tracking
- **Cell**: Board positions with coordinate system
- **Player**: Aggregate root for managing players and history tracking

### Key Enumerations
- **GameState**: None, New, Ready, Started, GameOver
- **BoardSide**: None, Player, Opponent
- **CellState**: None, Clear, Occupied, Hit, Missed
- **ShipKind**: None, Destroyer(2), Submarine(3), Cruiser(3), Battleship(4), Carrier(5) - ship sizes in parentheses
- **ShipOrientation**: None, Vertical, Horizontal

### Business Rules
- Board sizes: 10x10 (default) to 26x26 (maximum)
- Exactly 5 ships per board (one of each kind)
- Ships must be placed in straight lines only
- Turn-based gameplay: Player attacks first, then opponent, until Game Over.
- `TargetSide` property controls which board can be attacked
- Cannot attack the same cell twice
- Cannot attack out of turn (wrong board)
- Game ends when all ships on one board are sunk

## Contributing to Documentation

When updating documentation:

1. **Keep it Current**: Update docs when code changes
2. **Use Clear Language**: Write for your intended audience
3. **Include Diagrams**: Visual representations aid understanding
4. **Validate Mermaid**: Ensure diagrams render correctly
5. **Cross-Reference**: Link related concepts between documents

## Mermaid Diagrams

This documentation uses Mermaid for diagrams. To view them properly:
- Use GitHub's built-in Mermaid rendering
- Use VS Code with Mermaid extension
- Use online Mermaid editor: https://mermaid.live/

## Questions or Feedback

For questions about the architecture or design decisions, please:
1. Check existing documentation first
2. Review the codebase for implementation details
3. Raise questions during code reviews
4. Update documentation when knowledge gaps are identified

### 📁 Documentation Structure

```
README.md                           # Documentation guide and navigation
docs/
├── architecture.md                 # High-level system architecture
├── analysis.md                     # Domain analysis and business rules
├── bounded-context-analysis.md     # Bounded context alignment assessment
├── design.md                       # Detailed technical design with diagrams
├── openapi.yaml                    # OpenAPI specification for the Web API
└── technical-debt-and-roadmap.md   # Technical debt analysis and future roadmap
```

### How to Generate OpenAPI Documentation

**Restore local tools**

`dotnet tool restore`

**Build the project first**

`dotnet build ./src/BattleshipGame.WebAPI/BattleshipGame.WebAPI.csproj`

**Generate the OpenAPI YAML document**

`dotnet swagger tofile --output ./docs/openapi.yaml --yaml ./src/BattleshipGame.WebAPI/bin/Debug/net8.0/BattleshipGame.WebAPI.dll v1`
