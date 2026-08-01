using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Bathyal.Common.Systems.WorldGeneration
{
    public class TrenchGenPass : GenPass
    {
        public TrenchGenPass(string name, double loadWeight) : base(name, loadWeight) {}

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Desolate Bathyal World...";

            Main.worldSurface = (int)(Main.maxTilesY * 0.30);
            Main.rockLayer = (int)(Main.maxTilesY * 0.50);
            
            int centerX = Main.maxTilesX / 2;
            int groundLevel = (int)Main.worldSurface;
            int rockLayer = (int)Main.rockLayer;
            
            int underworldTop = Main.UnderworldLayer; 
            int platformY = groundLevel - 15;
            int ashFloorY = Main.maxTilesY - 45; 

            int maxTrenchDepth = 3250;
            int trenchTotalHeight = maxTrenchDepth - groundLevel;

            float[] dividerPercentages = { 0.10f, 0.25f, 0.45f, 0.70f };
            int[] zoneDividers = new int[dividerPercentages.Length];
            
            for (int i = 0; i < dividerPercentages.Length; i++)
            {
                zoneDividers[i] = groundLevel + (int)(trenchTotalHeight * dividerPercentages[i]);
            }

            for (int y = 0; y < Main.maxTilesY; y++)
            {
                double progressY = Math.Min(1.0, (double)y / maxTrenchDepth);
                int wallOffset = (int)(Math.Sin(y * 0.06) * 5 + Math.Cos(y * 0.02) * 3);
                int halfWidth = (int)(1250 - (750 * progressY)) + wallOffset;

                int leftBound = centerX - halfWidth;
                int rightBound = centerX + halfWidth;

                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;

                    Tile tile = Main.tile[x, y];
                    tile.ClearEverything();

                    bool isLeftWall = Math.Abs(x - leftBound) <= 4;
                    bool isRightWall = Math.Abs(x - rightBound) <= 4;
                    bool inTrench = (x > leftBound + 4 && x < rightBound - 4);

                    if (y >= underworldTop)
                    {
                        if (y >= ashFloorY)
                        {
                            tile.HasTile = true;
                            tile.TileType = TileID.Ash;
                        }
                        continue;
                    }

                    if (y == platformY && x >= centerX - 10 && x <= centerX + 10)
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.LihzahrdBrick;
                        
                        Main.spawnTileX = centerX;
                        Main.spawnTileY = platformY - 2;
                    }
                    else if ((isLeftWall || isRightWall) && y >= groundLevel - 10 && y <= maxTrenchDepth)
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.LihzahrdBrick;
                    }
                    else if (inTrench && y <= maxTrenchDepth)
                    {
                        if (y >= groundLevel)
                        {
                            bool isDivider = false;
                            
                            for (int i = 0; i < zoneDividers.Length; i++)
                            {
                                int baseY = zoneDividers[i];
                                
                                if (Math.Abs(y - baseY) < 25) 
                                {
                                    double wave = Math.Sin(x * 0.08) * 3 + Math.Cos(x * 0.03) * 2;
                                    int halfThickness = 7 + (int)wave; 
                                    
                                    if (Math.Abs(y - baseY) <= halfThickness)
                                    {
                                        isDivider = true;
                                        break;
                                    }
                                }
                            }

                            if (isDivider)
                            {
                                tile.HasTile = true;
                                tile.TileType = TileID.LihzahrdBrick;
                            }
                        }
                    }
                    else
                    {
                        if (y >= groundLevel)
                        {
                            tile.HasTile = true;

                            if (x <= leftBound)
                            {
                                tile.TileType = (ushort)(y < rockLayer ? TileID.SnowBlock : TileID.IceBlock);
                            }
                            else if (x >= rightBound)
                            {
                                if (y < groundLevel + 30)
                                    tile.TileType = TileID.Sand;
                                else if (y < rockLayer)
                                    tile.TileType = TileID.HardenedSand;
                                else
                                    tile.TileType = TileID.Sandstone;
                            }
                        }
                    }
                }
            }
        }
    }
}