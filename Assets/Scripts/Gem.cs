using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gem : MonoBehaviour
{
    [SerializeField] private Sprite m_curSprite;
    [SerializeField] private Image m_myImg;

    // All positions are variations on the Zero position, this defines the (X,Y) positions of Zero for each ring in an 8 column puzzle
    [SerializeField] private List<Vector2> positionZeroEightColumnRings;
    [SerializeField] private List<Vector2> widthHeightEightColumn;
    [SerializeField] private List<Vector2> pivotEightColumn;
    [SerializeField] private List<Sprite> ringSpritesEightColumn;
   
    // Same as above, but for 6 column puzzle
    [SerializeField] private List<Vector2> positionZeroSixColumnRings;
    
    // 4 column puzzle
    [SerializeField] private List<Vector2> positionZeroFourColumnRings;

    // Color ID of this gem, used for matching
    private int m_colorID;
    // Ring/Column position of this gem, used for visual updates
    private Vector2 m_gemLocation;

    public bool markedAsChecked = false;

    /// <summary>
    /// Constructor for creating a new Gem
    /// </summary>
    /// <param name="gemLoc">Ring/Column position of this gem</param>
    /// <param name="img">Reference to the image this gem will update</param>
    /// <param name="color">Color ID of this gem</param>
    /// <param name="spr">Sprite that will be assigned </param>
    public Gem(Vector2 gemLoc, Image img, int color, Sprite spr)
    {
        m_colorID = color;
        m_curSprite = spr;
        m_myImg = img;
        m_gemLocation = gemLoc;
    }

    public int GetColorID()
    {
        return m_colorID;
    }

    public void SetColorID(int colorID)
    {
        m_colorID = colorID;
        UpdateGemColor();
    }

    public void SetGemLocation(Vector2 gemPos)
    {
        m_gemLocation = gemPos;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gemPos">What node is this gem slotting into</param>
    /// <param name="numColumns">How many columns in this puzzle, used for rotation and positional math</param>
    /// <param name="rotationalOffset">During a game, when a gem is created and falls into a ring that has already been rotated, we'll need to know the ring's rotation</param>
    public void PositionUpdate(Vector2 gemPos, int numColumns, float rotationalOffset = 0)
    {
        int ringPositionalOffset = Mathf.FloorToInt(rotationalOffset / (360/numColumns));

        // Set Rotation, only based on which column this is gem is part of
        transform.rotation = Quaternion.Euler(0, 0, (gemPos.y+ringPositionalOffset)*(-360/numColumns));
        
        // Set Position
        Vector2 posToSet;
        switch(numColumns)
        {
            case 4:
                posToSet = positionZeroFourColumnRings[(int)gemPos.x];
                break;
            case 6:
                posToSet = positionZeroSixColumnRings[(int)gemPos.x];
                break;
            case 8:
                posToSet = positionZeroEightColumnRings[(int)gemPos.x];
                break;
            default:
                posToSet = Vector2.zero;
                break;
        }

        switch(gemPos.y)
        {
            case 0:
                // Do Nothing
                break;
            case 1:
                posToSet = new Vector2(posToSet.y, posToSet.x);
                break;
            case 2:
                posToSet = new Vector2(posToSet.y, -posToSet.x);
                break;
            case 3:
                posToSet = new Vector2(posToSet.x, -posToSet.y);
                break;
            case 4:
                posToSet = new Vector2(-posToSet.x, -posToSet.y);
                break;
            case 5:
                posToSet = new Vector2(-posToSet.y, -posToSet.x);
                break;
            case 6:
                posToSet = new Vector2(-posToSet.y, posToSet.x);
                break;
            case 7:
                posToSet = new Vector2(-posToSet.x, posToSet.y);
                break; 
        }

        transform.localPosition = posToSet;

        // Set Sprite, Pivot, and Scale
        m_curSprite = ringSpritesEightColumn[(int)gemPos.x];
        m_myImg.sprite = m_curSprite;
        (transform as RectTransform).pivot = pivotEightColumn[(int)gemPos.x];
        (transform as RectTransform).sizeDelta = widthHeightEightColumn[(int)gemPos.x];
    }

    public void UpdateGemColor(int updateColor = -1)
    {
        if(updateColor == -1)
        {
            updateColor = m_colorID;
        }

        switch (updateColor)
        {
            case 0:
                // Blue
                m_myImg.color = new Color(20f / 255f, 36f / 255f, 217f / 255f);
                break;
            case 1:
                // Green
                m_myImg.color = new Color(20f / 255f, 217f / 255f, 29f / 255f);
                break;
            case 2:
                // Red
                m_myImg.color = new Color(217f / 255f, 39f / 255f, 20f / 255f);
                break;
            case 3:
                // Purple
                m_myImg.color = new Color(187f / 255f, 20f / 255f, 217f / 255f);
                break;
            case 4:
                // Orange
                m_myImg.color = new Color(217f / 255f, 128f / 255f, 20f / 255f);
                break;
            case 5:
                // Yellow
                m_myImg.color = new Color(240f / 255f, 236f / 255f, 24f / 255f);
                break;
            case 6:
                m_myImg.color = new Color(204f / 255f, 204f / 255f, 255f / 255f);
                break;
        }
    }

    public void ShiftGem(bool isShiftingUp, int numColumns)
    {
        if(isShiftingUp)
        {
            if(m_gemLocation.x == numColumns-1)
            {
                if(m_gemLocation.y >= numColumns/2)
                {
                    m_gemLocation = new Vector2(m_gemLocation.x, m_gemLocation.y-numColumns/2);
                }
                else
                {
                    m_gemLocation = new Vector2(m_gemLocation.x, m_gemLocation.y+numColumns/2);
                }
            }
            else
            {
                m_gemLocation = new Vector2(m_gemLocation.x+1, m_gemLocation.y);
            }
        }
        else
        {
            if(m_gemLocation.x == 0)
            {
                if(m_gemLocation.y >= numColumns/2)
                {
                    m_gemLocation = new Vector2(m_gemLocation.x, m_gemLocation.y-numColumns/2);
                }
                else
                {
                    m_gemLocation = new Vector2(m_gemLocation.x, m_gemLocation.y+numColumns/2);
                }
            }
            else
            {
                m_gemLocation = new Vector2(m_gemLocation.x-1, m_gemLocation.y);
            }
        }

        PositionUpdate(m_gemLocation, numColumns);
    }

    public void TwistGem(bool clockwise, int numColumns)
    {
        if(clockwise)
        {
            if(m_gemLocation.y == numColumns-1)
            {
                m_gemLocation = new Vector2(m_gemLocation.x, 0);
            }
            else
            {
                m_gemLocation = new Vector2(m_gemLocation.x, m_gemLocation.y+1);
            }
        }
        else
        {
            if(m_gemLocation.y == 0)
            {
                m_gemLocation = new Vector2(m_gemLocation.x, numColumns-1);
            }
            else
            {
                m_gemLocation = new Vector2(m_gemLocation.x, m_gemLocation.y-1);
            }
        }

        PositionUpdate(m_gemLocation, numColumns);
    }

    public void ForcePositionUpdate(Vector2 newPos, int numColumns)
    {
        m_gemLocation = newPos;
        transform.localScale = Vector3.one;
        PositionUpdate(m_gemLocation, numColumns);
    }
}
