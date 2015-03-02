using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class scriptGame : MonoBehaviour
{
    public Texture2D background;    // Background image from "board.png" asset
    public Texture2D cell;          // Cell image from "cell.png" asset
    public Sprite[] sprites;        // List of sprites used in "cells" and other controls
    public Image imageNextTurn;     // Control to output the "Next turn" marker

    private int turn;               // Swaps between 1 and -1 on each turn. -1 means "o" turn, 1 means "x" turn

    private int[] cells;            // Board cells. 1 for "x", -1 for "o", by default is Zero, means empty

    private int cellWidth;          // Width of cell, taken from sprite[0]
    private int cellHeight;         // Height of cell, taken from sprite[0]
    private int cellSpaceX;         // Space between cells and horizontal offset depending on width of background and sprite[0]
    private int cellSpaceY;         // Space between cells and horizontal offset depending on height of background and sprite[0]



    // Resets cells and set all variables to defaults. Used in Start() and buttonResetGame.onClick. 
    public void resetGame()
    {
        turn = 1;   // "x" turn by default 
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = 0;   // Fill with zeros 
        }
    }


    // Switches the current turn and performs game logic calculations
    public void turnComplete(int theTurn)
    {
        turn = turn * -1;   // Set turn to oposite value
    }


    // Use this for initialization
    private const int boardLeft = 0;    // X coordinate for board of cells 
    private const int boardTop  = 80;   // Y coordinate for board of cells. 
    void Start()
    {
        //                           0 1 2 
        // Cell array for the board: 3 4 5 
        //                           6 7 8 
        cells = new int[9];

        // Pixel sizes for Cells
        cellWidth  = cell.width;
        cellHeight = cell.height;
        cellSpaceX = (int)Math.Round((double)((background.width - 3 * cellWidth ) / (2 + 2)));  // 2 spaces and 2 side offsets
        cellSpaceY = (int)Math.Round((double)((background.width - 3 * cellHeight) / (2 + 2)));  // !!!background.width!!! 2 spaces + top and bottom offsets
        if (Debug.isDebugBuild)
        {
            Debug.Log(string.Format("Cell size is {0}x{1} offsets are: {2}, {3}", cellWidth, cellHeight, cellSpaceX, cellSpaceY));
        }


        // Set all variables to default
        resetGame(); 
    }


    // Update is called once per frame
    void Update()
    {

    }


    void OnGUI()
    {
        // Draw background
        int x = background.width;
        int y = background.height;
        Rect r = new Rect(0, 0, x, y);
//        GUI.DrawTexture(r, background); // !!! Don't use it with new canvas UI !!!

        // Draw cells and perform cell state changes
        int beginX = (int)(((Screen.width  + x) / 2) - x) + boardLeft;
        int beginY = (int)(((Screen.height + y) / 2) - y) + boardTop;
        for (int i = 0; i < cells.Length; i++)
        {
            x = beginX + cellSpaceX + i % 3 * cellWidth + i % 3 * cellSpaceX;
            y = beginY + cellSpaceY + i / 3 * cellHeight + i / 3 * cellSpaceY;
            r = new Rect(x, y, cellWidth, cellHeight);

            // Choose proper sprite to draw current cell
            Sprite sprite = sprites[0];
            if (cells[i] == 1)
                sprite = sprites[1];
            if (cells[i] == -1)
                sprite = sprites[2];

            // Get button object for current cell
            if (GUI.Button(r, sprite.texture, GUIStyle.none))
            {
                if (cells[i] == 0) 
                {
                    cells[i] = turn;        // Set current turn marker as the cell state 
                    turnComplete(turn);
			    }
            }

        } // for (int i = 0; i < cells.Length; i++)


        // Draw "next turn" image
        if (imageNextTurn)
        {
            if (turn == 1)
                imageNextTurn.sprite = sprites[1];
            else if (turn == -1)
                imageNextTurn.sprite = sprites[2];
            else
                imageNextTurn.sprite = sprites[0];
        }

    } // void OnGUI()
}
