# Germ Sweep 3D Prototype

This folder contains the self-contained Unity 2022.3 LTS Android prototype for **Germ Sweep 3D**.

## How to run

1. Open `Assets/Scenes/Main.unity`.
2. Enter Play Mode. `GameManager` bootstraps the save system, services, runtime UI, level manager, camera follow, and level content.
3. Optional: run **Germ Sweep 3D/Rebuild Main Scene And Placeholder Prefabs** in the Unity editor to generate physical placeholder prefab assets and refresh the Main scene hierarchy.

## Manual test checklist

- Start game from HomePanel.
- Play level 1 with mouse drag in Editor or one-finger touch on Android.
- Win level 1 after delivering waste and healing the patient.
- Buy one upgrade from UpgradePanel and confirm coins decrease/level increases.
- Restart the active level from PausePanel or FailPanel.
- Pause and resume gameplay.
- Let the timer expire to verify Lose flow.
- Use Double Coins rewarded ad on WinPanel.
- Play level 5, collect Sanitizer Crystals, defeat the boss, and heal the critical patient.
