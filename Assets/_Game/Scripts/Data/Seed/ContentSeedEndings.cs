using System.Collections.Generic;

public static partial class ContentSeed
{
    public static readonly List<EndingDef> Endings = new()
    {
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
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
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
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
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
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_order",
            title = "ORDER",
            body = "The orders stand. The rounds are counted. The crew obeys "
                 + "without being asked. The log is full. The log is clean. The "
                 + "ship runs on time. Nobody remembers who wrote the first order. "
                 + "It does not matter. The order is the ship now.",
            priority = 72,
            minSurvival = 5, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new[] { "tamsin_oath", "soldier_disarmed" },
            forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_reactor_priest",
            title = "THE REACTOR PRIEST",
            body = "The core is open. The shielding is gone. The engineer who opened "
                 + "it is still in the room. The room is warm. The ship is warm. "
                 + "The crew does not go near the room. The log says the reactor is "
                 + "fine. The log is written by the reactor now.",
            priority = 70,
            minSurvival = 0, minScience = 6, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new[] { "reactor_open" },
            forbiddenFlags = new string[0],
            requiredTags = new[] { CardTag.Engineer },
            forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_mapped",
            title = "THE MAP WAS RIGHT",
            body = "The rivers are there. The map was old, not wrong. The dreamer "
                 + "is not surprised. The ship follows the rivers. The rivers are "
                 + "not water. The rivers are something else. The log has a new "
                 + "entry. The entry is a drawing.",
            priority = 65,
            minSurvival = 0, minScience = 0, minMorale = 5,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new[] { "map_followed" },
            forbiddenFlags = new string[0],
            requiredTags = new[] { CardTag.Dreamer },
            forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_clean_air",
            title = "CLEAN AIR",
            body = "The bunk is sealed. The sick man is gone. The air is clean. The "
                 + "crew breathes without thinking about it. The log has a name in "
                 + "it. The name is the only thing in the bunk. The air is very clean.",
            priority = 60,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new[] { "bunk_sealed" },
            forbiddenFlags = new string[0],
            requiredTags = new CardTag[0],
            forbiddenTags = new[] { CardTag.Sick },
        },
        new EndingDef
        {
            id = "end_all_high",
            title = "ALL HANDS",
            body = "The numbers are good. The crew is fed. The lab is warm. The log "
                 + "is full. The ship is not dying. It is not living either. It is "
                 + "holding. The crew calls this a victory. The log does not use that word.",
            priority = 50,
            minSurvival = 7, minScience = 7, minMorale = 7,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_lab",
            title = "THE LAB",
            body = "The science station is the warmest room on the ship. The crew "
                 + "does not go in. The samples are labeled. The labels are correct. "
                 + "The lab runs all night. The crew sleeps. The ship is a "
                 + "laboratory now. The laboratory is doing fine.",
            priority = 50,
            minSurvival = 0, minScience = 7, minMorale = 0,
            maxSurvival = 6, maxScience = 10, maxMorale = 6,
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_choir",
            title = "THE CHOIR",
            body = "The crew sings. Nobody knows when it started. The log has a "
                 + "line for it. The line is a note. The note is not in any of the "
                 + "manuals. The ship runs. The crew sings. The singing is not a "
                 + "celebration. It is a habit.",
            priority = 50,
            minSurvival = 0, minScience = 0, minMorale = 7,
            maxSurvival = 6, maxScience = 6, maxMorale = 10,
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_hold",
            title = "HOLD",
            body = "The ship holds. The numbers are good. The crew is fed. The log "
                 + "is clean. The ship does not move. It holds. The holding is the "
                 + "whole plan. The plan is working. The log says so. The log is "
                 + "written by the captain. The captain is holding too.",
            priority = 50,
            minSurvival = 7, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 6, maxMorale = 6,
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
        },
        new EndingDef
        {
            id = "end_drift",
            title = "DRIFT",
            body = "The ship drifts. The numbers are not good or bad. The crew is "
                 + "not happy or sad. The log has entries. The entries are short. "
                 + "The ship is still going. Nobody knows where. The drift is not a "
                 + "failure. It is not a success. It is a drift.",
            priority = 0,
            minSurvival = 0, minScience = 0, minMorale = 0,
            maxSurvival = 10, maxScience = 10, maxMorale = 10,
            requiredFlags = new string[0], forbiddenFlags = new string[0],
            requiredTags = new CardTag[0], forbiddenTags = new CardTag[0],
        },
    };
}