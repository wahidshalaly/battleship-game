---
agent: agent
---

Define the task to achieve, including specific requirements, constraints, and success criteria.

The current implementation has the following issues that need to be addressed:
1. Redundant Properties: The GameStateContext class contains properties that are either redundant or not used effectively.
For example, `RemainingShipSizes`, `GamePhase`, `ShipsSunk`, `RecentHits`, and `RecentMisses` are either not necessary for the AI opponent's decision-making process or can be retrieved or calculated as needed.
2. Inefficient Data Retrieval: The GameStateAnalyzer class retrieves the game state in a way that does not make sense to me. It does not do any analysis but simply maps data from the game state to the GameStateContext. I don't want a new property like `GamePhase` while we already have `GameState`. We should use the existing concepts as much as possible, only to introduce new when it serves a need. The retrieval of game state needs to be redone to focus on the actual needs of the AI opponent and to follow DDD principles and Clean Architecture. Is it part of the domain, or repositories, or application services? This needs to be clarified.
3. Code Clarity: The code should be refactored to improve clarity and maintainability. This includes renaming variables and methods to better reflect their purpose.
4. Lack of Clarity in AI implementation: The overall design and structure of the AI opponent's game state management lack clarity. The responsibilities of each class and method should be clearly defined and documented to ensure maintainability and ease of understanding. How can we swap between the AI strategies? If we want to play a game against the RandomStrategy and another against the SematicStrategy, how easy is that to do with the current design? I don't think it's doable without changing the design.
To address these issues, the following changes should be made:

Analyze the problem, plan for redesigning, and ask me if you have any questions before proceeding.