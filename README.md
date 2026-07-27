# Helm - Scenario S1: Big Brother Is Watching

Helm is a 16-decision sci-fi ruler-capability examination built on the recovered swipe-card engine. The player begins already inside Scenario S1, with no successor yet chosen. A first assessment takes roughly eight to twelve minutes: the candidate must prove they may be fit to become the next ruler by transforming Lionrise from third-world conditions into a durable first-world civilization within twenty simulated years.

The seven-card opening is led by ORISON-9: HELM identifies the candidate's place inside S1, reveals Earth and Lionrise in 3024, explains the third-to-first leadership test, then defines the four visible gauges and swipe preview before Central forces the first governing decision. Its visual scenarios change with the briefing, but one continuous music bed prevents it from feeling like a trailer montage. Pip provides the human through-line as a pressure-room child who later becomes Lionrise's teacher and civic witness.

Scenario S1 is set in 3024. Thirty billion people inhabit a highly monitored planetary city governed by centralized AI allocation. Scarcity, elite orbital settlements, a declining biosphere, and pervasive nano-surveillance create the conditions for the scenario's philosopher-ruler test.

The development sequence parallels the pressures Lee Kuan Yew's Singapore faced without using historical names directly: forced separation, water dependence, mass housing, anti-corruption, national service, foreign investment, labor coordination, common education, social integration, port strategy, land scarcity, greening, meritocracy, surveillance, and succession.

The player is explicitly being tested. HELM records each choice and grades whether the resulting state combines prosperity, governing capacity, and public trust—or merely looks first-world from orbit.

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

## Edit the scenario

The readable source of truth is in [`Assets/Helm/Story`](Assets/Helm/Story):

- `SCENARIO_S1.md`: setting, historical parallel, sequence, and grading rubric.
- `helm_cards.csv`: a seven-card opening, 16 policy decisions, eight later text interludes, two minigames, conditional callbacks, and five result cards.
- `helm_characters.csv`: scenario roles mapped onto native animated character rigs.
- `helm_objectives.csv`: replay objectives.
- `helm_ui_overrides.csv`: scenario title, assessment language, tutorial, and result labels.

Tables use semicolons because the recovered runtime expects that schema. Do not put semicolons inside prose. Save a table, then choose **Helm → Compile campaign data**. A build runs the same compiler automatically.

The compiler validates column counts, unique card IDs, the Scenario S1 identity, cast tags, the exact 16-decision/two-minigame/five-result structure, graph continuity, all 262,144 policy-and-performance paths, safe meter bounds, and result reachability.

## Playable sequences

- **Equatorial Signal Concert:** the recovered rhythm game becomes the live launch of Lionrise's rebuilt space elevator. Success or failure changes the port-opening aftermath.
- **Allocation Pulse:** a new twelve-second crisis game built in the recovered UI style. The player tracks a moving allocation window to stop a hacked water network from cascading.

After the purposeful visual sequence in the opening, the campaign holds a location for an entire act, as the original game does: Lionrise Command, Civic Forum, Signal Spine, Civic Eye, and HELM Observation are recurring anchors. Text-only cards carry time jumps and consequences without inventing fake choices, while special backgrounds are reserved for playable crises and verdicts.

Policy copy is capped at 125 characters but retains enough social, institutional, and causal context for a new player to understand each dilemma. Longer narration lives on dedicated text cards instead of expanding the inherited question panel over the character portrait. Objectives require multi-policy accomplishments; merely starting or seeing a card does not award one.

The recovered scenes retain their original `600×600` height-matched landscape canvas. This is deliberate: the source game uses that wide center-column composition, while full-bleed art continues behind it. Scenario cards keep the original dark question chrome rather than the tan landing palette, so the text surface reads as a compact panel instead of one oversized brown box.

## Capability model

The inherited visible meters are reframed for Lionrise:

- Power: grid strength and administrative execution.
- Oxygen: water and biosphere resilience.
- People: population welfare and social confidence.
- Hull: infrastructure and productive capacity.

The four visible meters and the final crisis response select the grade. There is no hidden verdict score: the ending conditions use the same Power, Oxygen, People, and Hull values that the player has watched throughout the assessment.

The five possible reports are:

- Pass with Distinction - The Lion City
- Conditional Pass - The Glass Citadel
- Pass - The Open Crossroads
- Pass - The Garden City
- Fail - Third World with Towers

The recovered vector rigs, animation, eyes, voice textures, backgrounds, music, and sound effects remain in use. Scenario-specific title graphics are assembled from native UI elements so the new presentation stays inside the original visual language.

## Verification

Useful Unity menu commands:

- **Helm → Compile campaign data**
- **Helm → Audit project**
- **Helm → Build macOS**

Development/editor shortcuts: F3 opens the first governing decision, F4 opens the signal concert, F5 opens the allocation pulse, F6 opens the examination, F7 opens the education decision, F8 opens the final stress test, and F9 dumps current state.
