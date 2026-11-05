using UnityEngine;
using UnityEngine.Tilemaps;

public class NavigationMapInfo : MonoBehaviour
{
    public int width;
    public int height;

    public float cellSize = 1f;
    public Vector2 origin;

    public Tilemap[] walkableTilemaps;
}
