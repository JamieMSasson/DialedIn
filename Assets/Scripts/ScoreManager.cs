using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    private float m_chainMultiplier = 0;

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

        m_curPatternScore += Mathf.FloorToInt(patternLookup * m_chainMultiplier);
        ui_PatternScoreDisplay.text = m_curPatternScore.ToString();
    }

    public void ScoreMatches(List<List<GemNode>> matches, float chainScale = 1)
    {
        m_chainMultiplier = chainScale;
        List<List<List<GemNode>>> specialMatches = new List<List<List<GemNode>>>();
        bool ringRectFound = false;
        bool columnRectFound = false;

        for(int i = 0; i < matches.Count; i++)
        {
            int neighborMatchesInner = 0;
            int neighborMatchesOuter = 0;
            int neighborMatchesClockwise = 0;
            int neighborMatchesCounter = 0;
            List<List<GemNode>> involvedMatches = new List<List<GemNode>>();
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
                        involvedMatches.Add(matches[j]);
                    }
                    else
                    {
                        if(matches[j].Contains(node.m_innerNeighbor) && node.GetGem().GetColorID() == node.m_innerNeighbor.GetGem().GetColorID())
                        {
                            neighborMatchesInner++;
                        }
                        else if(matches[j].Contains(node.m_outerNeighbor) && node.GetGem().GetColorID() == node.m_outerNeighbor.GetGem().GetColorID())
                        {
                            neighborMatchesOuter++;
                        }
                        else if(matches[j].Contains(node.m_clockwiseNeighbor) && node.GetGem().GetColorID() == node.m_clockwiseNeighbor.GetGem().GetColorID())
                        {
                            neighborMatchesClockwise++;
                        }
                        else if(matches[j].Contains(node.m_counterClockwiseNeighbor) && node.GetGem().GetColorID() == node.m_counterClockwiseNeighbor.GetGem().GetColorID())
                        {
                            neighborMatchesCounter++;
                        }
                    }
                }
            }

            if(involvedMatches.Count > 0)
            {
                bool existingGroup = false;
                involvedMatches.Add(matches[i]);
                foreach(List<List<GemNode>> group in specialMatches)
                {
                    foreach(List<GemNode> foundMatch in involvedMatches)
                    {
                        if(group.Contains(foundMatch))
                        {
                            group.AddRange(involvedMatches);
                            existingGroup = true;
                            break;
                        }
                    }
                    
                }
                if(existingGroup == false)
                {
                    specialMatches.Add(involvedMatches);
                }
                
            }
            else
            {
                // Check if this is a ring match
                if(matches[i][0].GetNodePosition().x == matches[i][1].GetNodePosition().x)
                {
                    // If this wasn't part of a 'special' match, use the neighbor checks to see if it is a small rect
                    if(neighborMatchesInner == matches[i].Count || neighborMatchesOuter == matches[i].Count)
                    {
                        // Toggle bool to avoid double counting the same rect
                        if(ringRectFound)
                        {
                            ringRectFound = false;
                        }
                        else
                        {
                            AddPatternScore(12, matches[i].Count);
                            ringRectFound = true;
                        }
                    }
                    else
                    {
                        AddPatternScore(matches[i].Count-3, matches[i].Count);
                        matches.RemoveAt(i);
                        i--;
                    }
                }
                // Otherwise it has to be a Column match
                else
                {
                    // If this wasn't part of a 'special' match, use the neighbor checks to see if it is a small rect
                    if(neighborMatchesClockwise == matches[i].Count || neighborMatchesCounter == matches[i].Count)
                    {
                        // Toggle bool to avoid double counting the same rect
                        if(columnRectFound)
                        {
                            columnRectFound = false;
                        }
                        else
                        {
                            AddPatternScore(12, matches[i].Count);
                            columnRectFound = true;
                        }
                    }
                    else
                    {
                        AddPatternScore(matches[i].Count+3, matches[i].Count);
                        matches.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        if(specialMatches.Count > 0)
        {
            for(int i = 0; i < specialMatches.Count; i++)
            {
                specialMatches[i] = specialMatches[i].Distinct().ToList();
                int totalGems = 0;
                List<int> uniqueX = new List<int>();
                List<int> uniqueY = new List<int>();

                foreach(List<GemNode> match in specialMatches[i])
                {
                    totalGems += match.Count;
                    foreach(GemNode node in match)
                    {
                        if(!uniqueX.Contains((int)node.GetNodePosition().x))
                        {
                            uniqueX.Add((int)node.GetNodePosition().x);
                        }
                        if(!uniqueY.Contains((int)node.GetNodePosition().y))
                        {
                            uniqueY.Add((int)node.GetNodePosition().y);
                        }
                    }
                }

                // If the count is 2 and it is special, this is a T match
                if(specialMatches[i].Count == 2)
                {
                    AddPatternScore(14, totalGems);
                }
                // Otherwise, we'll use the # of unique X and Y points to determine the type
                else
                {
                    // If the number of unique Xs and Ys add up to the number of matches, it must be a rectangle
                    if(uniqueX.Count + uniqueY.Count == specialMatches[i].Count)
                    {
                        // If they are the same amount, it is a square
                        if(uniqueX.Count == uniqueY.Count)
                        {
                            AddPatternScore(13, totalGems);
                        }
                        // Otherwise it is a rect
                        else
                        {
                            AddPatternScore(12, totalGems);
                        }
                    }
                    // Otherwise it is a hammer
                    else
                    {
                        AddPatternScore(15, totalGems);
                    }
                }
            }
        }
    }

    public void TotalCurrentScore()
    {
        m_playerTotalScore = m_curGemScore * m_curPatternScore;
        m_curGemScore = 0;
        m_curPatternScore = 0;
        m_chainMultiplier = 1;
        ui_TotalPlayerScoreDisplay.text = m_playerTotalScore.ToString();
        ui_GemScoreDisplay.text = m_curGemScore.ToString();
        ui_PatternScoreDisplay.text = m_curPatternScore.ToString();
    }
}
