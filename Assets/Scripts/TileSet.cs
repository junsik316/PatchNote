using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Map/Tile Set")]
public class TileSet : ScriptableObject
{
    public TileBase ground;
    public TileBase wall;
    public TileBase platform;
}