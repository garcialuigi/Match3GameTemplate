﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A grid is a structured arrangement of cells. The grid properties can be configured by inspector.
/// The grid design is almost the design of a level. It tells how big is the game field, from where
/// tiles are spawned, where tiles are placed. The grid main responsibilities are:
/// - Initialize and Create the cells;
/// - Provide ways to locate and access cells;
/// - Provide information about the state of a cell, so the game logic can react to it.
/// </summary>
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Grid : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private int width;

    [SerializeField]
    private int height;

    [SerializeField]
    private float spacement;

    /// <summary>
    /// Cells used as reference "points" for spawnPipes in the TileManager Tile Spawner
    /// functionality. There will be one spawnPipe per cell.
    /// </summary>
    [SerializeField]
    private int[] cellsReferencedByTileSpawner;

    [SerializeField]
    private int[] ignoredCells;

    [SerializeField]
    private int[] pipePathCells;

    [SerializeField]
    private Color gridBorderColor;

    [SerializeField]
    private Color pipeBorderColor;

    /// <summary>
    /// The multidimensinal array that hold all the cells of the grid.
    /// PS: The pivot of this array is top_left, for example:
    ///  => [x]
    /// ||       (0,0) (1,0) (2,0)
    /// \/       (0,1) (1,1) (2,1)
    /// [y]      (0,2) (1,2) (2,2)
    /// </summary>
    private Cell[,] cells;

    #endregion

    #region Properties

    public int Width { get { return width; } }

    public int Height { get { return height; } }

    public Cell[,] Cells { get { return cells; } }

    public float Spacement { get { return spacement; } }

    #endregion

    private void Awake()
    {
        CreateCells();
        PlaceCells();
    }

    /// <summary>
    /// Create the cells of the grid in the form of a multidimensional array, and configure its id
    /// and indexers, which are used to locate cells in the whole code.
    /// </summary>
    private void CreateCells()
    {
        cells = new Cell[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // The cell's id formula is (y * width) + x
                // x = i, y = j
                cells[i, j] = new Cell(id: j * width + i, xIndex: i, yIndex: j);
            }
        }

        for (int i = 0; i < ignoredCells.Length; i++)
        {
            GetCellById(ignoredCells[i]).SetIsIgnored(true);
        }
    }

    private int CalculateCellId(int xIndex, int yIndex)
    {
        return (yIndex * width) + xIndex;
    }

    /// <summary>
    /// Iterate through all cells and set its position in the world. Called in the awake of the Grid
    /// and by the Update function(Edit mode).
    /// </summary>
    private void PlaceCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cells[i, j].Position = new Vector3(transform.position.x + (i * (TileManager.TILE_SIZE + spacement)),
                                                   transform.position.y - (j * (TileManager.TILE_SIZE + spacement)));
            }
        }
    }

    public Cell GetCellById(int id)
    {
        // The cell's id formula is (y * width) + x
        // To find the xIndex from a given id, we do xIndex = id%width
        // To find the yIndex from a given id, we do yIndex = id/width
        return cells[id % width, id / width];
    }

    /// <summary>
    /// Find a cell that is closest to a given position, and return it. If there is no cell close to
    /// the position(the position is too far away), the function will return null.
    /// </summary>
    public Cell GetCellCloseToWorldPosition(Vector3 position)
    {
        // Make sure the given position's z is zero, so it doesn't interfere in the distance calculation
        position.z = 0.0f;

        // Maintain the closest
        Cell closestCell = null;
        float closestSqrDistance = Mathf.Infinity;

        float sqrDistanceConsideredTooFar = ((TileManager.TILE_SIZE / 2) + (spacement / 2)) * ((TileManager.TILE_SIZE / 2) + (spacement / 2));

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // PS: Using sqrMagnitude instead of magnitude/distance because it is more performant
                float currentSqrDistance = Vector3.SqrMagnitude(position - cells[i, j].Position);
                if (currentSqrDistance < sqrDistanceConsideredTooFar && currentSqrDistance < closestSqrDistance)
                {
                    closestCell = cells[i, j];
                    closestSqrDistance = currentSqrDistance;
                }
            }
        }

        return closestCell;
    }

    public int QuantityOfEmptyCellsAtColumn(int xIndex)
    {
        int spacesCounter = 0;
        for (int i = 0; i < height; i++)
        {
            if (!cells[xIndex, i].IsFull())
            {
                spacesCounter++;
            }
        }
        return spacesCounter;
    }

    /// <summary>
    /// Find and return the empty cell that is lower located in a given column, iterating the cells
    /// from down to up. Will return null if the column is full(no empty cells).
    /// </summary>
    public Cell GetTheLowerEmptyCellAtColumn(int xIndex)
    {
        Cell lowerEmptyCell = null;
        for (int i = height - 1; i >= 0; i--)
        {
            if (!cells[xIndex, i].IsFull())
            {
                lowerEmptyCell = cells[xIndex, i];
                break;
            }
        }
        return lowerEmptyCell;
    }

    /// <summary>
    /// Get the array of tiles used as reference in the TileManager tile spawner functionality.
    /// </summary>
    public Cell[] GetCellsReferencedByTileSpawner()
    {
        Cell[] spawnCells = new Cell[cellsReferencedByTileSpawner.Length];
        for (int i = 0; i < cellsReferencedByTileSpawner.Length; i++)
        {
            spawnCells[i] = GetCellById(cellsReferencedByTileSpawner[i]);
        }
        return spawnCells;
    }

    public bool IsCellReferenceForTileSpawner(Cell cellToTest)
    {
        bool isTileSpawnerReference = false;
        for (int i = 0; i < cellsReferencedByTileSpawner.Length; i++)
        {
            if (cellToTest.id == cellsReferencedByTileSpawner[i])
            {
                isTileSpawnerReference = true;
                break;
            }
        }
        return isTileSpawnerReference;
    }

    [EditorButton]
    private void TestDraw()
    {
        DrawBorderAroundCells(cell => !cell.IsIgnored(), gridBorderColor);
        DrawBorderAroundCells(cell => pipePathCells.Contains(CalculateCellId(cell.xIndex, cell.yIndex)), pipeBorderColor);
    }

    private void DrawBorderAroundCells(Predicate<Cell> predicateCellFilter, Color borderColor)
    {
        List<LineInfo> lines = new List<LineInfo>();
        LineInfo currentLine = null;

        //LEF TO RIGHT, CHECKIN UP #################################################################################
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (predicateCellFilter(cells[x, y]) && (y == 0 || !predicateCellFilter(cells[x, y - 1])))
                {
                    if (currentLine == null)
                    {
                        currentLine = new LineInfo();
                        currentLine.from = cells[x, y].GetVertex(SpriteAlignment.TopLeft);
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.TopRight);
                    }
                    else
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.TopRight);
                    }
                }
                else
                {
                    if (currentLine != null)
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.TopLeft);
                        lines.Add(currentLine);
                        currentLine = null;
                    }
                }
            }

            if (currentLine != null)
            {
                lines.Add(currentLine);
                currentLine = null;
            }
        }
        //#################################################################################


        //lEFT TO RIGHT, CHECKIN DOWN ####################################################################
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (predicateCellFilter(cells[x, y]) && (y == height - 1 || !predicateCellFilter(cells[x, y + 1])))
                {
                    if (currentLine == null)
                    {
                        currentLine = new LineInfo();
                        currentLine.from = cells[x, y].GetVertex(SpriteAlignment.BottomLeft);
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.BottomRight);
                    }
                    else
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.BottomRight);
                    }
                }
                else
                {
                    if (currentLine != null)
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.BottomLeft);
                        lines.Add(currentLine);
                        currentLine = null;
                    }
                }
            }

            if (currentLine != null)
            {
                lines.Add(currentLine);
                currentLine = null;
            }
        }
        //#################################################################################

        //TOP TO DOWN, CHECKIN LEFT #################################################################################
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (predicateCellFilter(cells[x, y]) && (x == 0 || !predicateCellFilter(cells[x - 1, y])))
                {
                    if (currentLine == null)
                    {
                        currentLine = new LineInfo();
                        currentLine.from = cells[x, y].GetVertex(SpriteAlignment.TopLeft);
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.BottomLeft);
                    }
                    else
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.BottomLeft);
                    }
                }
                else
                {
                    if (currentLine != null)
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.TopLeft);
                        lines.Add(currentLine);
                        currentLine = null;
                    }
                }
            }

            if (currentLine != null)
            {
                lines.Add(currentLine);
                currentLine = null;
            }
        }
        //#################################################################################

        //TOP TO DOWN, CHECKIN RIGHT #################################################################################
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (predicateCellFilter(cells[x, y]) && (x == width - 1 || !predicateCellFilter(cells[x + 1, y])))
                {
                    if (currentLine == null)
                    {
                        currentLine = new LineInfo();
                        currentLine.from = cells[x, y].GetVertex(SpriteAlignment.TopRight);
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.BottomRight);
                    }
                    else
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.BottomRight);
                    }
                }
                else
                {
                    if (currentLine != null)
                    {
                        currentLine.to = cells[x, y].GetVertex(SpriteAlignment.TopRight);
                        lines.Add(currentLine);
                        currentLine = null;
                    }
                }
            }

            if (currentLine != null)
            {
                lines.Add(currentLine);
                currentLine = null;
            }
        }
        //#################################################################################


        // <><><><><><><><><><><><><><><><><><><><><><><><><><<<><><><><><><><>
        for (int i = 0; i < lines.Count; i++)
        {
            LineDrawer.Instance.DrawLine(lines[i].from, lines[i].to, borderColor, 0.03f);
        }
    }

    private class LineInfo
    {
        public Vector2 from;
        public Vector2 to;
    }

    #region Editor
#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (cells == null || cells.GetLength(0) != width || cells.GetLength(1) != height)
            {
                CreateCells();
            }
            PlaceCells();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            if (cells != null)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (!ignoredCells.Contains(CalculateCellId(i, j)))
                        {
                            Color color = Color.blue;
                            color.a = 0.5f;
                            Gizmos.color = color;
                            Gizmos.DrawCube(cells[i, j].Position, Vector3.one * TileManager.TILE_SIZE);
                        }
                    }
                }

                // Tile Spawner
                for (int i = 0; i < cellsReferencedByTileSpawner.Length; i++)
                {
                    Color color = Color.red;
                    color.a = 0.5f;
                    Gizmos.color = color;
                    Gizmos.DrawCube(GetCellById(cellsReferencedByTileSpawner[i]).Position, Vector3.one * TileManager.TILE_SIZE * 0.2f);
                }

                // Pipe Path
                for (int i = 0; i < pipePathCells.Length; i++)
                {
                    Color color = Color.green;
                    color.a = 0.5f;
                    Gizmos.color = color;
                    Gizmos.DrawCube(GetCellById(pipePathCells[i]).Position, Vector3.one * TileManager.TILE_SIZE * 0.5f);
                }
            }
        }
    }
#endif 
    #endregion
}