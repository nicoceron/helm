#!/usr/bin/env python3
"""Validate and simulate Lionrise card content without requiring Unity."""

from __future__ import annotations

import argparse
import json
import math
import random
import statistics
from collections import Counter, defaultdict
from copy import deepcopy
from pathlib import Path
from typing import Any, Iterable

ROOT = Path(__file__).resolve().parents[1]
CARD_PATH = ROOT / "Assets" / "StreamingAssets" / "Cards" / "cards.json"
ARC_SLOTS = [
    "cut", "legitimacy", "housing", "jobs", "water", "cohesion", "corruption",
    "defence", "withdrawal", "skills", "upgrade", "autonomy", "succession", "final_audit",
]
METERS = ("cohesion", "growth", "security", "autonomy")
HIDDEN = (
    "housingStock", "waterResilience", "corruption", "skillBase", "institutionDepth",
    "civilLiberties", "founderDependence", "foreignConfidence",
)
BOTS = (
    "random", "balanced", "pro_cohesion", "pro_growth", "pro_security", "pro_autonomy",
    "clean_state", "founder_loyalist", "institution_builder",
)
ARC_PROGRESS = {
    "cut": {"institutionDepth": 7, "foreignConfidence": 7},
    "legitimacy": {"institutionDepth": 7, "civilLiberties": 7},
    "housing": {"housingStock": 42},
    "jobs": {"skillBase": 15, "foreignConfidence": 13},
    "water": {"waterResilience": 30},
    "cohesion": {"institutionDepth": 9, "civilLiberties": 16},
    "corruption": {"institutionDepth": 9, "corruption": -14},
    "defence": {"institutionDepth": 9, "foreignConfidence": 7},
    "withdrawal": {"housingStock": 9, "skillBase": 13, "foreignConfidence": 11},
    "skills": {"skillBase": 19, "civilLiberties": 9},
    "upgrade": {"skillBase": 15, "foreignConfidence": 15, "corruption": -2},
    "autonomy": {"waterResilience": 30, "foreignConfidence": 5},
    "succession": {"institutionDepth": 20, "founderDependence": -21},
}
CAPACITY_METER_ARCS = {"jobs", "defence", "withdrawal", "skills", "upgrade", "autonomy"}


def load_cards(path: Path = CARD_PATH) -> list[dict[str, Any]]:
    with path.open(encoding="utf-8") as handle:
        return json.load(handle)["cards"]


def effects(choice: dict[str, Any]) -> Iterable[int]:
    data = choice.get("effects", {})
    yield from data.get("meters", {}).values()
    yield from data.get("hidden", {}).values()


