﻿using HappyFunTimes;
using System.Collections.Generic;
using UnityEngine;

// There is supposed to be only 1 of these.
// Other objects can use LevelSettings.settings to
// access all the global settings
public class LevelSettings : MonoBehaviour
{
    public Transform bottomOfLevel;
    public Transform leftEdgeOfLevel;
    public Transform rightEdgeOfLevel;
    public Transform[] spawnPoints;
    public int numGames;

    public PlayerSpawner playerSpawner
    {
        get
        {
            return m_playerSpawner;
        }
    }

    public static LevelSettings settings
    {
        get
        {
            return s_settings;
        }
    }

    private PlayerSpawner m_playerSpawner;
    static private LevelSettings s_settings;

    void Awake()
    {
        if (s_settings != null)
        {
            throw new System.InvalidProgramException("there is more than one level settings object!");
        }
        s_settings = this;

        ArgParser p = new ArgParser();
        p.TryGet<int>("num-games", ref numGames);

        m_playerSpawner = GetComponent<PlayerSpawner>();
    }

    void Cleanup()
    {
        s_settings = null;
    }

    void OnDestroy()
    {
        Cleanup();
    }

    void OnApplicationExit()
    {
        Cleanup();
    }
}
