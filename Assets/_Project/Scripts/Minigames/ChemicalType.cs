namespace Minigames
{
    /// <summary>
    /// Identifies the three cleaning chemicals in the chemical cleaning minigame. The integer values are
    /// deliberately 1, 2 and 3 so a value maps directly onto the bill stage it satisfies.
    /// </summary>
    public enum ChemicalType
    {
        /// <summary>The first chemical. Cleans a dirty bill to stage 1.</summary>
        Chem1 = 1,

        /// <summary>The second chemical. Required after <see cref="Chem1"/>.</summary>
        Chem2 = 2,

        /// <summary>The third and final chemical. Cleans the bill when applied last.</summary>
        Chem3 = 3
    }
}
