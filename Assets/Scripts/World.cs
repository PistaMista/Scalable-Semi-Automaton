using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

using UnityEngine;

public struct World
{
    public static World current;
    const string world_info_format = ".ssaworld";
    const string world_chunk_format = ".ssachunk";
    public static void Create(string world_name)
    {
        string world_path = GetSaveFolderPath(world_name);
        if (Directory.Exists(world_path)) return;

        Directory.CreateDirectory(world_path);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(GetInfoFilePath(world_name), FileMode.OpenOrCreate);

        formatter.Serialize(stream, new World(world_name));
        stream.Close();
    }
    public static void Delete(string world_name)
    {
        if (current.name == world_name) Unload();

        string world_path = GetSaveFolderPath(world_name);
        if (!Directory.Exists(world_path)) return;

        Directory.Delete(world_path, true);
    }
    public static bool Load(string world_name)
    {
        Unload();

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(GetInfoFilePath(world_name), FileMode.Open);
        }
        catch
        {
            Debug.LogWarning("There was an issue with loading this world, recreating.");
            return false;
        }

        current.loadedChunks = new Dictionary<Vector2Int, Chunk>();
        return true;

        if (!(Directory.Exists(world_path)))
        {
            Directory.CreateDirectory(world_path);
            current.name = world_name;
            current.chunk_size = Chunk.default_chunk_size;
        }


    }
    public static void Unload()
    {
        if (current.loadedChunks != null) Chunk.UnloadAll();
    }

    public static string GetSaveFolderPath(string world_name)
    {
        if (world_name.Contains("/")) throw new Exception("World names cannot have '/' in them.");
        return Path.Combine(Application.persistentDataPath, "Worlds/" + world_name);
    }
    public static string GetInfoFilePath(string world_name)
    {
        return GetSaveFolderPath(world_name) + "/info" + world_info_format;
    }

    public static string GetChunkFilePath(string world_name, Vector2Int chunkPosition)
    {
        return GetSaveFolderPath(world_name) + "/" + chunkPosition.ToString() + world_chunk_format;
    }

    World(string name)
    {
        this.name = name;
        this.chunk_size = Chunk.default_chunk_size;
        this.loadedChunks = null;
    }

    public string name;
    public int chunk_size;

    public Tile this[int x, int y]
    {
        get
        {
            Vector2Int chunk_position = new Vector2Int(Mathf.FloorToInt(x / (float)Chunk.default_chunk_size), Mathf.FloorToInt(y / (float)Chunk.default_chunk_size));
            Vector2Int local_tile_position = new Vector2Int(x < 0 ? (Chunk.default_chunk_size - 1) + (x + 1) % Chunk.default_chunk_size : x % Chunk.default_chunk_size, y < 0 ? (Chunk.default_chunk_size - 1) + (y + 1) % Chunk.default_chunk_size : y % Chunk.default_chunk_size);
            if (!loadedChunks.ContainsKey(chunk_position)) Chunk.Load(chunk_position);

            return loadedChunks[chunk_position].Get(local_tile_position);
        }
        set
        {
            Vector2Int chunk_position = new Vector2Int(Mathf.FloorToInt(x / (float)Chunk.default_chunk_size), Mathf.FloorToInt(y / (float)Chunk.default_chunk_size));
            Vector2Int local_tile_position = new Vector2Int(x < 0 ? (Chunk.default_chunk_size - 1) + (x + 1) % Chunk.default_chunk_size : x % Chunk.default_chunk_size, y < 0 ? (Chunk.default_chunk_size - 1) + (y + 1) % Chunk.default_chunk_size : y % Chunk.default_chunk_size);
            if (!loadedChunks.ContainsKey(chunk_position)) Chunk.Load(chunk_position);

            loadedChunks[chunk_position].Set(local_tile_position, value);
        }
    }

    [NonSerialized]
    Dictionary<Vector2Int, Chunk> loadedChunks;
    [Serializable]
    struct Chunk
    {
        public const int default_chunk_size = 64; //DANGEROUS - CHANGE ONLY WHEN NECESSARY
        Tile[,] contents;
        public static void MapToChunkCoordinates()
        {

        }
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
            string file_path = GetChunkFilePath(current.name, position);
            if (World.current.loadedChunks.ContainsKey(position))
            {
                Debug.LogWarning("Tried to load already loaded chunk at: " + position);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();

            if (File.Exists(file_path))
            {
                FileStream stream = new FileStream(file_path, FileMode.Open);

                World.current.loadedChunks.Add(position, (Chunk)formatter.Deserialize(stream));
                stream.Close();
            }
            else
            {
                Chunk chunk;
                chunk.contents = new Tile[default_chunk_size, default_chunk_size];

                World.current.loadedChunks.Add(position, chunk);
            }
        }
        public static void Unload(Vector2Int position)
        {
            string file_path = GetChunkFilePath(current.name, position);
            if (!World.current.loadedChunks.ContainsKey(position))
            {
                Debug.LogWarning("Tried to unload already unloaded chunk at: " + position);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(file_path, FileMode.Create);
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
