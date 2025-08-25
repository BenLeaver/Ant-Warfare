# Ant Warfare

*A real-time strategy game where players control ant colonies and compete for survival.*

## Overview
- RTS game developed in **Unity (C#)**.
- Control an ant colony: gather food, expand, and battle rival colonies.
- Features both **singleplayer** and **multiplayer** modes.

## Screenshots / GIFs
(Screenshots will be added here).

## Features

- ### Colony Management
    - Spawn ants: **workers**, **soldiers**, and unique types.
    - Manage **food resources** and **colony size**.
    - Use **pheromone markers** to command ants, enabling complex trails and automated behaviours.
- ### Combat & Survival
    - Start with small skirmishes, escalating into large-scale battles.
    - Destroy an enemy queen to eliminate their colony.
    - Last colony standing wins.
- ### Upgrades & Progression
    - Tiered upgrade system; only one upgrade per tier can be purchased.
- ### Multiplayer
    - Compete against other players on different maps.
    - Currently LAN-only; online multiplayer is planned.

## Technologies & Packages Used
- **Unity 2022.3**
- **Unity Netcode for GameObjects** - Multiplayer networking.
- **ParrelSync** - Multiplayer testing.
- **Unity NavMesh** - Ant pathfinding.
- **TextMeshPro** - UI Text.
- **Post-Processing Stack** - Visual effects.

## Development & Learning Outcomes
- **Multiplayer networking:** Implemented with Unity Netcode, including RPCs for syncing game state.
- **Game Systems & AI:** Designed upgrades, pheromone commands, AI behavior.
- **UI:** Develop intuitive menus using Unity UI & TextMeshPro.
- **Iterative development:** prototyping, testing, balancing mechanics.
- **Clean, Scalable Code:** Debugged, refactored, and organised code to ensure maintainability and long-term project growth.

## Limitations & Future Plans
- **Local Multiplayer Only:** Online networking could be added.
- **Performance Constraints:** Optimisation would enable larger colonies and smoother gameplay.
- **UI & User Experience:** Some interfaces, such as pheromone marker selection, could be more intuitive.
- **Content Expansion:** Architecture supports the future addition of new species, unit types, and maps.

## Project Structure (Simplified)
```
Assets/
└── Ant Warfare/
    ├── Archived/
    ├── Art/
    ├── Audio/
    ├── Post Processing/
    ├── Prefabs/
    ├── Scenes/
    └── Scripts/
        ├── Core/
        ├── Gameplay/
        ├── Multiplayer/
        └── Singleplayer/
```

## How to Play
(Instructions on how to play will be added here).
