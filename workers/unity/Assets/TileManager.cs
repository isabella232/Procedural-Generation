using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Gamelogic.Visualizers
{
    public class TileManager : MonoBehaviour
    {
        public static Dictionary<int, GameObject> tileMappings =
            new Dictionary<int, GameObject>(1000);

        public static UnityEngine.Object[] terrainPrefabs = new UnityEngine.Object[6];

        // Dimensions hardcoded to be slightly longer than prefab to make individual tiles visible
        private const float tileSideLength = 11.5f;
        private const float tileHeightOffset = -10f;

        // Number of tiles to generate on either side of a player's current tile
        private const int interestRadius = 3;

        // Tile index values can be no greater than +/- arbitraryMaxTileDim
        private const int arbitraryMaxTileDim = 1000;

        //Efficiency: don't need to be calling Mathf.Sqrt(3) over and over again for the same result
        private const float sqrt3 = 1.73205080757f;

        //2D Random Maps for procedural generation
        static Color[] noiseValues;
        static Texture2D smoothNoiseA;
        static Texture2D smoothNoiseB;

        protected void OnEnable()
        {
            noiseValues = new Color[RandomProcGenSeed.noiseWidth * RandomProcGenSeed.noiseHeight];
            smoothNoiseA = new Texture2D(RandomProcGenSeed.noiseWidth, RandomProcGenSeed.noiseHeight);
            smoothNoiseB = new Texture2D(RandomProcGenSeed.noiseWidth, RandomProcGenSeed.noiseHeight);
            
            GenerateSmoothNoise(smoothNoiseA, RandomProcGenSeed.noiseASeed, RandomProcGenSeed.noiseAScale);
            GenerateSmoothNoise(smoothNoiseB, RandomProcGenSeed.noiseBSeed, RandomProcGenSeed.noiseBScale);

            terrainPrefabs[0] = Resources.Load("HexTiles/Tile_City01");
            terrainPrefabs[1] = Resources.Load("HexTiles/Tile_City02");
            terrainPrefabs[2] = Resources.Load("HexTiles/Tile_Fields");
            terrainPrefabs[3] = Resources.Load("HexTiles/Tile_Forrest");
            terrainPrefabs[4] = Resources.Load("HexTiles/Tile_Mountains");
            terrainPrefabs[5] = Resources.Load("HexTiles/Tile_Village");
        }

        public static void GenerateAroundPosition(Vector3 pos)
        {
            int[] hexIndices = CartesianToAxialIndex(pos);
            int[][] neighbourhoodHexIndices = NeighbouringHexIndices(hexIndices[0], hexIndices[1], interestRadius);
            GenerateTiles(neighbourhoodHexIndices);
        }

        private static void GenerateTiles(int[][] tileIndexes)
        {
            foreach (int[] indexTuple in tileIndexes)
            {
                int key = HashTupleToInt(indexTuple);
                if (!tileMappings.ContainsKey(key))
                {
                    tileMappings.Add(key, GenerateTile(indexTuple));
                }
            }
        }

        private static GameObject GenerateTile(int[] indexTuple)
        {
            Vector3 tilePos = AxialIndexToCartesian(indexTuple[0], indexTuple[1]);
            int terrainPrefabId = ChooseTilePrefab(tilePos);
            GameObject tile = (GameObject)Instantiate(terrainPrefabs[terrainPrefabId]);
            tilePos.y = tileHeightOffset;
            tile.transform.position = tilePos;

            //Randomise the rotation using the index as a hash
            //Hexagon tiles have 6 possible states of rotation
            int rotationState = HashTupleToInt(indexTuple)%6;
            tile.transform.rotation *= Quaternion.Euler(0, 60 * rotationState, 0);

            return tile;
        }

        private static int ChooseTilePrefab(Vector3 tilePos)
        {
            float noiseAValue = smoothNoiseA.GetPixel((int)tilePos.x, (int)tilePos.z).r;
            float noiseBValue = smoothNoiseB.GetPixel((int)tilePos.x, (int)tilePos.z).r;
            int chosenTilePrefab = 0;

            //Probabalistically assign a tile prefab
            if (noiseAValue < 0.3f)
            {
                if (noiseBValue < 0.5f)
                {
                    chosenTilePrefab = 0;
                }
                else
                {
                    chosenTilePrefab = 1;
                }
            }
            else if (noiseAValue < 0.6f)
            {
                if (noiseBValue < 0.3f)
                {
                    chosenTilePrefab = 2;
                }
                else
                {
                    chosenTilePrefab = 3;
                }
            }
            else
            {
                if (noiseBValue < 0.5f)
                {
                    chosenTilePrefab = 4;
                }
                else
                {
                    chosenTilePrefab = 5;
                }
            }
            return chosenTilePrefab;
        }

        /* Utility functions for generating noise, converting between coordinate systems, etc. 
           There are plenty of online resources to describe Perlin Noise generation, as
           well as the geometric properties of hex grids        */

        static private void GenerateSmoothNoise(Texture2D noiseTex, int noiseSeed, float noiseScale)
        {
            //Produce Perlin Noise
            float z = 0.0F;
            while (z < noiseTex.height)
            {
                float x = 0.0F;
                while (x < noiseTex.width)
                {
                    float xCoord = x / noiseTex.width * noiseScale;
                    float zCoord = z / noiseTex.height * noiseScale;
                    float sample = Mathf.PerlinNoise(noiseSeed * xCoord, noiseSeed * zCoord);
                    noiseValues[(int)(z * noiseTex.width + x)] = new Color(sample, sample, sample);
                    x++;
                }
                z++;
            }
            noiseTex.SetPixels(noiseValues);
            noiseTex.Apply();
        }

        private static int HashTupleToInt(int[] indexTuple)
        {
            return arbitraryMaxTileDim * indexTuple[0] + indexTuple[1];
        }

        private static int[] CartesianToAxialIndex(Vector3 cartesian)
        {
            //Get the hex tile index pair (axial coordinates) for given world coordinates (x,z)
            int u = Mathf.RoundToInt(cartesian.x / (sqrt3 * tileSideLength) - cartesian.z / (tileSideLength * 3));
            int v = Mathf.RoundToInt(cartesian.z / (1.5f * tileSideLength));

            return new int[] { u, v };
        }

        private static Vector3 AxialIndexToCartesian(int u, int v)
        {
            //Get the (x,z) coordinates of the centre of a given hex tile index pair
            float x = tileSideLength * sqrt3 * u + tileSideLength * sqrt3/2 * v;
            float z = tileSideLength * 1.5f * v;

            return new Vector3(x, 0.0f, z);
        }

        private static int[][] NeighbouringHexIndices(int u, int v, int radius)
        {
            List<int[]> rangeMask = new List<int[]> { };

            //Start by building a 'mask' (centred around the origin) of hexes with the radius
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    //A hex grid element is within a given radius if it satisfies the property below
                    //If you want a fun maths proof of this, google 'hexagonal cubic coordinates'
                    if (Math.Max(Math.Abs(i), Math.Max(Math.Abs(j), Math.Abs(-j - i))) <= radius)
                    {
                        //Offset the mask by the origin coordinates
                        rangeMask.Add(new int[] { u + i, v + j});
                    }
                }
            }
            return rangeMask.ToArray();
        }
    }
}
