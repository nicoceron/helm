# MVP Balance Report

Deterministic 50,000-run simulation, seed `928177`.

| Metric | Result | Design target |
| --- | ---: | ---: |
| Modeled median full run | 117.6 sec | 105–125 sec |
| Mean cards, balanced bot | 14.00 | 13–14 |
| Random win rate | 5.33% | 5–12% |
| Balanced win rate | 35.39% | 25–40% |
| Early death before card 5 | 0.14% overall | <15% |
| Civic Republic | reachable | rare |
| Unique endings observed | 14 | at least 4 for first build |

The current pre-audit death rate is 14.25%, below the long-term 35–55% production target. This is intentional for the first-playable content set: the MVP currently favors complete-arc learning and replay comprehension. Increase extreme-state pressure only after human timing and comprehension tests confirm players understand both ends of each meter.

Seven automated tests cover schema rules, card-pool counts, deterministic simulation, conditioned follow-ups, fourteen-card structure, balanced-bot win band, and true-ending reachability.

