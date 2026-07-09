# Lionrise Protocol

An original, portrait-first civic sci-fi strategy game. Guide the orbital city-state of Aster Lion through fourteen national turning points in a run designed to last about two minutes.

## Open and play

1. Install Unity 6.3 LTS in Unity Hub.
2. Add this folder as a Unity project and open it (Unity Hub may patch the exact 6.3 LTS revision in `ProjectVersion.txt`).
3. Allow the first import to finish. The editor bootstrap creates `Boot.unity`, `Game.unity`, and the build settings.
4. Open `Assets/_Game/Scenes/Game.unity` (or any scene) and press Play.

The UI is created at runtime, so there are no opaque prefab dependencies. Drag the card left or right; on desktop you can also use `A`/`D` or the arrow keys. `Escape` opens the journey menu, which contains status, objectives, active effects, and accessibility options.

To build a macOS player, use **Tools → Lionrise → Build macOS Player**. The output is written to `Builds/macOS/Lionrise Protocol.app`.

## Verification without Unity

```bash
python3 Tools/content_tool.py validate
python3 Tools/content_tool.py simulate --runs 50000
python3 -m unittest discover -s Tools/tests -v
```

The command-line harness reads the same card JSON as the Unity game. It validates content rules, checks that endings are reachable, and runs deterministic random/balanced bot simulations.

## Project map

- `Assets/_Game/Scripts/Core`: pure gameplay state and rules
- `Assets/_Game/Scripts/Cards`: card loading, conditions, and weighted draw
- `Assets/_Game/Scripts/UI`: runtime portrait UI and swipe interaction
- `Assets/StreamingAssets/Cards/cards.json`: launch-MVP card content
- `Tools/content_tool.py`: validator and simulation harness
- `Docs`: design, writing, and technical notes

No extracted Reigns assets, copy, music, or serialized UI are included. Lionrise uses its own generated advisor atlas and original procedural presentation, measured against a private reference export for layout and animation fidelity.
