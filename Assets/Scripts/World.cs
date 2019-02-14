using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

using UnityEngine;

public struct World
{
    public static World current;
    public static void Load(string world_name)
    {
        Unload();
        current.loadedChunks = new Dictionary<Vector2Int, Chunk>();
        current.name = world_name;
    }

    public static void Unload()
    {
        if (current.loadedChunks != null) Chunk.UnloadAll();
    }




    public string name;
    public Tile this[int x, int y]
    {
        get
        {
            Vector2Int chunk_position = new Vector2Int(Mathf.FloorToInt(x / (float)Chunk.chunk_size), Mathf.FloorToInt(y / (float)Chunk.chunk_size));
            Vector2Int local_tile_position = new Vector2Int(x < 0 ? (Chunk.chunk_size - 1) + (x + 1) % Chunk.chunk_size : x % Chunk.chunk_size, y < 0 ? (Chunk.chunk_size - 1) + (y + 1) % Chunk.chunk_size : y % Chunk.chunk_size);
            if (!loadedChunks.ContainsKey(chunk_position)) Chunk.Load(chunk_position);

            return loadedChunks[chunk_position].Get(local_tile_position);
        }
        set
        {
            Vector2Int chunk_position = new Vector2Int(Mathf.FloorToInt(x / (float)Chunk.chunk_size), Mathf.FloorToInt(y / (float)Chunk.chunk_size));
            Vector2Int local_tile_position = new Vector2Int(x < 0 ? (Chunk.chunk_size - 1) + (x + 1) % Chunk.chunk_size : x % Chunk.chunk_size, y < 0 ? (Chunk.chunk_size - 1) + (y + 1) % Chunk.chunk_size : y % Chunk.chunk_size);
            if (!loadedChunks.ContainsKey(chunk_position)) Chunk.Load(chunk_position);

            loadedChunks[chunk_position].Set(local_tile_position, value);
        }
    }

    Dictionary<Vector2Int, Chunk> loadedChunks;
    [Serializable]
    struct Chunk
    {
        public const int chunk_size = 512; //DANGEROUS - CHANGE ONLY WHEN NECESSARY
        Tile[,] contents;
        public Tile Get(Vector2Int pos)
        {
            return contents[pos.x, pos.y];
        }

        public void Set(Vector2Int pos, Tile value)
        {
            contents[pos.x, pos.y] = value;
        }
        public static void Load(Vector2Int position)
        {
            if (World.current.loadedChunks.ContainsKey(position))
            {
                Debug.LogWarning("Tried to load already loaded chunk at: " + position);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();

            if (File.Exists(Path.Combine(Application.persistentDataPath, position.ToString())))
            {
                FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, position.ToString()), FileMode.Open);

                World.current.loadedChunks.Add(position, (Chunk)formatter.Deserialize(stream));
                stream.Close();
            }
            else
            {
                Chunk chunk;
                chunk.contents = new Tile[chunk_size, chunk_size];

                World.current.loadedChunks.Add(position, chunk);
            }
        }
        public static void Unload(Vector2Int position)
        {
            if (!World.current.loadedChunks.ContainsKey(position))
            {
                Debug.LogWarning("Tried to unload already unloaded chunk at: " + position);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, position.ToString()), FileMode.Create);
            formatter.Serialize(stream, World.current.loadedChunks[position]);

            World.current.loadedChunks.Remove(position);
            stream.Close();
        }

        public static void UnloadAll()
        {
            List<Vector2Int> to_unload = new List<Vector2Int>(World.current.loadedChunks.Keys);
            to_unload.ForEach(x => Unload(x));
        }
    }
}
