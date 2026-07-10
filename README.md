# Helm: Lionrise Protocol

Helm is a 16-decision sci-fi nation-builder built on the recovered swipe-card game engine. A complete timeline takes roughly two minutes. Replays explore different balances between openness, state capacity, and human development.

## Open and build

- Unity: `6000.3.18f1`
- Open this repository folder as the Unity project.
- In Unity, choose **Helm → Build macOS**.
- The app is written to `Builds/macOS/Helm.app`.

Command-line build:

```sh
/Applications/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /Users/ceron/Developer/Projects/helm \
  -executeMethod HelmBuild.BuildMacOS \
  -logFile /Users/ceron/Developer/Projects/helm/Logs/helm-build.log
```

## Edit the campaign

The readable source of truth is in [`Assets/Helm/Story`](Assets/Helm/Story):

- `helm_cards.csv`: 16 decisions and five ending cards.
- `helm_characters.csv`: new names, roles, and descriptions mapped onto native character rigs.
- `helm_objectives.csv`: replay objectives.
- `helm_ui_overrides.csv`: Helm title, run-summary, tutorial, and ending terminology.

Tables use semicolons because the recovered runtime expects that schema. Do not put semicolons inside prose. Save a table, then choose **Helm → Compile campaign data**. A build runs the same compiler automatically.

The compiler validates column counts, unique card IDs, cast tags, the exact 16-decision/five-ending structure, all 65,536 choice paths, safe meter bounds, and ending reachability before writing the encoded runtime resources.

## Design map

The inherited four meters are reframed for Lionrise:

- Power: grid and state capacity.
- Oxygen: habitat and environmental resilience.
- People: population, talent, and social trust.
- Hull: infrastructure and productive capacity.

The campaign tracks three invisible philosophies: `nb_open`, `nb_order`, and `nb_human`. Their final balance selects one of five timelines:

- The Lionrise Protocol
- The Iron Helm
- The Open Constellation
- The Orbital Garden
- The Last Light

New characters reuse the original game's native vector rigs, animation, eyes, voice texture, and card presentation. That keeps the visual silhouette consistent while the names, roles, story, sequence, locations, objectives, music cues, and SFX are Helm-specific.

## Verification

Useful Unity menu commands:

- **Helm → Compile campaign data**
- **Helm → Audit project**
- **Helm → Build macOS**

Development/editor shortcuts: F6 opens the first card, F7 opens the labor crisis, F8 opens the solar-strike decision, and F9 dumps current state.
