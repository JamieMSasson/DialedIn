using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "RedOrangePair", menuName = "ScriptableObjects/ModData/RedOrangePair", order = 1)]
public class RedOrangePair : ModData
{
    public override void OnScore(ScoreManager scoreManager)
    {
        List<int> matchedColors = scoreManager.GetMatchedColors();

        if(matchedColors.Contains(4) && matchedColors.Contains(2))
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