def validate(cards: list[dict[str, Any]]) -> tuple[list[str], list[str]]:
    errors: list[str] = []
    warnings: list[str] = []
    ids = [card.get("id") for card in cards]
    id_set = set(ids)
    if len(id_set) != len(ids):
        for card_id, count in Counter(ids).items():
            if count > 1:
                errors.append(f"{card_id}: duplicate id")

    defined_flags = {"forced_independence"}
    for card in cards:
        for side in ("left", "right"):
            defined_flags.update(card.get(side, {}).get("effects", {}).get("flagsOn", []))

    valid_slots = set(ARC_SLOTS) | {"crisis"}
    for card in cards:
        card_id = card.get("id", "<missing id>")
        if card.get("arcSlot") not in valid_slots:
            errors.append(f"{card_id}: invalid arcSlot {card.get('arcSlot')!r}")
        if not card.get("speakerId") or not card.get("speakerName"):
            errors.append(f"{card_id}: speaker is undefined")
        prompt = card.get("prompt", "")
        if not prompt:
            errors.append(f"{card_id}: missing prompt")
        if len(prompt) > 120:
            errors.append(f"{card_id}: prompt is {len(prompt)} characters (max 120)")
        if "reigns" in prompt.lower():
            errors.append(f"{card_id}: protected reference appears in player-facing text")

        if not isinstance(card.get("left"), dict) or not isinstance(card.get("right"), dict):
            errors.append(f"{card_id}: must contain exactly two choices")
            continue
        for side in ("left", "right"):
            choice = card[side]
            label = choice.get("label", "")
            prefix = f"{card_id}/{side}"
            if not label:
                errors.append(f"{prefix}: missing label")
            elif len(label) > 20:
                errors.append(f"{prefix}: label is {len(label)} characters (max 20)")
            meter = choice.get("effects", {}).get("meters", {})
            hidden = choice.get("effects", {}).get("hidden", {})
            unknown_meters = set(meter) - set(METERS)
            unknown_hidden = set(hidden) - set(HIDDEN)
            if unknown_meters:
                errors.append(f"{prefix}: unknown meters {sorted(unknown_meters)}")
            if unknown_hidden:
                errors.append(f"{prefix}: unknown hidden fields {sorted(unknown_hidden)}")
            values = [value for value in effects(choice) if value]
            if len(values) < 2:
                errors.append(f"{prefix}: changes fewer than two systems")
            if any(abs(value) > 15 for value in meter.values()):
                errors.append(f"{prefix}: meter delta exceeds ±15")
            if any(abs(value) > 20 for value in hidden.values()):
                errors.append(f"{prefix}: hidden delta exceeds ±20")
            if values and all(value > 0 for value in values) and not card.get("crisis"):
                errors.append(f"{prefix}: all-positive effect has no trade-off")
            if values and all(value < 0 for value in values) and not card.get("crisis"):
                errors.append(f"{prefix}: all-negative effect is reserved for crises")
            for unlock in choice.get("effects", {}).get("unlockCards", []):
                if unlock not in id_set:
                    errors.append(f"{prefix}: unlock target {unlock!r} does not exist")

        conditions = card.get("conditions", {})
        required = set(conditions.get("requiredFlags", []))
        blocked = set(conditions.get("blockedFlags", []))
        for flag in required & blocked:
            errors.append(f"{card_id}: {flag!r} is both required and blocked")
        for flag in required - defined_flags:
            errors.append(f"{card_id}: required flag {flag!r} is never defined")
        for flag in blocked - defined_flags:
            warnings.append(f"{card_id}: blocked flag {flag!r} is never defined")

    by_slot = Counter(card.get("arcSlot") for card in cards if not card.get("crisis"))
    for slot in ARC_SLOTS:
        if by_slot[slot] < 5:
            errors.append(f"{slot}: only {by_slot[slot]} cards (minimum 5)")
    if len(cards) < 90:
        warnings.append(f"MVP target is 90 cards; found {len(cards)}")
    return errors, warnings


def initial_state(seed: int) -> dict[str, Any]:
    return {
        "seed": seed,
        "slotIndex": 0,
        "meters": {"cohesion": 45, "growth": 35, "security": 25, "autonomy": 25},
        "hidden": {
            "housingStock": 20, "waterResilience": 10, "corruption": 55, "skillBase": 25,
            "institutionDepth": 20, "civilLiberties": 55, "founderDependence": 70,
            "foreignConfidence": 25,
        },
        "flags": {"forced_independence"},
        "seen": set(),
        "follow": set(),
        "crisisUsed": False,
    }


def passes(card: dict[str, Any], state: dict[str, Any]) -> bool:
    condition = card.get("conditions", {})
    if not condition:
        return True
    index = state["slotIndex"]
    if index < condition.get("minSlotIndex", 0) or index > condition.get("maxSlotIndex", 99):
        return False
    if any(flag not in state["flags"] for flag in condition.get("requiredFlags", [])):
        return False
    if any(flag in state["flags"] for flag in condition.get("blockedFlags", [])):
        return False
    for item in condition.get("meterRanges", []):
        if not item["min"] <= state["meters"][item["name"]] <= item["max"]:
            return False
    for item in condition.get("hiddenRanges", []):
        if not item["min"] <= state["hidden"][item["name"]] <= item["max"]:
            return False
    return True


def crisis_tags(state: dict[str, Any]) -> set[str]:
    result: set[str] = set()
    names = (("cohesion", "cohesion"), ("growth", "economy"), ("security", "security"), ("autonomy", "sovereignty"))
    for meter, tag in names:
        if state["meters"][meter] <= 12 or state["meters"][meter] >= 88:
            result.add(tag)
    if state["hidden"]["corruption"] >= 75:
        result.add("corruption")
    if state["hidden"]["waterResilience"] <= 20:
        result.add("water")
    return result


def weighted_pick(candidates: list[dict[str, Any]], state: dict[str, Any], tags: set[str], rng: random.Random) -> dict[str, Any]:
    weights = []
    for card in candidates:
        weight = max(1, card.get("weight", 1))
        weight += 8 * sum(tag in tags for tag in card.get("tags", []))
        if card["id"] in state["follow"]:
            weight += 25
        weights.append(weight)
    return rng.choices(candidates, weights=weights, k=1)[0]


