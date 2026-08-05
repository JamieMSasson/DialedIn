using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "RedBluePair", menuName = "ScriptableObjects/ModData/RedBluePair", order = 1)]
public class RedBluePair : ModData
{
    public override void OnScore(ScoreManager scoreManager)
    {
        List<int> matchedColors = scoreManager.GetMatchedColors();

        if(matchedColors.Contains(0) && matchedColors.Contains(2))
        {
            Debug.Log("Condition Met!");
            scoreManager.AddModScore(0, 10, 1);
        }
        else
        {
            Debug.Log("Condition not met...");
        }
    }
}
