namespace Lionrise
{
    public static class RunPlanGenerator
    {
        public static readonly string[] ArcSlots =
        {
            "cut", "legitimacy", "housing", "jobs", "water", "cohesion", "corruption",
            "defence", "withdrawal", "skills", "upgrade", "autonomy", "succession", "final_audit"
        };

        public static string[] Generate()
        {
            var copy = new string[ArcSlots.Length];
            ArcSlots.CopyTo(copy, 0);
            return copy;
        }
    }
}

