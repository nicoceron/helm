# First Build Acceptance

| Requirement | Implementation status |
| --- | --- |
| Fresh start under three seconds | Verified in the macOS standalone player |
| Swipe understood without tutorial wall | Title enters directly; bottom hint and directional labels appear during drag |
| Fourteen cards in a full run | Implemented and tested |
| About two minutes | Simulator median 117.6 seconds; first standalone run completed all 14 cards |
| Every choice changes at least two systems | Validator enforced; all 180 choices pass |
| Later hidden consequence | Ice treaty can condition and heavily weight a later autonomy demand |
| At least four reachable endings | Fourteen observed in 50,000 simulations |
| Immediate replay | Implemented on ending screen |
| No borrowed franchise assets or text | All visuals/audio/code/copy are original |
| No real speech text | Enforced by writing process and source separation |

Unity `6000.3.18f1` editor compilation and a 105.7 MB macOS standalone build are verified. The first live run completed all 14 cards, persisted its profile, resolved a Failed Port ending at Tier Index 69, and logged no runtime exceptions. iOS and Android modules are not installed yet.
