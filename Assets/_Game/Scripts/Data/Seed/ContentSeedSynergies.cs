using System.Collections.Generic;

public static partial class ContentSeed
{
    public static readonly List<SynergyDef> Synergies = new()
    {
        new SynergyDef
        {
            id = "syn_eng_sol",
            tagA = CardTag.Engineer,
            tagB = CardTag.Soldier,
            countB = 1,
            survival = 1, science = 0, morale = -1,
            setFlag = "",
            text = "The engineer builds it. The soldier aims it.",
        },
        new SynergyDef
        {
            id = "syn_med_sick",
            tagA = CardTag.Medic,
            tagB = CardTag.Sick,
            countB = 1,
            survival = 1, science = 0, morale = 1,
            setFlag = "",
            text = "She has one patient and no supplies. She improvises.",
        },
        new SynergyDef
        {
            id = "syn_eng_eng",
            tagA = CardTag.Engineer,
            tagB = CardTag.Engineer,
            countB = 2,
            survival = 1, science = 1, morale = -1,
            setFlag = "",
            text = "Two engineers means one argument per bolt.",
        },
        new SynergyDef
        {
            id = "syn_child_dream",
            tagA = CardTag.Child,
            tagB = CardTag.Dreamer,
            countB = 1,
            survival = 0, science = 0, morale = 2,
            setFlag = "",
            text = "The child asks what the stars are called. Someone answers.",
        },
        new SynergyDef
        {
            id = "syn_sick_sick",
            tagA = CardTag.Sick,
            tagB = CardTag.Sick,
            countB = 2,
            survival = -1, science = 0, morale = -1,
            setFlag = "",
            text = "They share a bunk. They share everything now.",
        },
        new SynergyDef
        {
            id = "syn_sol_mut",
            tagA = CardTag.Soldier,
            tagB = CardTag.Mutant,
            countB = 1,
            survival = 0, science = 0, morale = -2,
            setFlag = "",
            text = "The soldier sleeps with the locker key on a string.",
        },
        new SynergyDef
        {
            id = "syn_farm_stub",
            tagA = CardTag.Farmer,
            tagB = CardTag.Stubborn,
            countB = 1,
            survival = 1, science = 0, morale = 0,
            setFlag = "",
            text = "The soil is bad. She plants anyway. The soil loses.",
        },
        new SynergyDef
        {
            id = "syn_sci_mut",
            tagA = CardTag.Scientist,
            tagB = CardTag.Mutant,
            countB = 1,
            survival = 0, science = 1, morale = -1,
            setFlag = "",
            text = "The sample is labelled. The sample is also crew.",
        },
        new SynergyDef
        {
            id = "syn_med_med",
            tagA = CardTag.Medic,
            tagB = CardTag.Medic,
            countB = 2,
            survival = 1, science = 0, morale = 0,
            setFlag = "",
            text = "One keeps the log. One keeps the hands steady.",
        },
        new SynergyDef
        {
            id = "syn_child_sick",
            tagA = CardTag.Child,
            tagB = CardTag.Sick,
            countB = 1,
            survival = 0, science = 0, morale = -1,
            setFlag = "",
            text = "The child is not allowed near that bunk. She goes anyway.",
        },
        new SynergyDef
        {
            id = "syn_stub_stub",
            tagA = CardTag.Stubborn,
            tagB = CardTag.Stubborn,
            countB = 2,
            survival = -1, science = 0, morale = 1,
            setFlag = "",
            text = "Nobody backs down. Somehow the work still gets done.",
        },
        new SynergyDef
        {
            id = "syn_dream_sick",
            tagA = CardTag.Dreamer,
            tagB = CardTag.Sick,
            countB = 1,
            survival = 0, science = -1, morale = 1,
            setFlag = "",
            text = "He describes the surface. Some of it is invented.",
        },
        new SynergyDef
        {
            id = "syn_eng_mut",
            tagA = CardTag.Engineer,
            tagB = CardTag.Mutant,
            countB = 1,
            survival = 0, science = 1, morale = -1,
            setFlag = "",
            text = "The hands fit where the tools do not. Nobody records it.",
        },
        new SynergyDef
        {
            id = "syn_farm_farm",
            tagA = CardTag.Farmer,
            tagB = CardTag.Farmer,
            countB = 2,
            survival = 1, science = 0, morale = 1,
            setFlag = "",
            text = "Two sets of rows. One harvest.",
        },
    };
}