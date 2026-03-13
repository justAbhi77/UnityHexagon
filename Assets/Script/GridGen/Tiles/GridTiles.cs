using UnityEngine;
using Script.GridGen.Enums;

namespace Script.GridGen.Tiles
{
    public class GridTiles : MonoBehaviour
    {
        public Vector2Int tileCoordinates;

        public Color tileColor;

        public GridTileTypes tileType = GridTileTypes.None;

        public int diceNumber;

        public int owningPlayerIndex = -1;
    }
}