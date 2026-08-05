using UnityEngine;

public enum ModType
{
    Scoring,
    GemGeneration,
    Matching,
    MoveRun,
    Shop
}

public enum ModRarity
{
    Common,
    Uncommon,
    Rare,
    Glitched
}

public abstract class ModData : ScriptableObject
{
    [Header("Display Info")]
    public string modName;

    [TextArea]
    public string modDescription;

    public Sprite image;

    [Header("Shop Info")]
    public int cost;
    public ModRarity rarity;

    public virtual void OnScore(ScoreManager scoreManager)
    {
        
    }

    public virtual void OnGemGeneration()
    {
        
    }

    public virtual void OnMatch()
    {
        
    }

    public virtual void OnTurnStart()
    {
        
    }

    public virtual void OnLevelStart()
    {
        
    }

    public virtual void OnLevelEnd()
    {
        
    }

    public virtual void OnShopOpen()
    {
        
    }

    public virtual void OnPurchase()
    {
        
    }
}
