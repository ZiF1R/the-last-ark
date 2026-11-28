using System.Collections.Generic;

public static partial class ContentSeed
{
    public static readonly List<EventDef> Events = new()
    {
        new EventDef
        {
            id = "oskar_reveal",
            title = "THE HAND IN THE VENT",
            causeLine = "The new engineer has six fingers.",
            body = "He shows nobody. The vent shows everyone. The grate is bent outward "
                 + "in a shape with five points and one extra. He does not explain. "
                 + "The log has a field for this. Nobody fills it.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Mutant },
            },
            minTurn = 0, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "KEEP HIM QUIET",
                    result = "The log stays blank. He stays. The vent stays warm.",
                    survival = 0, science = 1, morale = -1,
                    setFlag = "oskar_vented",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "PUT HIM OUT",
                    result = "The airlock cycles. The shape in the grate is still there.",
                    survival = 1, science = 0, morale = -2,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval { selector = CrewSelector.ById, cardId = "oskar_engineer" },
                },
            },
        },
        new EventDef
        {
            id = "tamsin_orders",
            title = "ORDERS FROM THE BRIDGE",
            causeLine = "The soldier has been writing orders.",
            body = "Three pages. Rationing, watch rotation, a line about the child. "
                 + "She has signed them. She has not asked anyone. The pen is still "
                 + "in her hand when you find her.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.RolePresent, role = CardRole.Soldier },
            },
            minTurn = 0, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "SIGN THEM",
                    result = "The orders stand. The crew obeys. Nobody looks at her.",
                    survival = 1, science = 0, morale = 1,
                    setFlag = "tamsin_oath",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "BURN THEM",
                    result = "The paper curls. She watches it. She does not argue.",
                    survival = 0, science = 0, morale = -1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "essa_sings",
            title = "SOMETHING IN THE DUCT",
            causeLine = "The child has been singing to the vent.",
            body = "The duct is not a person. The duct answers anyway. The sound is not "
                 + "a child's sound. She says it is a friend. She says it has been "
                 + "there since the burn.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Child },
            },
            minTurn = 0, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "LET HER SING",
                    result = "The vent hums back. The crew sleeps better. Nobody asks why.",
                    survival = 0, science = 0, morale = 2,
                    setFlag = "essa_kept",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "SEAL THE DUCT",
                    result = "The grate goes quiet. She does not sing again.",
                    survival = -1, science = 0, morale = -1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "corwin_cough",
            title = "THE COUGH",
            causeLine = "The new hand coughs in the corridor.",
            body = "It is a dry cough. He says it is the dust. The dust has been in "
                 + "the air since the burn. Nobody else is coughing. He says that "
                 + "means nothing. He is probably right.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Sick },
            },
            minTurn = 0, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "LOG IT AS ALLERGY",
                    result = "The log says allergy. He keeps working. The cough keeps going.",
                    survival = 0, science = 1, morale = -1,
                    setFlag = "cough_hidden",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "SEND HIM TO BUNK",
                    result = "He goes. The work slows. The corridor is quiet.",
                    survival = -1, science = -1, morale = 1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "yusuf_hands",
            title = "SIX FINGERS",
            causeLine = "The new engineer has an extra finger.",
            body = "It is on the left hand. It is not a thumb. He uses it to reach "
                 + "a panel nobody else can reach. The panel works better now. "
                 + "The log has no field for the finger.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Mutant },
            },
            minTurn = 0, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "KEEP HIM",
                    result = "The panel stays fixed. The finger stays unlogged. The crew adjusts.",
                    survival = 0, science = 1, morale = -1,
                    setFlag = "yusuf_spared",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "LOG THE ANOMALY",
                    result = "The log records it. The crew reads the log. The panel breaks again.",
                    survival = 1, science = 0, morale = -2,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "fever_breaks",
            title = "THE FEVER BREAKS",
            causeLine = "The sick man stops answering.",
            body = "He has been quiet since the second day. Now the bunk is quiet in "
                 + "a different way. The others have already decided what to do about "
                 + "it. They are waiting for you to say it out loud.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Sick },
            },
            minTurn = 2, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "CALL THE MEDIC",
                    result = "She works for an hour. He breathes. Nobody thanks her.",
                    survival = 1, science = 0, morale = 1,
                    setFlag = "",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Medic },
                    },
                    lockHint = "NO MEDIC ON BOARD",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "SEAL THE BUNK",
                    result = "The door closes. The log records a name and nothing else.",
                    survival = 0, science = 0, morale = -2,
                    setFlag = "bunk_sealed",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval { selector = CrewSelector.ByTag, tag = CardTag.Sick },
                },
            },
        },
        new EventDef
        {
            id = "bunk_quiet",
            title = "THE BUNK IS QUIET",
            causeLine = "The sealed door has not opened.",
            body = "The others walk past it. Nobody stops. The log has a line for the "
                 + "name. The line is the only thing in the bunk now.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.FlagSet, flag = "bunk_sealed" },
                new StateCondition { kind = ConditionKind.FlagSet, flag = "cough_hidden" },
            },
            minTurn = 3, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "SIT BY THE DOOR",
                    result = "You sit. The door does not open. After an hour you leave.",
                    survival = 0, science = 0, morale = 1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "reactor_strain",
            title = "THE REACTOR STRAINS",
            causeLine = "The core has been running hot for two days.",
            body = "The gauge is in the red. The engineer says it is fine. The gauge "
                 + "disagrees. Opening the core would vent the heat. It would also "
                 + "vent the shielding.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Engineer },
            },
            minTurn = 2, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "OPEN THE CORE",
                    result = "The heat vents. The gauge drops. The shielding goes with it.",
                    survival = 2, science = 1, morale = -2,
                    setFlag = "reactor_open",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.FlagSet, flag = "oskar_vented" },
                    },
                    lockHint = "NO VENT REPORT",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "RUN IT COLD",
                    result = "The gauge holds. The output drops. The lights dim.",
                    survival = 0, science = -1, morale = 1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "seed_count",
            title = "THE SEED COUNT",
            causeLine = "The farmer has been counting the seeds.",
            body = "She has a number. The number is smaller than yesterday. The seeds "
                 + "are in a locker with a lock nobody has the key to. She has the "
                 + "key. She is not using it.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Farmer },
            },
            minTurn = 2, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "SAVE THE SEED",
                    result = "The locker stays locked. The number stays the same. Dinner is smaller.",
                    survival = 1, science = 1, morale = -1,
                    setFlag = "seed_saved",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "EAT IT",
                    result = "The lock opens. Dinner is bigger. The number drops.",
                    survival = 1, science = 0, morale = 0,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "soldier_round",
            title = "ROUNDS AT NIGHT",
            causeLine = "Someone is walking the corridor at night.",
            body = "The soldier has been checking the airlock. He has also been "
                 + "checking the crew. He does not say which he trusts less. "
                 + "The rounds are not in the log.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Soldier },
            },
            minTurn = 3, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "TAKE THE ROUNDS",
                    result = "The rounds stop. The airlock stays shut. The crew sleeps.",
                    survival = 1, science = 0, morale = -1,
                    setFlag = "soldier_disarmed",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval { selector = CrewSelector.ByRole, role = CardRole.Soldier },
                },
                new Choice
                {
                    label = "LET HIM KEEP THEM",
                    result = "The rounds continue. The corridor is quieter. Nobody sleeps.",
                    survival = 0, science = 0, morale = 1,
                    setFlag = "",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.FlagSet, flag = "mutant_kept" },
                    },
                    lockHint = "NO MUTANT ABOARD",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "child_question",
            title = "A QUESTION AT DINNER",
            causeLine = "The child has been reading the ship manual.",
            body = "She asks what happens if the ship does not stop. The manual has an "
                 + "answer. The answer is three pages long. She has read all three "
                 + "pages. She is waiting for you to say it.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Child },
            },
            minTurn = 2, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "ANSWER HONESTLY",
                    result = "You tell her. She nods. She does not ask again.",
                    survival = 0, science = 1, morale = -1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "CHANGE THE SUBJECT",
                    result = "You talk about the weather. There is no weather. She lets it go.",
                    survival = 0, science = 0, morale = 1,
                    setFlag = "",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Stubborn },
                    },
                    lockHint = "NO ONE STUBBORN",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "hungry_turn",
            title = "THE WATER TASTES OF IRON",
            causeLine = "The recycler has been running dry.",
            body = "The water is brown. The filter is older than the ship. The farmer "
                 + "says the last ration is still in the locker. The locker is not "
                 + "locked. Nobody has opened it.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.StatAtMost, stat = StatKind.Survival, value = 3 },
            },
            minTurn = 0, maxTurn = 99, priority = 10, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "OPEN THE RATION",
                    result = "The locker opens. The water clears. The last ration is gone.",
                    survival = 2, science = 0, morale = -1,
                    setFlag = "",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Farmer },
                    },
                    lockHint = "NO FARMER LEFT",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "WAIT",
                    result = "The water stays brown. Nobody drinks. The recycler runs.",
                    survival = 0, science = 0, morale = -2,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "morale_low",
            title = "NOBODY SIGNS THE LOG",
            causeLine = "The log has been empty for two days.",
            body = "The captain's page is blank. The crew's pages are blank. The pen "
                 + "is on the desk. Nobody has touched it. The silence is not "
                 + "hostile. It is worse than hostile.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.StatAtMost, stat = StatKind.Morale, value = 3 },
            },
            minTurn = 0, maxTurn = 99, priority = 10, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "GIVE A SPEECH",
                    result = "You talk for ten minutes. Someone signs the log after.",
                    survival = 0, science = 0, morale = 2,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "SAY NOTHING",
                    result = "The silence holds. The log stays empty. The lights stay on.",
                    survival = 1, science = 0, morale = -1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "crew_thin",
            title = "TOO FEW HANDS",
            causeLine = "The watch rotation has gaps in it.",
            body = "Two people on the roster. Three shifts. The math does not work. "
                 + "Nobody has said it out loud. The roster is on the wall. "
                 + "The gaps are visible.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.CrewCountAtMost, value = 2 },
            },
            minTurn = 0, maxTurn = 99, priority = 10, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "WORK DOUBLE SHIFTS",
                    result = "The shifts fill. The crew does not sleep. The work gets done.",
                    survival = 1, science = 0, morale = -2,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "LET IT SLIDE",
                    result = "The shifts stay empty. Nothing breaks. The crew sleeps.",
                    survival = -1, science = 0, morale = 1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "mutant_rumor",
            title = "WHAT THE SCAN SHOWS",
            causeLine = "The scanner has been running all night.",
            body = "The scan is clean. The scan is too clean. The mutant has been in "
                 + "the same room as the scanner for three days. Nobody has asked "
                 + "what the scanner was looking for.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Mutant },
            },
            minTurn = 3, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "RUN THE SCAN",
                    result = "The scan runs again. The result is the same. Nobody is convinced.",
                    survival = 0, science = 1, morale = -1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "KEEP IT QUIET",
                    result = "The scanner goes dark. The crew stops asking. The room stays quiet.",
                    survival = 0, science = 0, morale = 1,
                    setFlag = "mutant_kept",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "stubborn_wall",
            title = "THE SAME ARGUMENT",
            causeLine = "Two crew have been arguing since breakfast.",
            body = "The argument is about the water filter. It has been about the "
                 + "water filter for three days. Neither of them is wrong. Neither "
                 + "of them will stop. The filter is still broken.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Stubborn },
            },
            minTurn = 3, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "LET THEM ARGUE",
                    result = "The argument continues. The filter stays broken. The crew adjusts.",
                    survival = 0, science = 0, morale = -1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "PICK A SIDE",
                    result = "You pick a side. The other side stops talking to you.",
                    survival = 1, science = 0, morale = -1,
                    setFlag = "",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Stubborn },
                    },
                    lockHint = "NO ONE STUBBORN",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "dreamer_map",
            title = "A MAP ON THE WALL",
            causeLine = "Someone has been drawing on the bulkhead.",
            body = "It is a map of the surface. The surface is dead. The map has "
                 + "rivers on it. The rivers are wrong. The dreamer says they are "
                 + "not wrong. The dreamer says the map is old.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Dreamer },
                new StateCondition { kind = ConditionKind.FlagSet, flag = "essa_kept" },
            },
            minTurn = 4, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "FOLLOW THE MAP",
                    result = "You follow the map. The rivers are not there. The dreamer is not surprised.",
                    survival = 0, science = 1, morale = 1,
                    setFlag = "map_followed",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "BURN IT",
                    result = "The bulkhead is clean. The dreamer stops drawing. The wall is blank.",
                    survival = 1, science = 0, morale = -2,
                    setFlag = "",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Dreamer },
                    },
                    lockHint = "NO DREAMER LEFT",
                    removal = new CrewRemoval { selector = CrewSelector.ByTag, tag = CardTag.Dreamer },
                },
            },
        },
        new EventDef
        {
            id = "science_push",
            title = "THE LAB IS COLD",
            causeLine = "The cultures have stopped growing.",
            body = "The incubator is at the wrong temperature. It has been at the "
                 + "wrong temperature for a day. The scientist has not slept. "
                 + "The cultures are not growing. The scientist is still watching them.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.StatAtMost, stat = StatKind.Science, value = 6 },
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Scientist },
            },
            minTurn = 2, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "LOG THE FAILURE",
                    result = "The log records the temperature. The cultures stay cold. The scientist sleeps.",
                    survival = 0, science = 1, morale = 0,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "medic_round",
            title = "THE MEDIC MAKES ROUNDS",
            causeLine = "The medic has been checking every bunk.",
            body = "She checks the same bunk twice. She does not say why. The second "
                 + "check is shorter. She writes nothing in the log. The pen is dry.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.TagPresent, tag = CardTag.Medic },
            },
            minTurn = 4, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "LET HER WORK",
                    result = "She finishes the rounds. The log stays empty. The bunks are quiet.",
                    survival = 0, science = 0, morale = 1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
            },
        },
        new EventDef
        {
            id = "empty_mess",
            title = "EMPTY MESS",
            causeLine = "The mess hall has been set for four.",
            body = "There are three of you. The fourth place is set anyway. Nobody "
                 + "has moved it. The plates are clean. The chairs are pushed in. "
                 + "The mess hall is very quiet.",
            conditions = new[]
            {
                new StateCondition { kind = ConditionKind.CrewCountAtMost, value = 3 },
            },
            minTurn = 3, maxTurn = 99, priority = 0, weight = 10, once = true,
            choices = new[]
            {
                new Choice
                {
                    label = "EAT ALONE",
                    result = "You eat alone. The fourth place stays set. Nobody clears it.",
                    survival = 0, science = 0, morale = -1,
                    setFlag = "",
                    requirements = new StateCondition[0],
                    lockHint = "",
                    removal = new CrewRemoval(),
                },
                new Choice
                {
                    label = "SIT TOGETHER",
                    result = "You sit together. The fourth place stays set. The food is shared.",
                    survival = 0, science = 0, morale = 1,
                    setFlag = "",
                    requirements = new[]
                    {
                        new StateCondition { kind = ConditionKind.FlagSet, flag = "seed_saved" },
                    },
                    lockHint = "NO SEED TO SHARE",
                    removal = new CrewRemoval(),
                },
            },
        },
    };
}