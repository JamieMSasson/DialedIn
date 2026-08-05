using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "PurpleYellowPair", menuName = "ScriptableObjects/ModData/PurpleYellowPair", order = 1)]
public class PurpleYellowPair : ModData
{
    public override void OnScore(ScoreManager scoreManager)
    {
        List<int> matchedColors = scoreManager.GetMatchedColors();

        if(matchedColors.Contains(3) && matchedColors.Contains(5))
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
