# Lionrise reference implementation

Lionrise uses the interaction grammar of a Reigns-like decision game while keeping its own setting, writing, art, data, and code.

## Reference measurements

- 1024 × 768 fixed landscape window.
- 600 × 600 height-matched virtual canvas, giving an 800 × 600 working area at the target window size.
- Central 280 × 280 character card.
- Horizontal preview travel of 80 virtual pixels after quintic ease-out.
- A release commits after an intentional drag: roughly 0.02 normalized input to preview, 0.035 to select, and at least 0.05 seconds held.
- Choice labels fade in while dragging and the affected civic meters preview the consequence direction.
- Cards enter over 0.5 seconds from 180° Y rotation, hit 1.2× scale and ±50 px at the midpoint, and leave in the selected direction over 0.25 seconds.

## Lionrise adaptation

- Original square, full-bleed geometric portraits arranged as modular advisor cards.
- Original six-layer, four-stage Aster Lion world backdrop that recomposes and evolves through the run.
- Four chapter interstitials retelling the 1965–2015 Singapore development arc as a 2165 space-city history.
- Four balancing meters: cohesion, growth, security, and autonomy.
- Click either card half, drag, use arrow keys, or enable hold-to-choose.
- Route, interception, and signal-performance cards use bespoke animated interaction fields; crisis cards become a timed three-decision rapid-swipe sequence.
- All game text and gameplay state are driven by Lionrise's own JSON card database and C# systems.

## Clean-room boundary

The purchased game export is a private local visual/behavior reference only. The project does not bundle its extracted images, audio, source stubs, or serialized assets. Any production asset added to this repository must be original or separately licensed for Lionrise.
