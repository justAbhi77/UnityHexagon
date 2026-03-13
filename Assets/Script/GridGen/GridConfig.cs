using System.Collections.Generic;
using Script.GridGen.Enums;
using Script.GridGen.Structs;
using UnityEngine;
using Script.GridGen.Tiles;

namespace Script.GridGen
{
    [CreateAssetMenu(fileName = "Generate Grid Config", menuName = "Generate Grid Config", order = 0)]
    public class GridConfig : ScriptableObject
    {
        public int gridSize = 3;

        public Vector3 gridTileSize = new Vector3(115f, 10f, 100f);

        public Vector3 tileMeshSize = new Vector3(115f, 10f, 100f);

        public Vector3 settlementMeshSize = new Vector3(25f, 3f, 25f);

        public float roadMeshSizeY = 5f;

        public GridTiles gridTilesClass;

        public GridTiles settlementTileClass;

        public GridTiles roadTileClass;

        public Dictionary<GridTileTypes, int> TileTypeDistribution = new Dictionary<GridTileTypes, int>
        {
            { GridTileTypes.Desert, 1 },
            { GridTileTypes.Brick, 3 },
            { GridTileTypes.Forest, 4 },
            { GridTileTypes.Mountain, 3 },
            { GridTileTypes.Farm, 4 },
            { GridTileTypes.Grassland, 4 }
        };

        public Dictionary<GridTileTypes, Color> TileTypeColor = new Dictionary<GridTileTypes, Color>
        {
            { GridTileTypes.Desert, new Color(0.85f, 0.80f, 0.55f, 1f) }, // sand
            { GridTileTypes.Brick, new Color(0.72f, 0.25f, 0.20f, 1f) }, // clay
            { GridTileTypes.Forest, new Color(0.10f, 0.45f, 0.15f, 1f) }, // wood
            { GridTileTypes.Mountain, new Color(0.45f, 0.45f, 0.48f, 1f) }, // ore
            { GridTileTypes.Farm, new Color(0.95f, 0.85f, 0.30f, 1f) }, // wheat
            { GridTileTypes.Grassland, new Color(0.35f, 0.70f, 0.30f, 1f) }, // sheep
        };

        public Vector2Int GetGridTileCount()
        {
            var tileSize = gridSize * 2 - 1;
            return new Vector2Int(tileSize, tileSize);
        }

        public static void ShuffleArray<T>(List<T> array, int seed)
        {
            var random = new System.Random(seed);
            var lastIndex = array.Count - 1;

            for (var i = 0; i < lastIndex; i++)
            {
                var index = random.Next(0, lastIndex + 1);

                if (i == index) continue;

                (array[i], array[index]) = (array[index], array[i]);
            }
        }

        public ShuffledTiles GetShuffledTiles(int seed)
        {
            var tileTypes = new List<GridTileTypes>();

            foreach (var pair in TileTypeDistribution)
            {
                for (var i = 0; i < pair.Value; i++)
                {
                    tileTypes.Add(pair.Key);
                }
            }

            // Standard Catan dice number distribution (excluding desert)
            var diceNumbers = new List<int> {
                2,
                3, 3,
                4, 4,
                5, 5,
                6, 6,
                8, 8,
                9, 9,
                10, 10,
                11, 11,
                12
            };

            ShuffleArray(tileTypes, seed);
            ShuffleArray(diceNumbers, seed + 1);

            var shuffledTiles = new ShuffledTiles
            {
                ShuffledTileTypes = tileTypes
            };

            var diceIndex = 0;

            foreach (var tileType in tileTypes)
            {
                if (tileType == GridTileTypes.Desert)
                {
                    shuffledTiles.ShuffledDiceNumbers.Add(-1); // no dice
                }
                else
                {
                    shuffledTiles.ShuffledDiceNumbers.Add(diceNumbers[diceIndex]);
                    diceIndex++;
                }
            }

            return shuffledTiles;
        }
    }
}
