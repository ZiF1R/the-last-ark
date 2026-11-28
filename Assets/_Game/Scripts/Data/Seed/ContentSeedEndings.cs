using System.Collections.Generic;

public static partial class ContentSeed
{
    public static readonly List<EndingDef> Endings = new()
    {
        // ------------------------------------------------- death endings
        // Each cause of death has two variants with the same priority; the
        // resolver picks randomly among tied endings, so the same stats do not
        // always read the same way twice.

        new EndingDef
        {
            id = "end_no_air",
            title = "NO AIR",
            body = "The gauge hits zero. The recycler stops. The crew has been "
                 + "holding its breath for two days. The log records the last "
                 + "reading. The pen is still in the holder.",
            priority = 100,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 0, maxScience = 10, maxMorale = 10,
        },
        new EndingDef
        {
            id = "end_no_air_b",
            title = "THE LONG BREATH",
            body = "The last canister is logged, sealed and empty. The crew "
                 + "voted to stop opening the reserves. There is nothing to "
                 + "open. Someone winds the clock anyway. It is still ticking. "
                 + "It will outlast the air by a week.",
            priority = 100,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 0, maxScience = 10, maxMorale = 10,
        },
        new EndingDef
        {
            id = "end_no_signal",
            title = "NO SIGNAL",
            body = "The science station goes dark. The samples are still in the "
                 + "fridge. The fridge is still cold. Nobody knows how to run the "
                 + "fridge. The log has a line for this. The line is blank.",
            priority = 100,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 0, maxMorale = 10,
        },
        new EndingDef
        {
            id = "end_no_signal_b",
            title = "STATIC",
            body = "The telemetry breaks into noise on turn nine. Nobody can "
                 + "say what broke it. The crew keeps the screens on. The "
                 + "screens keep the lights on. The noise keeps the crew. "
                 + "Somebody swears there is a pattern in it. Somebody else "
                 + "starts to hear one.",
            priority = 100,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 0, maxMorale = 10,
        },
        new EndingDef
        {
            id = "end_no_voice",
            title = "NO VOICE",
            body = "The crew stops talking. The log stops being written. The ship "
                 + "runs. The lights are on. The seats are filled. Nobody is in "
                 + "them. The ship is doing fine. The ship is doing fine.",
            priority = 100,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 0,
        },
        new EndingDef
        {
            id = "end_no_voice_b",
            title = "THE QUIET MESS",
            body = "Dinner is served on time. Six trays, six seats, no "
                 + "conversation. The chairs scrape, the trays clear, the "
                 + "lights dim. In the morning nobody remembers the meal. The "
                 + "roster does. The roster remembers everything now.",
            priority = 100,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 0,
        },

        // ------------------------------------------------- earned endings

        new EndingDef
        {
            id = "end_ark_sails",
            title = "THE ARK SAILS",
            body = "Everything holds. The air is sweet, the numbers are green, "
                 + "and the crew argues about movies instead of rations. The "
                 + "chart shows a coast that should not exist. The engines say "
                 + "you will reach it. For the first time, nobody on this ship "
                 + "is keeping a count of anything. There is nothing left to "
                 + "count against.",
            priority = 90,
            minSurvival = 9, minScience = 9, minMorale = 9,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
        },
        new EndingDef
        {
            id = "end_order",
            title = "ORDER",
            body = "The orders stand. The rounds are counted. The crew obeys "
                 + "without being asked. The log is full. The log is clean. The "
                 + "ship runs on time. Nobody remembers who wrote the first order. "
                 + "It does not matter. The order is the ship now.",
            priority = 82,
            minSurvival = 5, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new[] { "tamsin_oath", "soldier_disarmed" },
        },
        new EndingDef
        {
            id = "end_reactor_priest",
            title = "THE REACTOR PRIEST",
            body = "The core is open. The shielding is gone. The engineer who opened "
                 + "it is still in the room. The room is warm. The ship is warm. "
                 + "The crew does not go near the room. The log says the reactor is "
                 + "fine. The log is written by the reactor now.",
            priority = 78,
            minSurvival = 2, minScience = 6, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new[] { "reactor_open" },
        },
        new EndingDef
        {
            id = "end_mapped",
            title = "THE MAPPED COAST",
            body = "The map was right. Every badly drawn turn of it. The crew "
                 + "sails the route like a psalm, and the coast arrives exactly "
                 + "when the ink said it would. Whatever drew that map knew "
                 + "these waters. Nobody on board asks the only question that "
                 + "matters, which is: who is drawing the next one.",
            priority = 76,
            minSurvival = 4, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new[] { "map_followed" },
        },
        new EndingDef
        {
            id = "end_greenhouse",
            title = "THE GREENHOUSE",
            body = "The dead soil grows. It took three failures and one near "
                 + "mutiny, but the racks are green and the smell is wrong in "
                 + "the best way. The crew eats something it grew. Nobody had "
                 + "eaten something that was alive an hour before. The ship is "
                 + "a farm now. Farms do not sink.",
            priority = 70,
            minSurvival = 6, minScience = 4, minMorale = 4,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredTags = new[] { CardTag.Farmer },
            requiredFlags = new[] { "seed_saved" },
        },
        new EndingDef
        {
            id = "end_ward",
            title = "THE WARD",
            body = "The sick do not die. That is the whole miracle, and it "
                 + "takes all of it: the medic's hours, the cold bunks, the "
                 + "rations nobody steals. The fevers break in order, like a "
                 + "list being worked. The last one up is the one who kept the "
                 + "list. She takes it down to the mess and pins it to the "
                 + "wall, finished.",
            priority = 68,
            minSurvival = 2, minScience = 4, minMorale = 2,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredTags = new[] { CardTag.Medic, CardTag.Sick },
        },
        new EndingDef
        {
            id = "end_choir",
            title = "THE CHOIR",
            body = "It starts as humming in the ducts. Then the children "
                 + "carry it to the mess, and the soldiers stop pretending not "
                 + "to listen. By the fourth watch the whole crew is singing, "
                 + "badly, in three languages that agree on the chorus. The "
                 + "ship sails on the sound. It should not work. It works.",
            priority = 68,
            minSurvival = 2, minScience = 0, minMorale = 7,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredTags = new[] { CardTag.Dreamer },
        },
        new EndingDef
        {
            id = "end_new_crew",
            title = "THE NEW CREW",
            body = "The scan came back and nobody hid it again. The changed "
                 + "ones eat at the same table, work the same watches, and "
                 + "their names stay in the log without asterisks. Whatever "
                 + "the deep water is making, it signed the articles this "
                 + "time. The ship carries the first crew of two species, and "
                 + "the species gets along.",
            priority = 70,
            minSurvival = 3, minScience = 3, minMorale = 5,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredTags = new[] { CardTag.Mutant },
            requiredFlags = new[] { "mutant_kept" },
        },
        new EndingDef
        {
            id = "end_engine_holds",
            title = "THE ENGINE HOLDS",
            body = "The engine was not built for this voyage, and it holds "
                 + "anyway. Every night it runs a little hotter, and every "
                 + "night the engineers swear a little quieter. It holds. The "
                 + "crew learns to sleep inside the noise. When it finally "
                 + "shakes, they hold the rail, and it holds.",
            priority = 66,
            minSurvival = 7, minScience = 3, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredTags = new[] { CardTag.Engineer },
        },
        new EndingDef
        {
            id = "end_someone_small",
            title = "SOMEONE SMALL",
            body = "The future of the species is nine years old and currently "
                 + "grounded for stealing engine grease. She draws on the "
                 + "checklists, names the pumps, asks the questions that have "
                 + "no answers. The crew keeps her alive the way you keep a "
                 + "match alive. Something to carry across. Something to "
                 + "strike on the other side.",
            priority = 66,
            minSurvival = 4, minScience = 4, minMorale = 6,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredTags = new[] { CardTag.Child },
        },
        new EndingDef
        {
            id = "end_lab",
            title = "THE LAB",
            body = "The lab hums through the last stretch. Everything the "
                 + "crew survived is written up, dated and filed: the fever, "
                 + "the mutation, the air. The conclusions will reach land "
                 + "before the stories do, and they will save more people "
                 + "than the ship ever carried. The crew is small. The data "
                 + "is not.",
            priority = 65,
            minSurvival = 2, minScience = 8, minMorale = 2,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
        },
        new EndingDef
        {
            id = "end_clean_air",
            title = "CLEAN AIR",
            body = "The gauges settle at six and stay there. No alarms, no "
                 + "cannibalised parts, no speeches in the mess. The crew "
                 + "fixed what was broken and did not break anything that "
                 + "worked. It is the most boring voyage in the history of "
                 + "the service. The log says so. Nobody has ever been "
                 + "prouder of a boring log.",
            priority = 62,
            minSurvival = 6, minScience = 6, minMorale = 6,
            maxSurvival = 8, maxScience = 8, maxMorale = 8,
        },

        // ------------------------------------------------- middling / fallback

        new EndingDef
        {
            id = "end_hold",
            title = "THE HOLD",
            body = "Nobody dies. That is the report, and the report is the "
                 + "achievement. The air is thin, the science is thin, the "
                 + "jokes are thin. The crew walks through the ship like "
                 + "tired men carrying furniture. The ark holds together, and "
                 + "the crew holds on. Some voyages are just this: not "
                 + "losing.",
            priority = 30,
            minSurvival = 3, minScience = 3, minMorale = 3,
            maxSurvival = 7, maxScience = 9, maxMorale = 7,
        },
        new EndingDef
        {
            id = "end_drift",
            title = "DRIFT",
            body = "The ark does not sink and does not arrive. The current "
                 + "takes it somewhere the chart does not cover. The crew "
                 + "learns the sound of the hull and the smell of the "
                 + "recycled air, and then forgets there was ever another "
                 + "kind. The log keeps the count of days. The count is "
                 + "wrong. It does not matter.",
            priority = 0,
        },
    };
}
