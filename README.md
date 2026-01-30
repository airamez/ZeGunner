# ZeGunner - Unity 3D Tank Defense Game

A Unity 3D tank defense game where players defend their base from incoming enemy tanks using a turret-based cannon system.

## Project Overview

ZeGunner is a 3D first-person tower defense game built in Unity. Players control a turret/cannon positioned on a base and must destroy waves of enemy tanks before they reach the base. The game features realistic tank models, explosive effects, and a scoring system.

## Free Components Used

### 3D Models & Assets
- **Tank 3D Model** - Military tank with detailed textures and moving parts
  - Source: Unity Asset Store (Free)
  - Location: `Assets/Isle of Assets/Tank 3D Model/`
  - Includes: Tank prefab, materials, textures

### Visual Effects
- **WarFX Explosion Pack** - Professional explosion effects and particle systems
  - Source: Unity Asset Store (Free)
  - Location: `Assets/JMO Assets/WarFX/`
  - Includes: Multiple explosion prefabs, smoke effects, particle systems

### Audio Assets
- **Grenade Sound FX** - Realistic explosion sound effects
  - Source: Free sound pack
  - Location: `Assets/Grenade Sound FX/Grenade/`
  - Includes: 10 different explosion sound variations

### Unity Systems
- **Unity Input System** - Modern input handling for keyboard and mouse
- **Unity Particle System** - Built-in particle effects for explosions
- **Unity UI System** - Score display and game interface
- **Unity Physics** - Collision detection and projectile physics

## Game Features

### Core Gameplay
- **First-person turret control** with mouse aiming
- **Vertical camera movement** using W/S keys with height limits
- **Projectile system** with sphere and rocket options
- **Tank spawning system** with configurable waves
- **Collision detection** for accurate tank destruction

### Visual & Audio
- **Dynamic explosion effects** with fallback particle systems
- **Random explosion sounds** for variety
- **3D spatial audio** positioned at explosion locations
- **Smart rendering pipeline compatibility** (detects and fixes purple materials)

### UI & Scoring
- **Real-time score tracking**
- **Statistics display** (tanks destroyed, accuracy, longest distance)
- **Tank reach base counter**
- **Clean, readable UI overlay**

## Technical Implementation

### Key Scripts
- **`CannonController.cs`** - Turret control, camera movement, projectile firing
- **`TankSpawner.cs`** - Enemy tank spawning and configuration
- **`RocketCollision.cs`** - Projectile collision and tank destruction
- **`ExplosionManager.cs`** - Explosion effects and audio management
- **`ScoreManager.cs`** - Game statistics and scoring
- **`Tank.cs`** - Individual tank movement and behavior

### Architecture
- **Singleton patterns** for managers (ScoreManager, ExplosionManager)
- **Component-based design** for modular functionality
- **Event-driven scoring** system
- **Resource loading** for audio and effects
- **Automatic fallback systems** for rendering compatibility

## Controls

### Movement
- **Mouse** - Aim turret/cannon
- **Left Click** - Fire projectile
- **W Key** - Move camera/turret up
- **S Key** - Move camera/turret down (with minimum height limit)

### Game Settings
- **Tank Scale** - Adjustable tank size in TankSpawner
- **Vertical Speed** - Camera movement speed in CannonController
- **Minimum Height** - Prevents camera going below base level

## Project Structure

```
Assets/
├── Scripts/                    # C# game scripts
├── Enemies/                    # Tank prefabs
├── Resources/                  # Runtime-loaded assets
├── JMO Assets/                 # WarFX explosion effects
├── Grenade Sound FX/          # Explosion audio
├── Isle of Assets/            # Tank 3D models
└── Scenes/                    # Game scenes
```

## Setup Instructions

1. **Open in Unity** - Requires Unity 2021.3 or later
2. **Import Assets** - All free components included in project
3. **Configure Scene** - TankSpawner and ExplosionManager are pre-configured
4. **Play Game** - Ready to play with default settings

## Compatibility

- **Unity Version**: 2021.3 LTS or later
- **Render Pipeline**: Compatible with Built-in, URP, and HDRP
- **Platform**: Windows, Mac, Linux
- **Input**: Mouse and Keyboard

## Notes

- The game includes automatic material fixing for rendering pipeline compatibility
- Explosion effects use intelligent fallback systems for visual consistency
- All assets are free-to-use with appropriate licensing
- Project demonstrates clean Unity development patterns and best practices

---

**Developed with Unity 3D using free asset store components**
