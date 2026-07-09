using System;

namespace FM26Trainer
{
    public static class CheatTargets
    {
        private static readonly TargetField[] Fields =
        {
            new TargetField(
                "Unlimited fitness / condition",
                "CE: improveTeamCondition(ptrClub, 10000, ...); ptrPlayer + plao.Popc; ptrPlayer + plao.Pftg",
                "Likely keep Popc at 10000 and fatigue low, after live verification.",
                "Research only: ptrPlayer/ptrClub and symbolic offsets are not resolved yet."),

            new TargetField(
                "No injuries",
                "CE: removeTeamInjuries(ptrClub, 10000, ...); removePlayerInjuries(ptrPerson, 10000)",
                "Need decode/live observation of injury records before writing.",
                "Research only: helper body is encoded in the CE table."),

            new TargetField(
                "Players always happy",
                "CE: removeTeamUnhappiness(ptrClub); maxTrainingHappiness(ptrPerson); ptrPlayer + plao.Pmor max 20",
                "Likely keep morale at 20 and happiness fields at verified max values.",
                "Research only: happiness value ranges and pointer chains need live verification.")
        };

        public static void Print()
        {
            Console.WriteLine("FM26 requested targets");
            Console.WriteLine();

            foreach (TargetField field in Fields)
            {
                Console.WriteLine(field.Name);
                Console.WriteLine($"  Evidence: {field.Evidence}");
                Console.WriteLine($"  Plan:     {field.Plan}");
                Console.WriteLine($"  Status:   {field.Status}");
                Console.WriteLine();
            }
        }
    }

    internal sealed record TargetField(string Name, string Evidence, string Plan, string Status);
}

