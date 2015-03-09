using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class scriptGame : MonoBehaviour
{
    public Texture2D background;    // Background image from "board.png" asset
    public Texture2D cell;          // Cell image from "cell.png" asset
    public Sprite[] sprites;        // List of sprites used in "cells" and other controls
    public Image imageIndicator;    // Control to output the "Indicator" state
    public Text textIndicator;      // Control to output the text near the "Indicator" image 

    private int turn;               // Swaps between 1 and -1 on each turn. -1 means "o" turn, 1 means "x" turn
    private bool isGameOver;        // End of game flag. If true, winner contains result of the game
    private int winner;             // Winner of game,  1 for "x", -1 for "o", 0 - draw. Valid only if isGameOver and not isDraw
    private ArrayList winnerCells;  // Array of cells that take line or diagonal or both. Can be used for special effects, like stroke or blinking.
    private int[] cells;            // Board cells. 1 for "x", -1 for "o", by default is Zero, means empty
    private int[] sums;             // 3 Horizontal, 3 vertical and 2 diagonal sums to find best move or detect winning. 

    private int cellWidth;          // Width of cell, taken from cells
    private int cellHeight;         // Height of cell, taken from cells
    private int cellSpaceX;         // Space between cells and horizontal offset depending on width of background and cells
    private int cellSpaceY;         // Space between cells and horizontal offset depending on height of background and cells

    private const int boardLeft = 0;    // X coordinate for board of cells 
    private const int boardTop = 80;    // Y coordinate for board of cells. 

    //==========================================================================
    // Unity specific 
    //==========================================================================

    //--------------------------------------------------------------------------
    // Initialization
    void Start()
    {
        //                           0 1 2 
        // Cell array for the board: 3 4 5 
        //                           6 7 8 
        cells = new int[9];
        sums = new int[8];     // 3 Horizontal, 3 vertical and 2 diagonal
        winnerCells = new ArrayList();

        // Pixel sizes for Cells
        cellWidth = cell.width;
        cellHeight = cell.height;
        cellSpaceX = (int)Math.Round((double)((background.width - 3 * cellWidth) / (2 + 2)));  // 2 spaces and 2 side offsets
        cellSpaceY = (int)Math.Round((double)((background.width - 3 * cellHeight) / (2 + 2)));  // !!!background.width!!! 2 spaces + top and bottom offsets
        if (Debug.isDebugBuild)
        {
            Debug.Log(string.Format("Cell size is {0}x{1} offsets are: {2}, {3}", cellWidth, cellHeight, cellSpaceX, cellSpaceY));
        }


        // Set all variables to default
        gameReset();
    } // void Start()


    //--------------------------------------------------------------------------
    // Initialization
    void Update()
    {
        if (isGameOver) return; // Do nothing if game is stopped

        if (turn == -1) turnByAI(turn); // AI for "o" player

        //        if (turn == 1) turnByAI(turn); // AI for "x" player    
    }


    //--------------------------------------------------------------------------
    // Called when some GUI event or user input event occurs
    void OnGUI()
    {
        // Draw background
        int x = background.width;
        int y = background.height;
        Rect r = new Rect(0, 0, x, y);
        //        GUI.DrawTexture(r, background); // !!! Don't use it with new canvas UI !!!

        // Draw cells and perform cell state changes
        int beginX = (int)(((Screen.width + x) / 2) - x) + boardLeft;
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
                if (cells[i] == 0 && !isGameOver)
                {
                    cellSetValue(i, turn); // Set current turn marker as the cell state 
                    onTurnComplete(turn);
                }
            }

        } // for (int i = 0; i < cells.Length; i++)


        // Draw "Indicator" text and image
        gameUpdateIndicator();
    } // void OnGUI()


    //==============================================================================
    // Game and states control 
    //==============================================================================

    //--------------------------------------------------------------------------
    // Resets cells and set all variables to defaults. Used in Start() and buttonResetGame.onClick. 
    public void gameReset()
    {
        int i;
        for (i = 0; i < cells.Length; i++)
        {
            cells[i] = 0;       // Fill with zeros 
        }
        for (i = 0; i < sums.Length; i++)
        {
            sums[i] = 0;        // Fill with zeros 
        }
        winnerCells.Clear();

        turn = 1;               // "x" turn by default 
        isGameOver = false;     // Gaming is allowed
        winner = 0;             // Draw by default
    }

    //--------------------------------------------------------------------------
    // Called when there is no turn, some player wins, or critical error occurs 
    void gameStop(int theTurn)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log(string.Format("Call of gameStop({0}) complete", theTurn));
        }

        if (Math.Abs(theTurn) == 1) turn = theTurn; // Override global value if parameter is set

        isGameOver = true;      // Gaming is disabled
        gameUpdateGameOver();
        gameUpdateIndicator();
    }

    //--------------------------------------------------------------------------
    // Updates image of turn/winner and text near it
    void gameUpdateIndicator()
    {
        // Output "Indicator" text
        if (textIndicator)
        {
            if (!isGameOver)
                textIndicator.text = "Next turn";
            else
                if (winner == 0)
                    textIndicator.text = "It's a draw";
                else
                    textIndicator.text = "Winner is";

        }

        // Draw "Indicator" image
        if (imageIndicator)
        {
            if (!isGameOver)
            {
                if (turn == 1)
                    imageIndicator.sprite = sprites[1];
                else if (turn == -1)
                    imageIndicator.sprite = sprites[2];
                else
                    imageIndicator.sprite = sprites[0];
            }
            else
            {
                if (winner == 1)
                    imageIndicator.sprite = sprites[1];
                else if (winner == -1)
                    imageIndicator.sprite = sprites[2];
                else
                    imageIndicator.sprite = sprites[0];
            }

        }

    } // void gameUpdateIndicator()

    //--------------------------------------------------------------------------

    bool gameIsThereWinner()
    {
        cellSumsUpdate();
        foreach (int i in sums)
        {
            if (Math.Abs(i) >= 3) return true;
        }
        return false;
    }

    //--------------------------------------------------------------------------
    //
    void gameUpdateGameOver()
    {
        if (!isGameOver) return;

        // Verify is there winner. Get list of winning cells to blink, mark or something
        winner = 0;
        winnerCells.Clear();
        for (int i = 0; i < sums.Length; i++)
        {
            if (Math.Abs(sums[i]) >= 3)
            {
                int a, b, c;
                if (cellBySum(i, out a, out b, out c))
                {
                    winnerCells.Add(a);
                    winnerCells.Add(b);
                    winnerCells.Add(c);
                }
                // There is some winner
                if (sums[i] > 0)  winner = 1; else winner = -1;
            }
        }

    }


    //--------------------------------------------------------------------------
    // Event is called at the end of every turn.
    void onTurnComplete(int theTurn = 0)
    {
        if (Math.Abs(theTurn) == 1) turn = theTurn; // Override global value if parameter is set

        cellSumsUpdate();

        if (cellEmptyCount() < 1)
        {
            gameStop(turn);
            return; // Stop game right there, there is no cells to make turn. Todo: Verify is it Draw?
        }

        if (gameIsThereWinner())
        {
            gameStop(turn);
            return; // Stop game right there, somebody wins
        }

        turn = turn * -1;   // Set turn to opposite value
    }


    //==========================================================================
    // Cells and board
    //==========================================================================

    static int[,] mapCellToSum = new int[8, 3]
    {
        {0, 1, 2},
        {3, 4, 5},
        {6, 7, 8},
        {0, 3, 6},
        {1, 4, 7},
        {2, 5, 8},
        {0, 4, 8},
        {6, 4, 2}
    };

    //--------------------------------------------------------------------------
    // Sets value into calls[] array by index. Any changes of cells during the game process should be made using this method!
    bool cellSetValue(int index, int value = 0)
    {
        if (index < 0 || index >= cells.Length)
        {
            Debug.Log(string.Format("Invalid index parameter for setCellValue({0}, {1})", index, value));
            return false;
        }

        if (Math.Abs(value) > 1)
        {
            Debug.Log(string.Format("Invalid value parameter for setCellValue({0}, {1})", index, value));
            return false;
        }

        cells[index] = value;   // Should be the single place of entire program where cells[] value is changed 

        if (Debug.isDebugBuild)
        {
            Debug.Log(string.Format("Call of setCellValue({0}, {1}) complete", index, value));
        }

        return true;
    }

    //--------------------------------------------------------------------------
    // Returns number of empty cells
    int cellEmptyCount()
    {
        int count = 0;
        foreach (int i in cells)
        {
            if (i == 0) count++;
        }
        return count;
    }

    //--------------------------------------------------------------------------
    // Calculates sum of 3 cells by its' indexes. Used to verify winning cells and to make good turn. 
    // Indexes must be valid!
    int cellSumOf3(int a, int b, int c)
    {
        return cells[a] + cells[b] + cells[c];
    }

    int cellSumOf3(int[] values)
    {
        return values[0] + values[1] + values[2];
    }

    //--------------------------------------------------------------------------
    // Updates sum scores for horizontal, vertical and diagonal lines
    void cellSumsUpdate()
    {
        for (int i = 0; i < mapCellToSum.GetLength(0); i++)
        {
            sums[i] = cellSumOf3(
                mapCellToSum[i, 0],
                mapCellToSum[i, 1],
                mapCellToSum[i, 2]
            );
        }
        /*
                sums[0] = cellSumOf3(0, 1, 2);
                sums[1] = cellSumOf3(3, 4, 5);
                sums[2] = cellSumOf3(6, 7, 8);
                sums[3] = cellSumOf3(0, 3, 6);
                sums[4] = cellSumOf3(1, 4, 7);
                sums[5] = cellSumOf3(2, 5, 8);
                sums[6] = cellSumOf3(0, 4, 8);
                sums[7] = cellSumOf3(6, 4, 2);
        */
    }

    //--------------------------------------------------------------------------
    // Maps index of sums[] to index of cells[]

    bool cellBySum(int index, out int a, out int b, out int c)
    {
        if (index < 0 || index >= mapCellToSum.GetLength(0))
        {
            a = -1;
            b = -1;
            c = -1;
            return false;
        }

        a = mapCellToSum[index, 0];
        b = mapCellToSum[index, 1];
        c = mapCellToSum[index, 2];
        return true;
    }


    //==========================================================================
    // Turns routines and AI for computer player 
    //==========================================================================

    //--------------------------------------------------------------------------
    // Takes some empty cell by random
    bool turnRandom(int theTurn = 0)
    {
        int[] emptyCells = new int[9];  // Every cell of 3x3 board
        int emptyCellsCount = 0;

        // Get indexes of empty cells
        for (int i = 0; i < emptyCells.Length; i++)
        {
            if (cells[i] == 0)
            {
                emptyCells[emptyCellsCount] = i;
                emptyCellsCount++;
            }
        }
        if (emptyCellsCount < 1) return false; // There is no empty cells!!! Todo: stop game here

        // Get some random empty cell and put the turn value into it
        System.Random rnd = new System.Random();
        int randomIndex = rnd.Next(0, emptyCellsCount);
        cellSetValue(emptyCells[randomIndex], theTurn);

        return true;
    }

    //--------------------------------------------------------------------------
    // Takes center cell if possible
    bool turnCenter(int theTurn = 0)
    {
        if (cells[4] != 0) return false; // Center cell is already taken

        cellSetValue(4, theTurn);
        return true;
    }

    //--------------------------------------------------------------------------
    // Takes some corner cell by random
    bool turnCorner(int theTurn = 0)
    {
        int[] emptyCells = new int[4];  // 4 corners
        int emptyCellsCount = 0;

        // Get indexes of empty cells
        int[] cornerCells = new int[] { 0, 2, 6, 8 };    // Corner cells for 3x3 board
        foreach (int i in cornerCells)
        {
            if (cells[i] == 0)
            {
                emptyCells[emptyCellsCount] = i;
                emptyCellsCount++;
            }
        }
        if (emptyCellsCount < 1) return false; // There is no empty corner cells

        // Get some random corner cell and put the turn value into it
        System.Random rnd = new System.Random();
        int randomIndex = rnd.Next(0, emptyCellsCount);
        cellSetValue(emptyCells[randomIndex], theTurn);

        return true;
    }

    //--------------------------------------------------------------------------
    // Blocks possible winning turn for opposite player 
    bool turnBlock(int theTurn = 0)
    {
        // Todo: make defense turn to block line with 2 in row
        return turnWin(theTurn); // !!! Temporary !!!

        return true;
    }

    //--------------------------------------------------------------------------
    // Makes winning turn if possible 
    bool turnWin(int theTurn = 0)
    {
        if (theTurn == 0) theTurn = turn; // Use global variable if parameter is not set
 
        int lookFor = 2;
        if (theTurn < 0) lookFor = -2; 

        for (int i = 0; i < sums.Length; i++)
            if (sums[i] == lookFor) // We can take a win on this line
            {
                int a, b, c;
                cellBySum(i, out a, out b, out c);

                // We can search for empty cell here, but setting all 3 is faster and makes the same result
                if (Debug.isDebugBuild)
                {
                    Debug.Log(string.Format("We found winning line ({0}, {1}, {2}) so 2 of next 3 cellSetValue() calls are fake", a, b, c));
                }
                cellSetValue(a, theTurn);
                cellSetValue(b, theTurn);
                cellSetValue(c, theTurn); 

                return true; // We made the winning turn
            }

        return false; // There is no winnig turn
    }


    //--------------------------------------------------------------------------
    // Performs computer turn depending on current level of AI

    private int levelAI = 4; // 0 - no AI (manual play), from 1 to 5  - easy to hard AI

    void turnByAI(int theTurn = 0)
    {
        if (levelAI < 1) return; // No AI, play manually

        bool isTurnOk = false;
        switch (levelAI)
        {
            case 5:  // Win -> Block -> Center -> Corner -> Random turns
                isTurnOk = turnWin(theTurn);
                if (!isTurnOk) isTurnOk = turnBlock(theTurn);
                if (isTurnOk) break;
                goto case 3;

            case 4: // Win -> Center -> Corner -> Random turns
                isTurnOk = turnWin(theTurn);
                if (isTurnOk) break;
                goto case 3;

            case 3: // Center -> Corner -> Random turns 
                isTurnOk = turnCenter(theTurn);
                if (!isTurnOk) isTurnOk = turnCorner(theTurn);
                if (isTurnOk) break;
                goto default;

            case 2: // Center -> Random turns 
                isTurnOk = turnCenter(theTurn);
                if (isTurnOk) break;
                goto default;

            default: // Random turn for levelAI == 1
                isTurnOk = turnRandom(theTurn);
                break;
        }

        onTurnComplete(theTurn);

    } // void doTurnByAI()

    //--------------------------------------------------------------------------

}
