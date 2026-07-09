# Lionrise Protocol — MVP Game Design

## Product promise

Lionrise Protocol is a two-minute civic sci-fi pressure cooker. The player advises Aster Lion through fourteen turning points spanning fifty years, balancing Cohesion, Growth, Security, and Autonomy while building less visible national foundations.

The player is the civic AI, not the founder. Every decision is a policy trade-off. A completed run should feel like a whole rise-or-collapse timeline, not a survival run stopped arbitrarily.

## Run flow

The fixed arc is:

1. The Cut
2. Legitimacy
3. Housing
4. Jobs
5. Water
6. Cohesion
7. Corruption
8. Defence
9. Withdrawal
10. Skills
11. Upgrade
12. Autonomy
13. Succession
14. Final Audit

Each slot draws one of six weighted variants. A maximum of one crisis card may replace a middle slot. Cards seen in recent runs cool down, queued follow-ups receive extra weight, and state conditions can return an earlier policy as a visible consequence.

## Two layers of state

Visible meters fail at either extreme:

- Cohesion: riots at zero, subsidy spiral at one hundred.
- Growth: bankruptcy at zero, corporate protectorate at one hundred.
- Security: sabotage at zero, permanent emergency at one hundred.
- Autonomy: resource blackmail at zero, fortress isolation at one hundred.

Hidden foundations track housing stock, water resilience, corruption, skills, institution depth, civil liberties, founder dependence, and foreign confidence.

Because one card represents three to five years, each arc also applies a baseline national-development milestone. The explicit choice effects then determine distribution, legitimacy, durability, and later consequences. This makes the supplied Tier Score formula achievable in fourteen decisions without turning a final audit choice into a magic override.

## Input and pacing

- Commit at 28% of screen width.
- Choice labels appear after 7% and meter-direction previews after 15%.
- No exact delta numbers appear during play.
- Desktop controls: mouse drag, `A`/`D`, or arrow keys.
- Mobile controls: touch drag or optional 0.65-second hold buttons.
- No timer is forced.
- Modeled full-run duration: 117.6 seconds including start and recap.

## Endings

The implementation includes eight immediate collapse categories and nine final classifications. The Civic Republic requires a score of 85+, deep institutions, low corruption, low founder dependence, protected liberties, and institution-oriented evidence in the final audit. Audit evidence only boosts a city that already meets those guardrails.

## Prototype content

- 90 cards
- 6 variants per arc slot
- 6 crisis interrupts
- 16 fictional speakers
- 17 ending IDs across immediate and final resolution
- Original procedural UI, graphics, music loop, and sound cues

