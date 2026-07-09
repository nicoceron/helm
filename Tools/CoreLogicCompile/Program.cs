using System;
using Lionrise;

internal static class Program
{
    private static int Main()
    {
        var state = new RunState { runPlan = RunPlanGenerator.Generate() };
        if (state.runPlan.Length != 14) return 1;
        EffectResolver.Apply(state, new EffectDef
        {
            meters = new MeterDelta { growth = 5, autonomy = -2 },
            hidden = new HiddenDelta { institutionDepth = 4, corruption = -3 }
        });
        NationalDevelopment.ApplyArcMilestone(state, "jobs");
        if (state.meters.growth != 41 || state.hidden.skillBase != 40) return 2;
        var result = EndingResolver.Final(state);
        if (result == null || string.IsNullOrWhiteSpace(result.id)) return 3;
        Console.WriteLine($"Core logic compiled and executed: {result.id}, {result.tierScore:0.0}");
        return 0;
    }
}
