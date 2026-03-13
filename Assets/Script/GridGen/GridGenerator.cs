using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.GridGen.Math;
using Script.GridGen.Tiles;

namespace Script.GridGen
{
    public class GridGenerator : MonoBehaviour
    {
        public GridConfig gridConfig;

        public Vector2 tileDiv;
        public Vector3 snapGrid, gridCenterLocation, gridBottomLeftLocation;

        public readonly Dictionary<Vector2Int, GridTiles> SpawnedHexTiles = new();

        private IEnumerator Start()
        {
            var tileSize = gridConfig.gridTileSize;

            snapGrid = GridMath.CalculateSnapGrid(tileSize);
            tileDiv = GridMath.CalculateTileDiv(tileSize);

            gridCenterLocation = GridMath.SnapVectorToVector(transform.position, tileSize);

            gridBottomLeftLocation = GridMath.CalculateGridBottomLeft(gridCenterLocation, gridConfig.GetGridTileCount(), tileSize);

            var gridGenSeed = Random.Range(0, 10);
            yield return null;

            yield return GenerateBoard(gridGenSeed);
        }

        private IEnumerator GenerateBoard(int seed)
        {
            var currentX = 0;
            var currentYIndex = 0;
            var currentColumnRows = new List<int>();

            var gridSize = gridConfig.GetGridTileCount();

            var shuffled = gridConfig.GetShuffledTiles(seed);
            var tileTypes = shuffled.ShuffledTileTypes;
            var tileDiceNumbers = shuffled.ShuffledDiceNumbers;

            var tileTypeIndex = 0;

            while (true)
            {
                // Move to next column if needed
                if (currentColumnRows.Count == 0)
                {
                    if (currentX >= gridSize.x)
                        yield break;

                    GridMath.ForEachValidHexRow(currentX, gridSize, (y) => { currentColumnRows.Add(y); });

                    currentYIndex = 0;
                }

                var y = currentColumnRows[currentYIndex++];
                var tileIndex = GridMath.MakeTileIndex(currentX, y);

                SpawnSingleHex(tileIndex);

                if (currentYIndex >= currentColumnRows.Count)
                {
                    currentColumnRows.Clear();
                    currentX++;
                }

                yield return new WaitForSeconds(0.05f);
            }

            void SpawnSingleHex(Vector2Int tileIndex)
            {
                var tileSize = gridConfig.gridTileSize;

                var tileScale = new Vector3(gridConfig.tileMeshSize.x / tileSize.x, gridConfig.tileMeshSize.y / tileSize.y, gridConfig.tileMeshSize.z / tileSize.z);

                var settlementScale = new Vector3(gridConfig.settlementMeshSize.x / tileSize.x, gridConfig.settlementMeshSize.y / tileSize.y, gridConfig.settlementMeshSize.z / tileSize.z);

                var tileLocation = GridMath.TileIndexToWorld(tileIndex, gridBottomLeftLocation, tileSize);

                if (!SpawnedHexTiles.TryGetValue(tileIndex, out var tile) || tile == null)
                {
                    var obj = Instantiate(gridConfig.gridTilesClass, tileLocation, Quaternion.identity);
                    obj.transform.SetParent(transform, true);
                    obj.transform.localScale = tileScale;

                    tile = obj;

                    tile.tileCoordinates = tileIndex;
                    tile.tileType = tileTypes[tileTypeIndex % tileTypes.Count];
                    tile.diceNumber = tileDiceNumbers[tileTypeIndex];
                    tile.tileColor = gridConfig.TileTypeColor[tile.tileType];

                    SpawnedHexTiles[tileIndex] = tile;

                    tileTypeIndex++;
                }

                GridMath.SpawnVerticesAndEdges(tile.transform, tileIndex, gridBottomLeftLocation, tileSize, settlementScale, SpawnedHexTiles, gridConfig);
            }
        }
    }
}
