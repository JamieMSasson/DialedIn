using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GemNode
{
    // The node's Ring/Column position
    Vector2 m_nodePosition;

    // References to the neighboring GemNodes, used for matching
    GemNode m_clockwiseNeighbor;
    GemNode m_counterClockwiseNeighbor;
    GemNode m_outerNeighbor;
    GemNode m_innerNeighbor;

    // The gem that is currently on this node
    Gem m_curGem;

    /// <summary>
    /// Debug function that displays all non-gem information on this node
    /// </summary>
    public void DisplayAllInformation()
    {
        if(m_innerNeighbor != null)
        {
            if(m_outerNeighbor != null)
            {
                Debug.Log("My Position: " + m_nodePosition + "\nMy Inner Neighbor: " + m_innerNeighbor.GetNodePosition() + "\nMy Outer Neighbor: " + m_outerNeighbor.GetNodePosition());
            }
            else
            {
                Debug.Log("My Position: " + m_nodePosition + "\nMy Inner Neighbor: " + m_innerNeighbor.GetNodePosition() + "\nI Have No Outer Neighbor: ");
            }
        }
        else
        {
            if(m_outerNeighbor != null)
            {
                Debug.Log("My Position: " + m_nodePosition + "\nI Have No Inner Neighbor\nMy Outer Neighbor: " + m_outerNeighbor.GetNodePosition());
            }
            else
            {
                Debug.Log("My Position: " + m_nodePosition + "\nI Have No Neighbors, I Must Scream");
            }
        }
    }

    /// <summary>
    /// Constructor to create a new GemNode
    /// </summary>
    /// <param name="curGem">Reference to the gem on this node</param>
    /// <param name="gemColor">Color of the gem on this node</param>
    public GemNode(Gem curGem, int gemColor)
    {
        m_curGem = curGem;
        m_curGem.SetColorID(gemColor);
    }

    /// <summary>
    /// Sets the color of the gem on this node
    /// </summary>
    /// <param name="newColorID">New color for the gem</param>
    /// <returns>The previous color of the gem on this node</returns>
    public int SetGemColor(int newColorID)
    {
        int oldColorID = m_curGem.GetColorID();
        m_curGem.SetColorID(newColorID);
        return oldColorID;
        
    }

    /// <summary>
    /// Sets the gem that is on this node
    /// </summary>
    /// <param name="newGem">Reference to the new gem</param>
    /// <returns>Gem that was previously on this node</returns>
    public Gem SetGem(Gem newGem)
    {
        Gem oldGem = m_curGem;
        m_curGem = newGem;
        return oldGem;
    }

    /// <summary>
    /// Pop the gem off this node, used for collapsing the puzzle
    /// </summary>
    /// <returns></returns>
    public Gem PopGem()
    {
        Gem poppedGem = m_curGem;
        m_curGem = null;
        return poppedGem;
    }

    /// <summary>
    /// Get the gem that is on this node
    /// </summary>
    /// <returns></returns>
    public Gem GetGem()
    {
        return m_curGem;
    }

    /// <summary>
    /// Sets the ring neighbors for this GemNode
    /// </summary>
    /// <param name="clockwise"></param>
    /// <param name="counter"></param>
    public void SetRingNeighbors(GemNode clockwise, GemNode counter)
    {
        m_clockwiseNeighbor = clockwise;
        m_counterClockwiseNeighbor = counter;
    }

    /// <summary>
    /// Sets the column neighbors for this GemNode
    /// </summary>
    /// <param name="inner"></param>
    /// <param name="outter"></param>
    public void SetCollumnNeighbors(GemNode inner, GemNode outter)
    {
        m_innerNeighbor = inner;
        m_outerNeighbor = outter;
    }

    /// <summary>
    /// Set the Ring/Column position of this GemNode
    /// </summary>
    /// <param name="x">Ring that this node is in</param>
    /// <param name="y">Column this node is in</param>
    public void SetNodePosition(int x, int y)
    {
        m_nodePosition = new Vector2(x, y);
    }

    /// <summary>
    /// Returns the Ring/Column position
    /// </summary>
    /// <returns></returns>
    public Vector2 GetNodePosition()
    {
        return m_nodePosition;
    }

    /// <summary>
    /// Changes the color of the gem on this node to the 'Is Match' color
    /// TODO: Delete the gem on this node and remove the reference to it
    /// </summary>
    /// <param name="gemRecolor"></param>
    public void SignalMatch(int gemRecolor)
    {
        m_curGem.UpdateGemColor(gemRecolor);
    }

    /// <summary>
    /// Recursive method that checks clockwise and counter-clockwise neighbors to determine if the gems are the same color
    /// </summary>
    /// <param name="exploredNodes">All nodes that were previously explored in the recursion</param>
    /// <param name="linkedNodes">How many nodes are currently linked by color in this chain, defaults to 0 for starting node because no chain is created yet</param>
    /// <param name="colorID">Color ID to check, defaults to -1 for the starting node to know that it should use its color</param>
    /// <returns></returns>
    public int CheckRingMatches(List<GemNode> exploredNodes, int linkedNodes = 0, int colorID = -1)
    {
        // If we are the starting node, the ColorID will be -1
        // If it is, set this as the starting node and update the ColorID to the gem on this node's color
        bool isStartingNode = false;
        if(colorID == -1)
        {
            colorID = m_curGem.GetColorID();
            isStartingNode = true;
        }

        // If the colorID matches, add to the linked nodes and explored nodes, then recur through neighbors
        if(m_curGem.GetColorID() == colorID)
        {
            linkedNodes += 1;
            exploredNodes.Add(this);

            // Make sure not to double-check nodes that were already explored, prevents infinite loops
            bool clockwiseExplored = false;
            bool counterExplored = false;
            foreach(GemNode node in exploredNodes)
            {
                if(node == m_clockwiseNeighbor)
                {
                    clockwiseExplored = true;
                }
                else if(node == m_counterClockwiseNeighbor)
                {
                    counterExplored = true;
                }
            }

            // Recur clockwise and counter-clockwise
            int clockwiseMatches = 0;
            int counterMatches = 0;
            if(!clockwiseExplored)
            {
                clockwiseMatches = m_clockwiseNeighbor.CheckRingMatches(exploredNodes, linkedNodes, colorID);
            }

            if(!counterExplored)
            {
                counterMatches = m_counterClockwiseNeighbor.CheckRingMatches(exploredNodes, linkedNodes, colorID);
            }

            // If we are the starting node, the linkedNodes value will be off by one, so correct for this
            // NOTE: It is counted twice because it goes both clockwise and counter-clockwise, so it's addition gets counted twice
            if(isStartingNode)
            {
                linkedNodes = (clockwiseMatches + counterMatches) - 1;
            }
            else
            {
                linkedNodes = clockwiseMatches + counterMatches;
            }
            
            return linkedNodes;
        }
        else
        {
            return linkedNodes;
        }
    }

    /// <summary>
    /// Recursive method that checks inner and outer neighbors to determine if the gems are the same color
    /// </summary>
    /// <param name="exploredNodes">All nodes that were previously explored in the recursion</param>
    /// <param name="linkedNodes">How many nodes are currently linked by color in this chain, defaults to 0 for starting node because no chain is created yet</param>
    /// <param name="colorID">Color ID to check, defaults to -1 for the starting node to know that it should use its color</param>
    /// <returns></returns>
    public int CheckCollumnMatches(List<GemNode> exploredNodes, int linkedNodes = 0, int colorID = -1)
    {
        bool isStartingNode = false;

        // If this is the starting node, meaning no Color ID has been passed through,
        // update the Color ID to this one and set that this is the starting node
        if(colorID == -1)
        {
            colorID = m_curGem.GetColorID();
            isStartingNode = true;
        }

        // If the gem on this node matches the color ID
        // increment the Linked Nodes and recur to the neighbors
        if(m_curGem.GetColorID() == colorID)
        {
            linkedNodes += 1;
            exploredNodes.Add(this);

            bool innerExplored = false;
            bool outerExplored = false;
            foreach(GemNode node in exploredNodes)
            {
                if(node == m_innerNeighbor)
                {
                    innerExplored = true;
                }
                else if(node == m_outerNeighbor)
                {
                    outerExplored = true;
                }
            }

            int innerMatches = 0;
            int outerMatches = 0;
            
            // Perform a null reference check because some nodes will not have inner/outer neighbors
            if(!innerExplored && m_innerNeighbor != null)
            {
                innerMatches = m_innerNeighbor.CheckCollumnMatches(exploredNodes, linkedNodes, colorID);
            }

            if(!outerExplored && m_outerNeighbor != null)
            {
                outerMatches = m_outerNeighbor.CheckCollumnMatches(exploredNodes, linkedNodes, colorID);
            }
            
            // If this is the starting node, the combination will be off by one
            // NOTE: this happens because it will recur through inner and outer
            if(isStartingNode)
            {
                linkedNodes = innerMatches + outerMatches;
                // If either of the neighbors are null, we don't need to adjust because the double count doesn't occur
                if(m_innerNeighbor != null && m_outerNeighbor != null)
                {
                    linkedNodes -= 1;
                }
            }
            else
            {
                // If inner and outer matches added equal zero, we should return the base linkedNodes, skip the reassignment to zero as it will throw off the recursion
                if(innerMatches + outerMatches != 0)
                {
                    linkedNodes = innerMatches + outerMatches;
                }
            }

            return linkedNodes;
        }
        else
        {
            return linkedNodes;
        }
    }

    public int ClearGem()
    {
        GameObject.Destroy(m_curGem.gameObject);
        m_curGem = null;
        return (int)m_nodePosition.y;
    }
}
