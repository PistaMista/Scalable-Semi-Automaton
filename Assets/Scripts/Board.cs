using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    Tilemap tilemap;
    public TileBase tileOn;
    public TileBase tileOff;
    void Start()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        set_positions = new List<Vector3Int>();
    }

    List<Vector3Int> set_positions;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3Int cell_position = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (!set_positions.Contains(cell_position))
            {
                tilemap.SetTile(cell_position, tilemap.GetTile(cell_position) == tileOff ? tileOn : tileOff);
                set_positions.Add(cell_position);
            }
        }
        else if (Input.GetMouseButtonUp(0)) set_positions = new List<Vector3Int>();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyRules();
        }
    }

    void ApplyRules()
    {

    }
}
