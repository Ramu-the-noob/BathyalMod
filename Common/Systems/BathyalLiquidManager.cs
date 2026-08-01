using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Bathyal.Common.Systems
{
    public class BathyalBackgroundWaterTile : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && !effectOnly)
            {
                // Define your trapezoid boundaries here
                int centerX = Main.maxTilesX / 2;
                int groundLevel = (int)Main.worldSurface;
                int barrierTop = Main.UnderworldLayer - 20;

                // Only apply this logic if we are below ground and above the Underworld
                if (j >= groundLevel && j < barrierTop)
                {
                    // Calculate the trapezoid width at this specific depth (Y level)
                    double progressY = (double)j / Main.maxTilesY;
                    int halfWidth = (int)(1250 - (750 * progressY));

                    int leftBound = centerX - halfWidth;
                    int rightBound = centerX + halfWidth;

                    // Check if the mined block is inside the deep sea zone
                    if (i >= leftBound && i <= rightBound)
                    {
                        Tile tile = Main.tile[i, j];

                        // Instantly replace the mined block with water
                        tile.LiquidAmount = 255;
                        tile.LiquidType = LiquidID.Water;

                        // Sync the new water to multiplayer clients
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendTileSquare(-1, i, j, 1);
                        }
                    }
                }
            }
        }
    }
}