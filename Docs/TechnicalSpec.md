# Technical Specification

## Runtime

Target editor: Unity 6.3 LTS. The project uses C#, UGUI, Unity Input System, `JsonUtility`, and StreamingAssets. It has no third-party runtime dependency.

On first import, `ProjectBootstrap` creates empty `Boot.unity` and `Game.unity` scenes and assigns both to build settings. Runtime initialization creates the persistent app object and procedural UI. The Boot scene routes to Game after load.

## System map

| System | Responsibility |
| --- | --- |
| `GameStateManager` | Run orchestration, resume, choice flow, profile updates |
| `RunPlanGenerator` | Fixed fourteen-slot national arc |
| `CardDatabase` | StreamingAssets JSON load on desktop and mobile |
| `ConditionEvaluator` | Flags, ranges, and slot availability |
| `WeightedDeck` | Cooldowns, follow-ups, crises, deterministic weighted draw |
| `EffectResolver` | Meter, hidden-state, flag, and unlock changes |
| `NationalDevelopment` | Multi-year capacity milestone for each arc |
| `EndingResolver` | Immediate extremes, Tier Score, audit classification |
| `SaveSystem` | Atomic JSON profile and active-run saves |
| `LionriseUI` | Runtime portrait UI and accessibility settings |
| `CardDragController` | Swipe thresholds, snap-back, keyboard commit |
| `CivicAudio` | Original procedural ambience and interaction cues |
| `ContentValidator` | Runtime/editor content rules |

## Data contract

Cards live in `Assets/StreamingAssets/Cards/cards.json`. The format uses explicit meter and hidden-state fields so Unity can deserialize it without a dictionary-aware third-party JSON package. Optional arrays and conditions are null-safe at runtime.

Every effect is clamped to 0–100. Immediate endings are checked after both the chosen effect and the multi-year arc milestone. A crisis advances the planned slot and therefore still advances historical time and baseline capacity.

## Saves

`profile.json` and `run.json` are written beneath `Application.persistentDataPath`. The active run records its plan, seed, current card, meters, hidden state, flags, follow-ups, seen cards, and current slot. The profile records settings, run totals, card history, endings, and unlock collections.

## Verification

The command-line harness mirrors the Unity conditions, draw, effects, arc progress, immediate failures, and final resolver. Use:

```bash
python3 Tools/content_tool.py validate
python3 Tools/content_tool.py simulate --runs 50000
python3 -m unittest discover -s Tools/tests -v
```

Verified with Unity `6000.3.18f1` on Apple silicon. The project completes a clean import, resolves all packages, compiles editor and player assemblies, validates all 90 cards, and produces a signed local macOS player bundle.