def draw(cards: list[dict[str, Any]], slot: str, state: dict[str, Any], rng: random.Random) -> dict[str, Any]:
    tags = crisis_tags(state)
    if not state["crisisUsed"] and slot not in {"cut", "legitimacy", "succession", "final_audit"} and tags and rng.random() < .42:
        candidates = [
            card for card in cards if card.get("crisis") and not state["seen"].__contains__(card["id"])
            and tags.intersection(card.get("tags", [])) and passes(card, state)
        ]
        if candidates:
            state["crisisUsed"] = True
            return weighted_pick(candidates, state, tags, rng)
    candidates = [
        card for card in cards if card.get("arcSlot") == slot and not card.get("crisis")
        and card["id"] not in state["seen"] and passes(card, state)
    ]
    if not candidates:
        candidates = [card for card in cards if card.get("arcSlot") == slot and not card.get("crisis") and card["id"] not in state["seen"]]
    if not candidates:
        raise RuntimeError(f"No candidate for slot {slot}")
    return weighted_pick(candidates, state, tags, rng)


def projected_state(state: dict[str, Any], choice: dict[str, Any]) -> dict[str, Any]:
    result = deepcopy(state)
    apply_choice(result, choice)
    return result


def apply_choice(state: dict[str, Any], choice: dict[str, Any]) -> None:
    delta = choice.get("effects", {})
    for name, value in delta.get("meters", {}).items():
        state["meters"][name] = max(0, min(100, state["meters"][name] + value))
    for name, value in delta.get("hidden", {}).items():
        state["hidden"][name] = max(0, min(100, state["hidden"][name] + value))
    state["flags"].difference_update(delta.get("flagsOff", []))
    state["flags"].update(delta.get("flagsOn", []))
    state["follow"].update(delta.get("unlockCards", []))


def apply_arc_progress(state: dict[str, Any], slot: str) -> None:
    """Apply the capacity accumulated during the 3–5 years represented by an arc slot."""
    for name, value in ARC_PROGRESS.get(slot, {}).items():
        state["hidden"][name] = max(0, min(100, state["hidden"][name] + value))
    if slot in CAPACITY_METER_ARCS:
        state["meters"]["growth"] = min(100, state["meters"]["growth"] + 1)
        state["meters"]["security"] = min(100, state["meters"]["security"] + 1)


def balanced_cost(state: dict[str, Any]) -> float:
    meter_cost = sum(abs(value - 55) for value in state["meters"].values())
    h = state["hidden"]
    foundation = h["housingStock"] + h["waterResilience"] + h["skillBase"] + h["institutionDepth"] + h["civilLiberties"] + h["foreignConfidence"]
    penalty = h["corruption"] + h["founderDependence"]
    return meter_cost * 2 - foundation * .22 + penalty * .18


def choose(card: dict[str, Any], state: dict[str, Any], bot: str, rng: random.Random) -> dict[str, Any]:
    options = [card["left"], card["right"]]
    if bot == "random":
        return rng.choice(options)
    if bot == "balanced":
        return min(options, key=lambda option: balanced_cost(projected_state(state, option)))
    if bot.startswith("pro_"):
        meter = bot.removeprefix("pro_")
        return max(options, key=lambda option: option.get("effects", {}).get("meters", {}).get(meter, 0))
    if bot == "clean_state":
        return min(options, key=lambda option: (
            projected_state(state, option)["hidden"]["corruption"], balanced_cost(projected_state(state, option))))
    if bot == "founder_loyalist":
        return max(options, key=lambda option: projected_state(state, option)["hidden"]["founderDependence"])
    if bot == "institution_builder":
        return max(options, key=lambda option: (
            projected_state(state, option)["hidden"]["institutionDepth"],
            -projected_state(state, option)["hidden"]["founderDependence"]))
    raise ValueError(bot)


def immediate_ending(state: dict[str, Any]) -> str | None:
    for name, low, high in (
        ("cohesion", "deck_riots", "subsidy_spiral"),
        ("growth", "bankruptcy", "corporate_protectorate"),
        ("security", "sabotage_night", "emergency_without_end"),
        ("autonomy", "ice_blackmail", "fortress_aster"),
    ):
        if state["meters"][name] <= 0:
            return low
        if state["meters"][name] >= 100:
            return high
    return None


