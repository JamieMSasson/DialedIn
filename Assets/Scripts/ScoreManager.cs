using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Display Variables
    [SerializeField] private TextMeshProUGUI ui_GemScoreDisplay;
    [SerializeField] private TextMeshProUGUI ui_PatternScoreDisplay;
    [SerializeField] private TextMeshProUGUI ui_TotalPlayerScoreDisplay;
    [SerializeField] private TextMeshProUGUI ui_TargetScoreDisplay;

    // Score Variables
    private int m_curGemScore = 0;
    private int m_curPatternScore = 0;
    private int m_playerTotalScore = 0;
    private int m_targetScore = 1000;

    // Gem and Pattern Score Dictionary
    private Dictionary<int, int> m_dict_gemScores;
    private Dictionary<int, int> m_dict_patternScores;

    // Gem and Pattern Text Data Assets
    [SerializeField] private TextAsset m_data_patternScores;

    /// <summary>
    /// Load in the base score values for Gems and Patterns
    /// TODO: Make pattern scores read a CSV or some other file for ease of editing
    /// </summary>
    private void InitializeDictionaries()
    {
        m_dict_gemScores = new Dictionary<int, int>();
        m_dict_patternScores = new Dictionary<int, int>();

        for(int i = 0; i < 6; i++)
        {
            m_dict_gemScores.Add(i, 1);
        }

        string[] patternDataRows = m_data_patternScores.text.Split(new char[] {'\n'});
        for(int i = 1; i < patternDataRows.Length; i++)
        {
            // If the row is empty, skip
            if (string.IsNullOrWhiteSpace(patternDataRows[i]))
            {
                Debug.LogError("A Row Was Empty in Pattern Data! Row Num: " + i);
                continue;
            }

            string[] patternValues = patternDataRows[i].Split(',');
            m_dict_patternScores.Add(i-1, int.Parse(patternValues[1].Trim()));
        }
    }

    void Start()
    {
        ui_GemScoreDisplay.text = m_curGemScore.ToString();
        ui_PatternScoreDisplay.text = m_curPatternScore.ToString();
        ui_TotalPlayerScoreDisplay.text = m_playerTotalScore.ToString();
        ui_TargetScoreDisplay.text = m_targetScore.ToString();
        InitializeDictionaries();
    }

    /// <summary>
    /// Add gem score based on gem color and number of gems
    /// </summary>
    /// <param name="gemColorID">Color ID</param>
    /// <param name="numGems">Number of Gems</param>
    public void AddGemScore(int gemColorID)
    {
        int perGemScore = 0;
        m_dict_gemScores.TryGetValue(gemColorID, out perGemScore);
        m_curGemScore += perGemScore;
        ui_GemScoreDisplay.text = m_curGemScore.ToString();
    }

    /// <summary>
    /// Add pattern score based on specified pattern ID
    /// TODO: Scale with num gems in the pattern for special patterns
    /// </summary>
    /// <param name="patternID">Pattern ID: Row 3-8, Column 3-8, Special Patterns</param>
    /// <param name="numGems">Number of Gems in this pattern, used for Special Pattern Scaling</param>
    private void AddPatternScore(int patternID, int numGems)
    {
        int patternLookup = 0;
        m_dict_patternScores.TryGetValue(patternID, out patternLookup);

        // TODO: Add Scaling for more gems in special patterns

        m_curPatternScore += patternLookup;
        ui_PatternScoreDisplay.text = m_curPatternScore.ToString();
    }

    public void ScorePatterns(List<List<GemNode>> matches)
    {
        for(int i = 0; i < matches.Count; i++)
        {
            int specialPatternCount = 0;
            foreach(GemNode node in matches[i])
            {
                for(int j = 0; j < matches.Count; j++)
                {
                    if(i == j)
                    {
                        continue;
                    }

                    // Determine if this is a special pattern
                    if(matches[j].Contains(node))
                    {
                        specialPatternCount += 1;
                    }
                }
            }

            if(specialPatternCount > 0)
            {
                Debug.Log("Special Pattern Detected: " + specialPatternCount);
            }
            else
            {
                // Check if this is a ring match
                if(matches[i][0].GetNodePosition().x == matches[i][1].GetNodePosition().x)
                {
                    AddPatternScore(matches[i].Count-3, matches[i].Count);
                }
                // Otherwise it has to be a Column match
                else
                {
                    AddPatternScore(matches[i].Count+3, matches[i].Count);
                }
            }
        }
    }
}
