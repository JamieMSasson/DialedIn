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

    void Start()
    {
        ui_GemScoreDisplay.text = m_curGemScore.ToString();
        ui_PatternScoreDisplay.text = m_curPatternScore.ToString();
        ui_TotalPlayerScoreDisplay.text = m_playerTotalScore.ToString();
        ui_TargetScoreDisplay.text = m_targetScore.ToString();
    }
}
