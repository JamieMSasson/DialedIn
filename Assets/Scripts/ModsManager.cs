using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ModsManager : MonoBehaviour
{
    public static ModsManager Instance{ get; private set; }

    private List<ModData> activeMods = new();

    private ScoreManager scoreManager;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SetScoreManager(ScoreManager manager)
    {
        scoreManager = manager;
    }

    /// <summary>
    /// Trigger all active mods that trigger on score
    /// </summary>
    public void TriggerScoreMods()
    {
        Vector2 scoreValues = Vector2.zero;

        foreach(ModData mod in activeMods)
        {
            if(mod == null)
            {
                continue;
            }

            mod.OnScore(scoreManager);
        }
    }

    public void AddMod(ModData mod)
    {
        if(mod == null || activeMods.Count == 4)
        {
            return;
        }

        activeMods.Add(mod);
    }

    public void RemoveMod(ModData mod)
    {
        activeMods.Remove(mod);
    }
}
