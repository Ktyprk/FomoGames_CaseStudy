using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int MoveLimit;
    public int RowCount;
    public int ColCount;
    public List<CellInfo> CellInfo;
    public List<MovableInfo> MovableInfo;
    public List<ExitInfo> ExitInfo;
}

[Serializable]
public class CellInfo
{
    public int Row;
    public int Col;
}

[Serializable]
public class MovableInfo
{
    public int Row;
    public int Col;
    public List<int> Direction;
    public int Length;
    public int Colors;
}

[Serializable]
public class ExitInfo
{
    public int Row;
    public int Col;
    public int Direction;
    public int Colors;
}