using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Bathyal.Common.Systems.WorldGeneration
{
    public class MasterGenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // 1. Locate the vanilla Reset pass
            int resetIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Reset"));
            GenPass resetPass = resetIndex != -1 ? tasks[resetIndex] : null;

            // 2. Wipe standard Terraria progression
            tasks.Clear();

            // 3. Re-inject the Reset pass to initialize the empty world arrays
            if (resetPass != null)
            {
                tasks.Add(resetPass);
            }

            // --- THE BATHYAL PIPELINE ---
            // Add new modular structures here. Order of execution is strictly top-to-bottom.
            tasks.Add(new TrenchGenPass("Bathyal Trench Generation", 100f));
            tasks.Add(new EvilIslandGenPass("Bathyal Evil Island", 50f));
            tasks.Add(new MountainGenPass("making Mountain Generation", 70f));
            // Future structures will look like this:
            // tasks.Add(new DungeonHubGenPass("Bathyal Dungeon Hub", 50f));
            // tasks.Add(new LiquidFillPass("Bathyal Trench Flooding", 80f));
        }
    }
}