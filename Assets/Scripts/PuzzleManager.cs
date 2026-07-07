using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class PuzzleManager : MonoBehaviour
{
    // Number of rings in the puzzle
    public int numRings;
    // Number of columns in the puzzle, MUST BE EVEN
    public int numColumns;
    // Prefab for creating new gems
    public GameObject gemPrefab;
    // Int tracking which ring is selected, 0 is the inner most ring
    private int selectedRingNum = 0;
    // int tracking which column is selected, 0 first gem clockwise after Noon and continues clockwise
    private int selectedColumnNum = 0;
    // Transforms for all of the rings, used for rotation and reparenting gems
    private List<Transform> ringTransforms;

    // 2D array of all the Nodes a gem can fall into
    // Ring/Column
    private GemNode[,] gemNodes;

    // Move History and Increment Variables
    public int maxMovesAllowed = 0;
    private int playerTotalMoves = 0;
    public int maxRunsAllowed = 0;
    private int playerTotalRuns = 0;
    public TextMeshProUGUI movesRemainingDisplay;
    public TextMeshProUGUI runsRemainingDisplay;
    
    // 1st - 0 or 1, Ring or Column Move
    // 2nd - Ring or Column Num
    // 3rd - Positive or Negative change in the column
    private List<Vector3> moveHistory = new List<Vector3>();


    // Click/Tap to Move Variables
    bool isMouseDown = false;
    bool isRingMoveLocked = false;
    bool isColumnMoveLocked = false;
    int curColumnOffset = 0;
    Vector2 selectedNodePos = Vector2.zero;
    Vector2 origMousePos = Vector2.zero;

    // Falling Variables for Animation
    bool isCollapsingColumns = false;
    bool isMatchingGems = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateNewGame();
    }

    /// <summary>
    /// Create the GemNodes and populate them with gems for the start of the game
    /// NOTE: There MUST be an even number of collumns, otherwise collumns can't be passed through normally
    /// SIDE NOTE: Might be able to do something with odd collumns, think on this design wise
    /// TODO: Find a way to pass in a set pattern for the GemNodes
    /// </summary>
    private void CreateNewGame()
    {
        // Based on the number of rings and collumns, create a GemNode array
        gemNodes = new GemNode[numRings, numColumns];

        // First, generate all of the nodes in single array so we can assign neighbors later
        // Also instantiate the gems and assign them to the nodes
        GemNode[] tempNodeStorage = new GemNode[numRings * numColumns];
        for (int x = 0; x < numRings * numColumns; x++)
        {
            int gemColor = Random.Range(0, 6);
            GameObject newGem = Instantiate(gemPrefab, Vector2.zero, Quaternion.identity);
            GemNode node = new GemNode(newGem.GetComponent<Gem>(), gemColor);
            tempNodeStorage[x] = node;
        }
        
        // Create a list to hold the ring transforms that will be created
        ringTransforms = new List<Transform>();

        // Now that the GemNodes are created, create the rings they live in, link their neighbors for logic checks, and position the gems
        for (int i = 0; i < numRings; i++)
        {
            // Create a new Ring
            GameObject ring = new GameObject();
            ring.transform.SetParent(this.transform);
            ring.transform.localScale = Vector3.one;
            ring.transform.localPosition = Vector2.zero;
            ringTransforms.Add(ring.transform);

            for (int j = 0; j < numColumns; j++)
            {
                int storageIndex = (i * numColumns) + j;

                gemNodes[i, j] = tempNodeStorage[storageIndex];

                gemNodes[i, j].SetNodePosition(i, j);

                // Position, rotate, and scale the Gem to be in the appropriate location
                gemNodes[i, j].GetGem().transform.SetParent(ring.transform);
                gemNodes[i, j].GetGem().transform.localScale = Vector3.one;
                gemNodes[i, j].GetGem().PositionUpdate(new Vector2(i,j), numColumns);
                gemNodes[i, j].GetGem().SetGemLocation(new Vector2(i, j));

                // If we aren't the inner or outter rings, set our vertical neighbors
                // The innermost and outtermost ring don't have two neighbors vertically, so only set the one they have
                GemNode innerNeighbor = null;
                GemNode outterNeighbor = null;
                if (i > 0)
                {
                    innerNeighbor = tempNodeStorage[storageIndex - numColumns];
                }
                
                if (i < numRings-1)
                {
                    outterNeighbor = tempNodeStorage[storageIndex + numColumns];
                }

                gemNodes[i, j].SetCollumnNeighbors(innerNeighbor, outterNeighbor);

                // Rings will always have a clockwise and counter-clockwise neighbor
                if (j == 0)
                {
                    gemNodes[i, j].SetRingNeighbors(tempNodeStorage[storageIndex + 1], tempNodeStorage[storageIndex + (numColumns - 1)]);
                }
                else if (j == numColumns - 1)
                {
                    gemNodes[i, j].SetRingNeighbors(tempNodeStorage[storageIndex - (numColumns - 1)], tempNodeStorage[storageIndex - 1]);
                }
                else
                {
                    gemNodes[i, j].SetRingNeighbors(tempNodeStorage[storageIndex + 1], tempNodeStorage[storageIndex - 1]);
                }
            }
        }
    }

    /// <summary>
    /// Loop through the nodes of the puzzle and check for all matches
    /// CURRENT: Each gem that is in a match will turn a light teal color
    /// TODO: Delete each gem that is in a match and collapse all other affected gems inward
    /// </summary>
    private IEnumerator CheckForMatches()
    {
        isMatchingGems = true;
        List<GemNode> toClear = new List<GemNode>();
        foreach (GemNode node in gemNodes)
        {
            // node.DisplayAllInformation(); // Debugging tool used to check status of the nodes
            
            // Recursive methods that check if the node is part of a match
            // These return the number of linked colors with neighbors: 1 if alone, 2 if maximum chain of matching colors is 2, etc.
            int collumnMatches = node.CheckCollumnMatches(new List<GemNode>());
            int ringMatches = node.CheckRingMatches(new List<GemNode>());

            // If there are 3 or of the same color in a row, signal a match
            // CURRENT: Highlights the gem in a teal color
            // TODO: Mark each gem for deletion and then loop through them after the Match Check is done to delete them
            if (collumnMatches >= 3)
            {
                //Debug.Log("Collumn Match Found at: " + node.GetNodePosition());
                toClear.Add(node);
            }

            if (ringMatches >= 3)
            {
                //Debug.Log("Ring Match Found at: " + node.GetNodePosition());
                toClear.Add(node);
            }
        }

        // TODO: Delete all Matched Gems and collapse gems inward
        toClear = toClear.Distinct().ToList();
        int[] columnsToCollapse = new int[numColumns];
        foreach(GemNode nodeToClear in toClear)
        {
            int columnNum = nodeToClear.ClearGem();
            columnsToCollapse[columnNum] += 1;
        }

        // Make this a coroutine so the player can see the stuff falling
        if(toClear.Count > 0)
        {
            yield return new WaitForSeconds(1);
            isCollapsingColumns = true;
            StartCoroutine(CollapseColumns(columnsToCollapse));
            while(isCollapsingColumns)
            {
                yield return null;
            }
            StartCoroutine(CheckForMatches());
        }
        else
        {
            // Matching done, reset values for next Run
            isMatchingGems = false;
            playerTotalMoves = 0;
            moveHistory = new List<Vector3>();
            movesRemainingDisplay.text = maxMovesAllowed.ToString();
            playerTotalRuns += 1;
            runsRemainingDisplay.text = (maxRunsAllowed - playerTotalRuns).ToString();
            yield return null;
        }
    }

    /// <summary>
    /// Collapse the columns inward and create new gems as needed
    /// </summary>
    /// <param name="columns">Columns that need to be collapsed</param>
    private IEnumerator CollapseColumns(int[] columns)
    {
        bool moreCollumnsToCollapse = false;
        for(int column = 0; column < columns.Length; column++)
        {
            if(columns[column] != 0)
            {
                for(int ring = 0; ring < numRings; ring++)
                {
                    if(gemNodes[ring,column].GetGem() == null)
                    {
                        gemNodes[ring, column].SetGem(FillNodeGap(new Vector2(ring, column)));
                        
                        if(gemNodes[ring, column].GetGem() != null)
                        {
                            if(ring == numRings - 1)
                            {
                                gemNodes[ring, column].SetGemColor(Random.Range(0,6));
                            }
                            
                            gemNodes[ring, column].GetGem().transform.SetParent(ringTransforms[ring]);
                            gemNodes[ring, column].GetGem().ForcePositionUpdate(new Vector2(ring, column), numColumns);
                        }
                    }
                }
                columns[column] -= 1;
                if(columns[column] > 0)
                {
                    moreCollumnsToCollapse = true;
                }
            }
        }
        
        if(moreCollumnsToCollapse)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(CollapseColumns(columns));
        }
        else
        {
            isCollapsingColumns = false;
            yield return null;
        }
        
    }

    /// <summary>
    /// Recursive method to fill gaps in the puzzle by collapsing inward
    /// </summary>
    /// <param name="nodePos">starting node position to collapse to</param>
    private Gem FillNodeGap(Vector2 nodePos)
    {
        if(nodePos.x == numRings-1)
        {
            // This is the final ring, we need to create a new gem
            GameObject newGem = Instantiate(gemPrefab, Vector2.zero, Quaternion.identity);
            newGem.name = "Gem Created Mid Game";
            return newGem.GetComponent<Gem>();
        }
        else
        {
            if(gemNodes[(int)nodePos.x + 1, (int)nodePos.y].GetGem() == null)
            {
                // We just need to skip this node for now, it will be filled in later
                return null;
            }
            else
            {
                return gemNodes[(int)nodePos.x + 1, (int)nodePos.y].PopGem();
            }
        }
    }

    // Update all gem nodes to have a new color
    private void RefreshAllGemNodes()
    {
        foreach(GemNode node in gemNodes)
        {
            int gemColorID = Random.Range(0,6);
            node.SetGemColor(gemColorID);
        }
    }

    // Rotate the selected ring one tick clockwise or counter-clockwise
    // Used in loops to rotate multiple times at once
    // NOTE: The transform does not ACTUALLY rotate, instead the GemNodes swap their references and the gems update their display accordingly
    private void RotateRing(bool clockwise)
    {
        GemNode startNode = gemNodes[selectedRingNum,0];
        Gem gemReplaced = startNode.GetGem();

        if(clockwise)
        {
            // Start from 1 because we already have the gem to replace the second point in the ring
            for(int i = 1; i <= numColumns; i++)
            {
                // On the final gem, we just need to update the Gem Node, no need to store a reference
                if(i == numColumns)
                {
                    gemReplaced.TwistGem(clockwise, numColumns);
                    gemNodes[selectedRingNum, 0].SetGem(gemReplaced);
                }
                else
                {
                    gemReplaced.TwistGem(clockwise, numColumns);
                    gemReplaced = gemNodes[selectedRingNum, i].SetGem(gemReplaced);
                }
            }
        }
        else
        {
            // If we are rotating counter-clockwise, we don't need a specific outlier
            for(int i = numColumns-1; i >= 0; i--)
            {
                gemReplaced = gemNodes[selectedRingNum, i].SetGem(gemReplaced);
                gemReplaced.TwistGem(clockwise, numColumns);
            }
        }
    }

    /// <summary>
    /// Shift gems along the selected column, these shift through the center so the process is more complex
    /// NOTE: The columns don't actually move, only the gems themselves update their position and parent with the GemNodes swapping references
    /// </summary>
    /// <param name="outward">Are we shifting outward or inward in reference to the selected GemNode</param>
    private void ShiftCollumn(bool outward, bool isUndoing = false)
    {
        GemNode startNode = gemNodes[0, selectedColumnNum];
        int mirroredColumnNum = selectedColumnNum + numColumns/2;
        Gem gemReplaced = startNode.GetGem();

        if(outward)
        {
            gemReplaced.ShiftGem(outward, numColumns);
            for(int i = 1; i <= numRings*2; i++)
            {
                if(i >= numRings)
                {
                    if(i == numRings*2)
                    {
                        gemReplaced.transform.SetParent(ringTransforms[0]);
                        gemNodes[0, selectedColumnNum].SetGem(gemReplaced);
                    }
                    else 
                    {
                        gemReplaced.transform.SetParent(ringTransforms[((numRings*2)-1) - i]);
                        gemReplaced = gemNodes[((numRings*2)-1) - i, mirroredColumnNum].SetGem(gemReplaced);
                        gemReplaced.ShiftGem(false, numColumns);
                    }
                    
                }
                else
                {
                    gemReplaced.transform.SetParent(ringTransforms[i]);
                    gemReplaced = gemNodes[i, selectedColumnNum].SetGem(gemReplaced);
                    gemReplaced.ShiftGem(true, numColumns);
                }
            }
        }
        else
        {
            for(int i = 0; i < numRings*2; i++)
            {
                if(i >= numRings)
                {
                    gemReplaced.transform.SetParent(ringTransforms[((numRings*2)-1) - i]);
                    gemReplaced = gemNodes[((numRings*2)-1) - i, selectedColumnNum].SetGem(gemReplaced);
                    gemReplaced.ShiftGem(false, numColumns);
                }
                else
                {
                    gemReplaced.transform.SetParent(ringTransforms[i]);
                    gemReplaced = gemNodes[i, mirroredColumnNum].SetGem(gemReplaced);
                    gemReplaced.ShiftGem(true, numColumns);
                }
            }
        }

        if(!isUndoing)
        {
            if(moveHistory.Count == playerTotalMoves)
            {
                if(outward)
                {
                    moveHistory.Add(new Vector3(1, selectedColumnNum, 1));
                }
                else
                {
                    moveHistory.Add(new Vector3(1, selectedColumnNum, -1));
                }
            }
            else
            {
                if(outward)
                {
                    moveHistory[playerTotalMoves] = new Vector3(1, selectedColumnNum, moveHistory[playerTotalMoves].z + 1);
                }
                else
                {
                    moveHistory[playerTotalMoves] = new Vector3(1, selectedColumnNum, moveHistory[playerTotalMoves].z - 1);
                }
            }
        }
    }

    /// <summary>
    /// Function called when the player clicks anywhere on the screen
    /// CURRENT: Selects the nearest gem
    /// TODO: Make this only select a gem if the player clicks within the puzzle, as we'll need other things to tap on
    /// </summary>
    void OnClick()
    {
        if(playerTotalMoves == maxMovesAllowed)
        {
            Debug.Log("No More Moves Available!");
            return;
        }
        else if(playerTotalRuns == maxRunsAllowed)
        {
            Debug.Log("No More Runs Available!");
            return;
        }
        else if(isMatchingGems)
        {
            Debug.Log("Matching Gems! Wait until matching finished...");
            return;
        }

        Vector2 mousePos = Input.mousePosition;
        float closestDist = 100000;
        Vector2 closestPoint = Vector2.zero;

        // Determine which Gem is the closest to this mouse-click
        foreach(GemNode node in gemNodes)
        {
            float nodeDist = Vector2.Distance(mousePos, node.GetGem().transform.position);
            if(closestDist > nodeDist)
            {
                closestDist = nodeDist;
                closestPoint = node.GetNodePosition();
            }
        }

        // Assign the local variables
        selectedNodePos = closestPoint;
        origMousePos = mousePos;
        isMouseDown = true;

        // Update the selected ring and column to where the player clicks, used for Shifting and Rotating
        selectedRingNum = (int)closestPoint.x;
        if(closestPoint.y >= numColumns/2)
        {
            selectedColumnNum = (int)closestPoint.y - numColumns/2;
        }
        else
        {
            selectedColumnNum = (int)closestPoint.y;
        }
    }

    /// <summary>
    /// When the player drags their finger or mouse, the puzzle will react by rotating the selected ring or shifting the column
    /// </summary>
    void OnDrag()
    {
        // These variables and calculations determine the grace-area for a column shift action
        float distanceFromColumnNeighbor = 0;
        float angleDifferenceColumn = 0;
        if(selectedNodePos.x != numRings-1)
        {
            distanceFromColumnNeighbor = Vector2.Distance(gemNodes[(int)selectedNodePos.x, (int)selectedNodePos.y].GetGem().transform.position, gemNodes[(int)selectedNodePos.x+1, (int)selectedNodePos.y].GetGem().transform.position);
            Vector2 vectorBetweenNeighbor = (Vector2)gemNodes[(int)selectedNodePos.x+1, (int)selectedNodePos.y].GetGem().transform.position - (Vector2)gemNodes[(int)selectedNodePos.x, (int)selectedNodePos.y].GetGem().transform.position;
            angleDifferenceColumn = Vector2.Angle(vectorBetweenNeighbor, (Vector2)Input.mousePosition - origMousePos);
        }
        else
        {
            distanceFromColumnNeighbor = Vector2.Distance(gemNodes[(int)selectedNodePos.x, (int)selectedNodePos.y].GetGem().transform.position, gemNodes[(int)selectedNodePos.x-1, (int)selectedNodePos.y].GetGem().transform.position);
            Vector2 vectorBetweenNeighbor = (Vector2)gemNodes[(int)selectedNodePos.x, (int)selectedNodePos.y].GetGem().transform.position - (Vector2)gemNodes[(int)selectedNodePos.x-1, (int)selectedNodePos.y].GetGem().transform.position;
            angleDifferenceColumn = Vector2.Angle(vectorBetweenNeighbor, (Vector2)Input.mousePosition - origMousePos);
        }
        
        // If the player is not currently locked into a type of movement, determine if one has met the condition to occur
        if(!isRingMoveLocked && !isColumnMoveLocked)
        {
            if(Vector2.Angle(origMousePos - (Vector2)transform.position, (Vector2)Input.mousePosition - (Vector2)transform.position) > 360/numColumns/4)
            {
                isRingMoveLocked = true;
                isColumnMoveLocked = false;
            }
            // NOTE: Make the Column Lock variables static
            else if(Vector2.Distance(origMousePos, (Vector2)Input.mousePosition) > 0.75 * distanceFromColumnNeighbor && (angleDifferenceColumn < 15 || angleDifferenceColumn > 165))
            {
                isColumnMoveLocked = true;
                isRingMoveLocked = false;
            }
        }

        // If locked into a ring move, update the transform of the selected ring to rotate to the player's mouse/finger angle offset
        if(isRingMoveLocked)
        {
            ringTransforms[(int)selectedNodePos.x].rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(origMousePos - (Vector2)transform.position, (Vector2)Input.mousePosition - (Vector2)transform.position));
        }

        // If locked into a Column Shift, shift the gems up or down the column
        // Based on the distance and angle the player is dragging, determine how many columns the player has shifted through
        if(isColumnMoveLocked)
        {
            int updatedColumnOffset = Mathf.RoundToInt(Vector2.Distance(origMousePos, (Vector2)Input.mousePosition)/distanceFromColumnNeighbor);

            // If the angle the player is dragging is Above 90, we are dragging inward, so invert the column offset
            if(angleDifferenceColumn > 90)
            {
                updatedColumnOffset *= -1;
            }

            if(updatedColumnOffset != curColumnOffset)
            {
                int numShifts = curColumnOffset - updatedColumnOffset;
                bool isOutward = numShifts < 0;

                if(selectedNodePos.y >= numColumns/2)
                {
                    isOutward = !isOutward;
                }

                numShifts = Mathf.Abs(numShifts);

                for(int i = 0; i < numShifts; i++)
                {
                    ShiftCollumn(isOutward);
                }
                curColumnOffset = updatedColumnOffset;
            }

        }
    }

    private void UndoPreviousMove()
    {
        if(playerTotalMoves > 0)
        {
            // Decrement the Player Moves counter and remove the previous move from the move history
            playerTotalMoves -= 1;
            Vector3 toUndo = moveHistory[playerTotalMoves];
            moveHistory.RemoveAt(playerTotalMoves);
            movesRemainingDisplay.text = (maxMovesAllowed - playerTotalMoves).ToString();
            
            // Undo puzzle movements
            if(toUndo.x == 0)
            {
                // Ring Move
                selectedRingNum = (int)toUndo.y;
                for(int i = 0; i < toUndo.z; i++)
                {
                    RotateRing(true);
                }
            }
            else
            {
                // Column Move
                selectedColumnNum = (int)toUndo.y;
                for(int i = 0; i < Mathf.Abs(toUndo.z); i++)
                {
                    if(toUndo.z < 0)
                    {
                        ShiftCollumn(true, true);
                    }
                    else
                    {
                        ShiftCollumn(false, true);
                    }
                }
            }

        }
        else
        {
            Debug.Log("Nothing to Undo!");
        }
    }

    public void RunButtonClicked()
    {
        StartCoroutine(CheckForMatches());
    }

    public void UndoButtonClicked()
    {
        UndoPreviousMove();
    }

    // Update is called once per frame
    void Update()
    {
        // Refresh the Puzzle
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RefreshAllGemNodes();
        }

        // When the player left-clicks, call the click function
        // TODO: Hook up input system to make this work for taps too
        if(Input.GetMouseButtonDown(0))
        {
            OnClick();
        }

        // When the player releases the mouse button, do the logic for a Ring Rotation, as the transform is just updating visually
        if(Input.GetMouseButtonUp(0))
        {
            if(isRingMoveLocked)
            {
                // Determine how far around the circle the player has rotated and call the appropriate amount of Rotate Ring functions to align the gems
                int ringOffset = Mathf.RoundToInt(ringTransforms[(int)selectedNodePos.x].eulerAngles.z / (360/numColumns));
                ringTransforms[(int)selectedNodePos.x].rotation = Quaternion.Euler(0,0,0);
                if(ringOffset != 0)
                {
                    for(int i = 0; i < Mathf.Abs(ringOffset); i++)
                    {
                        RotateRing(false);
                    }
                    moveHistory.Add(new Vector3(0, selectedRingNum, ringOffset));
                    playerTotalMoves += 1;
                    movesRemainingDisplay.text = (maxMovesAllowed - playerTotalMoves).ToString();
                }
            }
            else if(isColumnMoveLocked)
            {
                if(moveHistory.Count == playerTotalMoves+1)
                {
                    if(moveHistory[playerTotalMoves].z != 0)
                    {
                        playerTotalMoves += 1;
                        movesRemainingDisplay.text = (maxMovesAllowed - playerTotalMoves).ToString();
                    }
                    else
                    {
                        moveHistory.RemoveAt(playerTotalMoves);
                    }
                }
            }

            // Reset Click-Drag variables
            selectedNodePos = Vector2.zero;
            origMousePos = Vector2.zero;
            curColumnOffset = 0;
            isMouseDown = false;
            isRingMoveLocked = false;
            isColumnMoveLocked = false;
        }

        // Only call the OnDrag function if the player is holding mouse/tap
        if(isMouseDown)
        {
            OnDrag();
        }
    }
}
