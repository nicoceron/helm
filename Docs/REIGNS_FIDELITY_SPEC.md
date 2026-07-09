# Lionrise presentation fidelity specification

This document records the visual and behavioral measurements used to make Lionrise feel like a new game in the Reigns family while replacing the setting, characters, writing, backgrounds, symbols, and card art with Lionrise material.

## Source hierarchy

1. The private local Reigns: Beyond Unity export is the source of truth for dimensions, hierarchy, timing, easing, motion, and presentation grammar.
2. Captured gameplay is used to verify the composed frame and the way those systems read in motion.
3. Lionrise art, text, world-building, audio, data, and implementation remain original to this project.

The reference export is not a runtime dependency. No extracted texture, mesh, audio file, serialized game asset, or decompiled source stub is shipped in Lionrise.

## Reference composition

- Target window: 1024 × 768 landscape.
- Virtual canvas: 600 × 600, matched to height, producing an 800 × 600 working area at the target window.
- Persistent dark vertical play spine: approximately 40% of the working width, centered over a visible animated world.
- Character card: 280 × 280 at the horizontal center, with reference central position `(0, 100)`.
- Clickable region: approximately 280 × 320.
- Question panel: 320 × 105, overlapping the top of the card stack.
- Speaker panel: 320 × 70 outside and below the card.
- Speaker reference block: approximately 300 × 95.
- Four compact status meters: x positions `-120, -40, 40, 120`, each with a 50 × 50 mechanical back and roughly 40-unit gauge.
- Choice fields: approximately 1000 × 130 in source space, centered around x `±33`, y `-115`, displayed on the card while dragging.
- A separate rear card remains visible during motion; it is not a drop shadow baked into the front art.

## Card anatomy

Reference character presentation is treated as a layered system rather than one static portrait:

`cloth → body → choice field → blink → eyes → rear card → hair → weapon → accessories`

Lionrise uses original full-bleed geometric advisor illustrations with paired open-eye and blink frames. The runtime adds:

- subtle breathing rather than a frozen image;
- eye/face response to choice direction;
- ordinary blink intervals of about 12.4–12.9 seconds;
- stressed/hostile blink intervals of about 2.0–2.9 seconds;
- blink holds of 0.3–0.5 seconds;
- a visible independent card reverse with geometric suit marks.

Future portraits should be authored as compatible layers when an advisor needs hair, carried objects, or accessories to move independently.

## Card motion

### Entry / turn

- Total duration: 0.5 seconds.
- Start: 180° around Y.
- Midpoint at 0.25 seconds: scale 1.2 and x offset ±50.
- Finish: scale 1, x 0, rotation 0.
- Rear/choice surfaces switch visibility around 0.1667 seconds where the reference hides them during the turn.

### Drag

- Normalize pointer travel against the card interaction region.
- Map horizontal travel through `QuintEaseOut(clamp(abs(input) × 2.4))`.
- Cap visual horizontal displacement at 80 virtual pixels.
- Smooth toward the target at 6× normally and 8× when a side is active.
- Begin preview near 0.02 normalized horizontal input.
- Treat a release around 0.035 normalized input, held longer than 0.05 seconds, as an intentional choice.
- Allow only restrained vertical drift and card roll during preview.

### Exit

- Total primary exit window: approximately 0.25 seconds.
- Horizontal velocity accelerates with time, approximately `2000 × t`.
- Downward travel grows from horizontal distance squared.
- Rotation accelerates in the direction of the throw.
- Fade only near the end, then prepare the next card behind the motion.

## Dynamic background system

Backgrounds are not static illustrations. Every run state selects a seeded palette and a set of large geometric world layers, and a new state can recompose the scene.

Reference layer end positions for the sampled city background:

| Layer | End position | Entry offset |
| --- | ---: | ---: |
| sky detail | `(-6, 303)` | `(0, 82)` |
| distant world | `(-37, 174)` | `(0, -311)` |
| far city | `(-7, 116)` | `(0, -303)` |
| middle city | `(67, 65)` | `(0, -391)` |
| infrastructure | `(69, 85)` | `(0, -573)` |
| foreground | `(28, 175)` | `(0, -601)` |

- Layer canvas: approximately 1400 × 600.
- New layers ease from the entry offset to their endpoint over exactly 3 seconds.
- Disappearance takes about 1 second.
- Incoming layers move from grayscale toward the selected state palette.
- Lionrise adds restrained pointer/depth parallax without changing the reference composition.
- Four original world stages currently cover survival port, construction city, global hub, and green world-city.

## Menus and interstitials

- Menus preserve the central vertical spine and let the animated world remain visible at the sides.
- Chapter changes are 280 × 280 interstitial cards with a rear card, not conventional full-screen dialog boxes.
- The journey menu contains status, objectives, active effects, and options blocks.
- The menu panel enters from y `+100` and lerps to its resting position at roughly `delta × 8`.
- The settings screen uses the same narrow central panel grammar.

## Special-card mechanics

The reference contains multiple card types, so Lionrise must not collapse the whole experience into static binary portraits.

- **Route card:** rotating orbit/map field, connected nodes, and swipe-driven route rotation.
- **Interception card:** expanding star tunnel, moving target/reticle, pointer-follow response, and a timed three-threat swipe sequence.
- **Signal performance card:** animated waveform, crowd field, hearts, and direction-sensitive wave bending.
- **Journey/status card:** dedicated objectives, effects, status, and options presentation.

These modes share the 280-square card footprint but have their own animated input response and mode instructions.

## Acceptance frame

A gameplay capture passes the presentation check when, at a glance, it reads as:

1. a living geometric world filling the landscape behind the interface;
2. a narrow, dark, centered play column;
3. four compact status symbols at the top;
4. a question panel visibly overlapping a square card stack;
5. full-bleed flat geometric character or special-card art;
6. choice copy revealed by a tactile tilting swipe;
7. a separate speaker panel below the card;
8. background, portrait, and UI motion continuing even while the player considers the choice.

All eight conditions are required; a static card centered on a generic dashboard does not pass.
