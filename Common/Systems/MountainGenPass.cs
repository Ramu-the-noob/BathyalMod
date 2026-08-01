using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.IO;

namespace Bathyal.Common.Systems.WorldGeneration
{
    public class MountainGenPass : GenPass
    {
        public MountainGenPass(string name, double loadWeight) : base(name, loadWeight) {}

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Raising the Left Wastes Mountain...";

            int groundLevel = (int)Main.worldSurface;
            
            // 1. Establish the Left Side Footprint
            // A 1000-block wide mountain positioned securely on the left side of the map
            int width = 1000;
            int leftBound = (int)(Main.maxTilesX * 0.05); // Starts 5% in from the left edge
            int rightBound = leftBound + width;

            ushort snowType = TileID.SnowBlock;
            ushort iceType = TileID.IceBlock;
            
            // Standardizing the Dungeon aesthetic for this world
            ushort dungeonBrick = TileID.BlueDungeonBrick;
            ushort dungeonWall = WallID.BlueDungeonUnsafe;

            float[] mountainProfile = new float[width];
            
            // The maximum height the mountain can be without hitting the skybox ceiling (Y=0)
            int peakMaxHeight = groundLevel - 50; 

            // 2. Generate the Jagged Mountain Profile
            for (int x = 0; x < width; x++)
            {
                // Create a bell-curve shape that favors the center
                float nx = (x - (width / 2f)) / (width / 2f);
                float baseHeight = (float)Math.Cos(nx * Math.PI / 2); 

                // Multiply by max height and add extreme noise for jagged peaks
                mountainProfile[x] = (baseHeight * peakMaxHeight) + WorldGen.genRand.NextFloat(-60f, 20f);
                
                // Ensure it doesn't dip below the ground level or spike out of bounds
                if (mountainProfile[x] < 0) mountainProfile[x] = 0;
                if (mountainProfile[x] > peakMaxHeight) mountainProfile[x] = peakMaxHeight;
            }

            // 3. Smooth the Profile (Running Average)
            for (int pass = 0; pass < 5; pass++)
            {
                float prevHeight = mountainProfile[0];
                for (int x = 1; x < width - 1; x++)
                {
                    float tempHeight = mountainProfile[x];
                    mountainProfile[x] = (prevHeight + mountainProfile[x] + mountainProfile[x + 1]) / 3f;
                    prevHeight = tempHeight;
                }
            }

            // 4. Place the Snow and Ice Mass
            for (int x = 0; x < width; x++)
            {
                int i = leftBound + x;
                int topY = groundLevel - (int)mountainProfile[x];

                for (int j = topY; j <= groundLevel; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    
                    // Core is Ice, outer shell (top 15 blocks) is Snow
                    if (j < topY + 15 + WorldGen.genRand.Next(-3, 4))
                    {
                        tile.TileType = snowType;
                    }
                    else
                    {
                        tile.TileType = iceType;
                    }
                }
            }

            // 5. Carve the Dungeon Entrance (Lodged in the Right Side)
            // Positioned roughly 80% across the mountain's width
            int entranceRelX = (int)(width * 0.80f); 
            int dungeonX = leftBound + entranceRelX;
            int dungeonY = groundLevel - (int)mountainProfile[entranceRelX] + 25; // Buried slightly into the slope

            int entranceWidth = 40;
            int entranceHeight = 30;
            int shaftWidth = 26; // widened from 12 -> 26 for a much roomier vertical shaft

            // Generate the imposing exterior brick structure
            for (int i = dungeonX - entranceWidth / 2; i <= dungeonX + entranceWidth / 2; i++)
            {
                for (int j = dungeonY - entranceHeight / 2; j <= dungeonY + entranceHeight / 2; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = dungeonBrick;
                    tile.WallType = dungeonWall;
                }
            }

            // Hollow out the foyer inside the structure
            for (int i = dungeonX - (entranceWidth / 2) + 5; i <= dungeonX + (entranceWidth / 2) - 5; i++)
            {
                for (int j = dungeonY - (entranceHeight / 2) + 5; j <= dungeonY + (entranceHeight / 2) - 5; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    Tile tile = Main.tile[i, j];
                    tile.HasTile = false; // Clear block
                }
            }

            // Blast an opening pointing to the right, exposing it to the outside
            for (int i = dungeonX; i <= dungeonX + entranceWidth; i++)
            {
                for (int j = dungeonY - 4; j <= dungeonY + 4; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    Tile tile = Main.tile[i, j];
                    tile.HasTile = false;
                }
            }

            // Carve the Dungeon Shaft straight down into the solid ice base
            for (int i = dungeonX - shaftWidth / 2; i <= dungeonX + shaftWidth / 2; i++)
            {
                for (int j = dungeonY + (entranceHeight / 2) - 5; j <= Main.rockLayer; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    Tile tile = Main.tile[i, j];
                    
                    // The shaft core is empty air
                    if (Math.Abs(i - dungeonX) < (shaftWidth / 2) - 2)
                    {
                        tile.HasTile = false;
                        tile.WallType = dungeonWall;
                    }
                    // The shaft outer shell (the sides) is solid dungeon brick backed by dungeon brick wall
                    else
                    {
                        tile.HasTile = true;
                        tile.TileType = dungeonBrick;
                        tile.WallType = dungeonWall;
                    }
                }
            }

            // 6. Final Framing
            for (int i = leftBound - 10; i <= rightBound + 50; i++)
            {
                for (int j = 40; j <= Main.rockLayer + 10; j++)
                {
                    if (WorldGen.InWorld(i, j))
                    {
                        WorldGen.SquareTileFrame(i, j, true);
                        WorldGen.SquareWallFrame(i, j, true);
                    }
                }
            }
        }
    }
}