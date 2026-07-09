import sys
import unittest
from pathlib import Path

TOOLS = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(TOOLS))

import content_tool


class ContentTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.cards = content_tool.load_cards()

    def test_mvp_has_six_cards_per_slot_and_crisis_pool(self):
        counts = {slot: 0 for slot in content_tool.ARC_SLOTS + ["crisis"]}
        for card in self.cards:
            counts[card["arcSlot"]] += 1
        self.assertEqual(90, len(self.cards))
        self.assertTrue(all(count == 6 for count in counts.values()), counts)

    def test_content_validator_has_no_errors(self):
        errors, _ = content_tool.validate(self.cards)
        self.assertEqual([], errors)

    def test_simulation_is_deterministic(self):
        first = content_tool.simulate_run(self.cards, "balanced", 928177)
        second = content_tool.simulate_run(self.cards, "balanced", 928177)
        self.assertEqual(first["ending"], second["ending"])
        self.assertEqual(first["shown"], second["shown"])
        self.assertEqual(first["state"]["meters"], second["state"]["meters"])

    def test_signed_ice_treaty_unlocks_later_consequence(self):
        state = content_tool.initial_state(1)
        water = next(card for card in self.cards if card["id"] == "water_001")
        content_tool.apply_choice(state, water["right"])
        state["slotIndex"] = 11
        followup = next(card for card in self.cards if card["id"] == "autonomy_001")
        self.assertTrue(content_tool.passes(followup, state))
        self.assertIn("autonomy_001", state["follow"])

    def test_full_run_is_fourteen_cards_without_collapse(self):
        result = content_tool.simulate_run(self.cards, "balanced", 12345)
        self.assertGreaterEqual(result["cards"], 10)
        self.assertLessEqual(result["cards"], 14)

    def test_balanced_bot_win_rate_is_in_target_band(self):
        results = [content_tool.simulate_run(self.cards, "balanced", 928177 + index * 7919) for index in range(1000)]
        win_rate = sum(result["victory"] for result in results) / len(results)
        self.assertGreaterEqual(win_rate, .20)
        self.assertLessEqual(win_rate, .45)

    def test_true_ending_is_rare_but_reachable(self):
        results = [content_tool.simulate_run(self.cards, "balanced", 4181 + index * 7919) for index in range(2500)]
        true_rate = sum(result["ending"] == "civic_republic" for result in results) / len(results)
        self.assertGreaterEqual(true_rate, .005)
        self.assertLessEqual(true_rate, .05)


if __name__ == "__main__":
    unittest.main()