def final_ending(state: dict[str, Any]) -> tuple[str, float, bool]:
    m, h = state["meters"], state["hidden"]
    imbalance = sum(abs(value - 55) for value in m.values()) / 4
    balance = max(0, 100 - imbalance * 2.1)
    foundation = (
        h["housingStock"] * .18 + h["waterResilience"] * .18 + h["skillBase"] * .16 +
        h["institutionDepth"] * .18 + h["foreignConfidence"] * .14 + h["civilLiberties"] * .16
    )
    score = max(0, min(100, balance * .45 + foundation * .55 - h["corruption"] * .20 - h["founderDependence"] * .12))
    durable = h["institutionDepth"] >= 75 and h["corruption"] < 35 and h["founderDependence"] < 40 and h["civilLiberties"] > 50
    evidence_flags = {"audit_institutions", "audit_water_records", "audit_rights", "audit_housing_queue", "audit_public_assets", "audit_system_test"}
    if durable and evidence_flags.intersection(state["flags"]):
        score = min(100, score + 16)
    if m["autonomy"] < 22 or h["waterResilience"] < 25:
        return "protectorate", score, False
    if m["security"] > 78 and h["civilLiberties"] < 42:
        return "garrison_state", score, False
    if m["growth"] > 78 and h["corruption"] > 55 and h["foreignConfidence"] > 55:
        return "corporate_port", score, False
    if m["growth"] > 62 and h["housingStock"] < 42:
        return "crowded_miracle", score, False
    if score >= 85 and durable:
        return "civic_republic", score, True
    if score >= 72 and h["civilLiberties"] < 45:
        return "glass_city", score, True
    if score >= 72 and h["founderDependence"] > 60:
        return "founders_shadow", score, True
    if score >= 72:
        return "tier_one", score, True
    return "failed_port", score, False


def simulate_run(cards: list[dict[str, Any]], bot: str, seed: int) -> dict[str, Any]:
    rng = random.Random(seed)
    state = initial_state(seed)
    shown: list[str] = []
    ending: str | None = None
    score = 0.0
    victory = False
    for index, slot in enumerate(ARC_SLOTS):
        state["slotIndex"] = index
        card = draw(cards, slot, state, rng)
        shown.append(card["id"])
        state["seen"].add(card["id"])
        option = choose(card, state, bot, rng)
        apply_choice(state, option)
        apply_arc_progress(state, slot)
        ending = immediate_ending(state)
        if ending:
            break
    if ending is None:
        ending, score, victory = final_ending(state)
    duration = 4.0 + len(shown) * 7.4 + (10.0 if len(shown) == 14 else 6.0)
    return {"ending": ending, "score": score, "victory": victory, "cards": len(shown), "duration": duration, "state": state, "shown": shown}


def simulate(cards: list[dict[str, Any]], runs: int, seed: int) -> dict[str, Any]:
    all_results = []
    by_bot: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for index in range(runs):
        bot = BOTS[index % len(BOTS)]
        result = simulate_run(cards, bot, seed + index * 7919)
        all_results.append(result)
        by_bot[bot].append(result)

    def summary(results: list[dict[str, Any]]) -> dict[str, Any]:
        return {
            "runs": len(results),
            "median_seconds": round(statistics.median(item["duration"] for item in results), 1),
            "mean_cards": round(statistics.fmean(item["cards"] for item in results), 2),
            "early_death_pct": round(sum(item["cards"] < 5 for item in results) * 100 / len(results), 2),
            "pre_audit_death_pct": round(sum(item["cards"] < 14 for item in results) * 100 / len(results), 2),
            "victory_pct": round(sum(item["victory"] for item in results) * 100 / len(results), 2),
            "mean_tier_score": round(statistics.fmean(item["score"] for item in results), 2),
        }

    return {
        "overall": summary(all_results),
        "bots": {name: summary(results) for name, results in by_bot.items()},
        "endings": dict(Counter(item["ending"] for item in all_results).most_common()),
        "reachable_ending_count": len({item["ending"] for item in all_results}),
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    subparsers = parser.add_subparsers(dest="command", required=True)
    subparsers.add_parser("validate", help="validate card content")
    simulation = subparsers.add_parser("simulate", help="run deterministic balance bots")
    simulation.add_argument("--runs", type=int, default=10_000)
    simulation.add_argument("--seed", type=int, default=928_177)
    args = parser.parse_args()

    cards = load_cards()
    errors, warnings = validate(cards)
    for warning in warnings:
        print(f"WARN  {warning}")
    for error in errors:
        print(f"ERROR {error}")
    if errors:
        print(f"\nValidation failed: {len(errors)} error(s), {len(warnings)} warning(s).")
        return 1
    print(f"Content valid: {len(cards)} cards across {len(ARC_SLOTS)} arc slots + crisis pool.")
    if args.command == "simulate":
        if args.runs < 1:
            parser.error("--runs must be positive")
        print(json.dumps(simulate(cards, args.runs, args.seed), indent=2, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
