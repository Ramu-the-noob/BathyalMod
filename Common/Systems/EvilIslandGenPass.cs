using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Bathyal.Common.Systems.WorldGeneration
{
    public class EvilIslandGenPass : GenPass
    {
        public EvilIslandGenPass(string name, double loadWeight) : base(name, loadWeight) {}

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Raising the Evil Island...";

            // Recalculate positional data for this specific module
            int centerX = Main.maxTilesX / 2;
            int groundLevel = (int)(Main.maxTilesY * 0.30);

            GenerateEvilIsland(centerX, groundLevel);
        }

        private void GenerateEvilIsland(int cX, int groundLevel)
        {
            int width = 340; 
            int leftEdge = cX - width / 2;

            ushort stoneType = WorldGen.crimson ? TileID.Crimstone : TileID.Ebonstone;
            ushort grassType = WorldGen.crimson ? TileID.CrimsonGrass : TileID.CorruptGrass;
            ushort wallType = WorldGen.crimson ? WallID.CrimstoneUnsafe : WallID.EbonstoneUnsafe;
            ushort dirtType = TileID.Dirt;
            ushort plantType = WorldGen.crimson ? TileID.CrimsonPlants : TileID.CorruptPlants;
            ushort oreType = WorldGen.crimson ? TileID.Crimtane : TileID.Demonite;

            float[] topProfile = new float[width];
            float[] bottomProfile = new float[width];
            float[] dirtProfile = new float[width];

            for (int x = 0; x < width; x++)
            {
                float nx = (x - (width / 2f)) / (width / 2f);
                float baseHeight = (float)Math.Sqrt(1 - nx * nx); 

                topProfile[x] = baseHeight * 130 + WorldGen.genRand.NextFloat(-35f, 35f);
                bottomProfile[x] = baseHeight * 70 + WorldGen.genRand.NextFloat(-25f, 25f);
                dirtProfile[x] = WorldGen.genRand.NextFloat(15f, 40f); 
            }

            for (int pass = 0; pass < 8; pass++) 
            {
                float prevTop = topProfile[0];
                float prevBottom = bottomProfile[0];
                float prevDirt = dirtProfile[0];

                for (int x = 1; x < width - 1; x++)
                {
                    float tempTop = topProfile[x];
                    topProfile[x] = (prevTop + topProfile[x] + topProfile[x + 1]) / 3f;
                    prevTop = tempTop;

                    float tempBottom = bottomProfile[x];
                    bottomProfile[x] = (prevBottom + bottomProfile[x] + bottomProfile[x + 1]) / 3f;
                    prevBottom = tempBottom;

                    float tempDirt = dirtProfile[x];
                    dirtProfile[x] = (prevDirt + dirtProfile[x] + dirtProfile[x + 1]) / 3f;
                    prevDirt = tempDirt;
                }
            }

            for (int x = 0; x < width; x++)
            {
                int i = leftEdge + x;
                int topY = groundLevel - (int)topProfile[x];
                int bottomY = groundLevel + (int)bottomProfile[x] - 15;

                for (int j = topY; j <= bottomY; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    
                    if (j < topY + (int)dirtProfile[x])
                    {
                        tile.TileType = dirtType;
                    }
                    else
                    {
                        tile.TileType = stoneType;
                    }
                }
            }

            int hollowWidth = 150;
            int hollowLeft = cX - hollowWidth / 2;
            float[] hollowTop = new float[hollowWidth];
            float[] hollowBottom = new float[hollowWidth];

            for (int x = 0; x < hollowWidth; x++)
            {
                float nx = (x - (hollowWidth / 2f)) / (hollowWidth / 2f);
                float baseHeight = (float)Math.Sqrt(1 - nx * nx);
                hollowTop[x] = baseHeight * 55 + WorldGen.genRand.NextFloat(-20f, 20f);
                hollowBottom[x] = baseHeight * 45 + WorldGen.genRand.NextFloat(-20f, 20f);
            }

            for (int pass = 0; pass < 6; pass++)
            {
                float pTop = hollowTop[0];
                float pBot = hollowBottom[0];
                for (int x = 1; x < hollowWidth - 1; x++)
                {
                    float tTop = hollowTop[x];
                    hollowTop[x] = (pTop + hollowTop[x] + hollowTop[x + 1]) / 3f;
                    pTop = tTop;

                    float tBot = hollowBottom[x];
                    hollowBottom[x] = (pBot + hollowBottom[x] + hollowBottom[x + 1]) / 3f;
                    pBot = tBot;
                }
            }

            int hollowCenterY = groundLevel - 45;

            for (int x = 0; x < hollowWidth; x++)
            {
                int i = hollowLeft + x;
                int topY = hollowCenterY - (int)hollowTop[x];
                int bottomY = hollowCenterY + (int)hollowBottom[x];

                for (int j = topY; j <= bottomY; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    Tile tile = Main.tile[i, j];
                    tile.HasTile = false;
                    tile.LiquidAmount = 0;
                    tile.WallType = wallType;
                }
            }

            int shaftWidth = 10;
            for (int i = cX - shaftWidth; i <= cX + shaftWidth; i++)
            {
                int surfaceY = groundLevel - 150; 
                while (surfaceY < groundLevel && !Main.tile[i, surfaceY].HasTile) surfaceY++;

                for (int j = surfaceY - 5; j <= hollowCenterY; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;

                    float wiggledX = i + (float)Math.Sin(j * 0.03f) * 7f;

                    if (Math.Abs(wiggledX - cX) <= shaftWidth / 2)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = false;
                        tile.WallType = wallType;
                    }
                }
            }

            for (int i = leftEdge; i <= leftEdge + width; i++)
            {
                for (int j = groundLevel - 160; j <= groundLevel + 100; j++)
                {
                    if (WorldGen.InWorld(i, j) && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == dirtType)
                    {
                        if (!Main.tile[i, j - 1].HasTile)
                        {
                            Tile tile = Main.tile[i, j];
                            tile.TileType = grassType;

                            if (WorldGen.genRand.NextBool(5))
                                WorldGen.GrowTree(i, j - 1);
                            else if (WorldGen.genRand.NextBool(4))
                                WorldGen.PlaceTile(i, j - 1, plantType, true, true, -1, WorldGen.genRand.Next(6));
                            
                            break; 
                        }
                    }
                }
            }

            int orbStyle = WorldGen.crimson ? 1 : 0;
            int altarStyle = WorldGen.crimson ? 1 : 0;

            for (int k = 0; k < 60; k++)
            {
                int rx = cX + WorldGen.genRand.Next(-hollowWidth / 2 + 15, hollowWidth / 2 - 15);
                int ry = hollowCenterY + WorldGen.genRand.Next(-40, 40);

                if (WorldGen.InWorld(rx, ry) && !Main.tile[rx, ry].HasTile)
                {
                    while (!Main.tile[rx, ry].HasTile && ry < hollowCenterY + 60) ry++;

                    ry--;

                    if (WorldGen.genRand.NextBool(2))
                        WorldGen.Place3x2(rx, ry, TileID.DemonAltar, altarStyle);
                    else
                        WorldGen.PlaceTile(rx, ry, TileID.ShadowOrbs, true, true, -1, orbStyle);
                }
            }

            int oreVeinCount = 200; 

            for (int k = 0; k < oreVeinCount; k++)
            {
                int rx = leftEdge + WorldGen.genRand.Next(width);
                int ry = groundLevel - 150 + WorldGen.genRand.Next(250);

                if (WorldGen.InWorld(rx, ry) && Main.tile[rx, ry].HasTile && Main.tile[rx, ry].TileType == stoneType)
                {
                    int strength = WorldGen.genRand.Next(3, 7);
                    int steps = WorldGen.genRand.Next(3, 7);
                    
                    int currentX = rx;
                    int currentY = ry;

                    while (strength > 0 && steps > 0)
                    {
                        int minX = currentX - strength / 2;
                        int maxX = currentX + strength / 2;
                        int minY = currentY - strength / 2;
                        int maxY = currentY + strength / 2;

                        for (int vx = minX; vx <= maxX; vx++)
                        {
                            for (int vy = minY; vy <= maxY; vy++)
                            {
                                if (WorldGen.InWorld(vx, vy))
                                {
                                    float dist = (float)Math.Sqrt((vx - currentX) * (vx - currentX) + (vy - currentY) * (vy - currentY));
                                    if (dist <= strength / 2f)
                                    {
                                        Tile tile = Main.tile[vx, vy];
                                        if (tile.HasTile && tile.TileType == stoneType)
                                        {
                                            tile.TileType = oreType;
                                        }
                                    }
                                }
                            }
                        }
                        steps--;
                        currentX += WorldGen.genRand.Next(-2, 3);
                        currentY += WorldGen.genRand.Next(-2, 3);
                        strength += WorldGen.genRand.Next(-1, 2); 
                    }
                }
            }

            for (int i = leftEdge - 15; i <= leftEdge + width + 15; i++)
            {
                for (int j = groundLevel - 170; j <= groundLevel + 100; j++)
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