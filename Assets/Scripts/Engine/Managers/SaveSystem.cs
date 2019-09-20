﻿using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static readonly string m_Path = 
        Application.persistentDataPath + "/save.bin";

    public static void SaveState(SaveData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(m_Path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }
    public static SaveData? LoadState()
    {
        if (!File.Exists(m_Path))
            return null;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(m_Path, FileMode.Open);

        SaveData data = (SaveData)formatter.Deserialize(stream);
        stream.Close();

        return data;
    }
    public static void DeleteSave()
    {
        if (File.Exists(m_Path))
            File.Delete(m_Path);
    }
}
