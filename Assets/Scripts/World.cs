using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

using UnityEngine;

public class World
{
    public static World current;
    public string name;
    public bool this[int x, int y]
    {
        get
        {

        }
        set
        {

        }
    }

    Dictionary<Vector2Int, Chunk> loadedChunks;

    [Serializable]
    struct Chunk
    {
        public bool[,] contents;
        public static void Load(Vector2Int position)
        {
            if (World.current.loadedChunks.ContainsKey(position))
            {
                Debug.LogWarning("Tried to load already loaded chunk at: " + position);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, World.current.name + "/" + position.ToString()), FileMode.Open);

            World.current.loadedChunks.Add(position, (Chunk)formatter.Deserialize(stream));
            stream.Close();
        }
        public static void Unload(Vector2Int position)
        {
            if (!World.current.loadedChunks.ContainsKey(position))
            {
                Debug.LogWarning("Tried to unload already unloaded chunk at: " + position);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, World.current.name + "/" + position.ToString()), FileMode.Create);
            formatter.Serialize(stream, World.current.loadedChunks[position]);

            World.current.loadedChunks.Remove(position);
            stream.Close();
        }
    }
}
