# Coin Collector 3D - Unity Game

A 3D platformer game built with Unity where you roll a ball across procedurally generated platforms to collect all coins and reach the highest score possible.

## Gameplay

- **Objective**: Collect all coins (default: 10) scattered across floating platforms
- **Lives**: You start with 3 lives. Falling off the platforms costs a life
- **Score**: Each coin awards 100 points; complete the level to win
- **Pause**: Press `P` to pause/resume the game

## Controls

| Action | Key |
|--------|-----|
| Move | `W A S D` / Arrow Keys |
| Jump | `Space` |
| Rotate Camera | Mouse (locked) |
| Zoom Camera | Mouse Scroll Wheel |
| Pause | `P` |
| Unlock Cursor | `Escape` |

## Project Structure

```
Assets/
├── Scripts/
│   ├── PlayerController.cs   # Ball movement, jump, ground check
│   ├── CameraController.cs   # Third-person orbit camera with collision
│   ├── GameManager.cs        # Score, lives, game state, events
│   ├── CoinPickup.cs         # Coin animation, pickup detection, effects
│   ├── UIManager.cs          # HUD and menu panel management
│   ├── LevelGenerator.cs     # Procedural platform/coin/light generation
│   └── PlatformMover.cs      # Oscillating moving platforms
├── Scenes/
│   └── MainScene.unity       # Main game scene
├── Materials/                # (add custom materials here)
├── Prefabs/                  # (add prefabs here)
└── Audio/                    # (add sound clips here)

ProjectSettings/
├── ProjectSettings.asset     # Core Unity project settings
├── InputManager.asset        # Keyboard/mouse input bindings
├── TagManager.asset          # Tags: Player, Ground, Coin, DeathZone
└── QualitySettings.asset     # Graphics quality presets

Packages/
└── manifest.json             # Unity Package Manager dependencies
```

## Unity Version

Developed for **Unity 2022.3 LTS** (also compatible with Unity 2021.3+).

Required packages (via Package Manager):
- TextMeshPro `3.0.6`
- Input System `1.7.0`
- Cinemachine `2.9.7`

## How to Open

1. Install **Unity Hub** and Unity 2022.3 LTS
2. Click **Add project from disk** and select this folder
3. Open `Assets/Scenes/MainScene.unity`
4. Press **Play** — the level generates automatically at runtime

## Architecture

### Game Flow
```
Start
  └─> GameManager.StartGame()
        └─> LevelGenerator.GenerateLevel()
              ├─> Platforms + Moving Platforms
              ├─> Coins (CoinPickup)
              └─> Player Sphere (PlayerController)
                    └─> Camera (CameraController)

CoinPickup.Collect()
  └─> GameManager.CollectCoin()
        ├─> AddScore()
        └─> [if all collected] LevelCompleteRoutine()

PlayerController.Die()
  └─> GameManager.OnPlayerDied()
        ├─> [lives > 0] RespawnRoutine()
        └─> [lives == 0] GameOverRoutine()
```

### Design Patterns Used
- **Singleton**: `GameManager`, `PlayerController`
- **Observer / Events**: `GameManager` fires C# events; `UIManager` subscribes
- **Component**: Each feature isolated in its own MonoBehaviour
- **Factory**: `LevelGenerator` spawns all game objects at runtime

## Customisation

All tuning values are exposed as `[SerializeField]` fields in the Inspector:

| Script | Key Parameters |
|--------|---------------|
| `PlayerController` | `moveSpeed`, `jumpForce`, `maxVelocity` |
| `CameraController` | `distance`, `mouseSensitivity`, `minVerticalAngle` |
| `GameManager` | `totalCoins`, `startingLives`, `respawnDelay` |
| `CoinPickup` | `scoreValue`, `rotationSpeed`, `bobHeight` |
| `LevelGenerator` | `platformCount`, `platformSpacing`, `coinsPerPlatform` |
| `PlatformMover` | `moveOffset`, `speed`, `moveCurve` |

## Extending the Game

- **New level**: Duplicate `MainScene.unity`, adjust `LevelGenerator` parameters in the Inspector
- **Power-ups**: Create a new `PowerupPickup.cs` following the `CoinPickup` pattern
- **Enemies**: Add a Rigidbody sphere + simple patrol script + death trigger on `PlayerController`
- **Audio**: Assign `AudioClip` fields in `CoinPickup` Inspector slots and add an `AudioSource` to the player

## License

MIT — free to use, modify, and distribute.
