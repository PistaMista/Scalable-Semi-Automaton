using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public static Tilemap tilemap;
    public TileBase tileOn;
    public TileBase tileOff;
    void Start()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        position_set_in_one_go = new List<Vector2Int>();

        World.Load("test");
    }

    List<Vector2Int> position_set_in_one_go;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {

            Vector3Int tilemap_position = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector2Int cell_position = new Vector2Int(tilemap_position.x, tilemap_position.y);

            if (!position_set_in_one_go.Contains(cell_position))
            {
                SetTile(cell_position, tilemap.GetTile(tilemap_position) == tileOff);
                //World.current[cell_position.x, cell_position.y] = 
                position_set_in_one_go.Add(cell_position);
            }
        }
        else if (Input.GetMouseButtonUp(0)) position_set_in_one_go = new List<Vector2Int>();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyRules();
        }
    }

    void SetTile(Vector2Int cellPosition, bool active)
    {
        tilemap.SetTile(new Vector3Int(cellPosition.x, cellPosition.y, 0), active ? tileOn : tileOff);
        World.current[cellPosition.x, cellPosition.y] = new Tile(active);
    }

    void ApplyRules()
    {

    }

    void OnApplicationQuit()
    {
        World.Unload();
    }
}

[System.Serializable]
public struct Tile
{
    public Tile(bool alive)
    {
        this.alive = alive;
    }
    public bool alive;
}
