using System;
using System.Collections.Generic;
using static System.Math;
using System.Linq;

namespace HartLib.PathFinding
{
    public class PathFinding
    {
        (int x, int y) gridSize;
        PathCell[,] grid;

        /// <summary>
        /// <para> checkBlocking - choose what blocks movement including walls, enemies etc </para> 
        /// <para> getNeigbours - choose neigbouring tiles, may include teleports, stairs etc (and for example hexagonal maps) </para> 
        /// <para> getTileCost - additional cost to neigbour (default to 0) </para> 
        /// <para> Example of use: var path = pathfinding.FindPath(GridPos, TargetPos, (GridPos) => MapTiles.GetCellv(GridPos) == blockingTile, pathfinding.GetNeigboursDiagonal, (GridPos) => 0) </para> 
        /// </summary>
        public List<PathCell> FindPath((int x, int y) startPos, (int x, int y) endPos, Func<(int x, int y), bool> checkBlocking, Func<PathCell, List<PathCell>> getNeigbours, Func<(int x, int y), int> getTileCost, bool checkLast = true)
        {
            int safeCheck = 3000;

            if (startPos.CheckIfInRange(gridSize) is false || endPos.CheckIfInRange(gridSize) is false)
                return new List<PathCell>();

            PathCell startCell = grid[startPos.x, startPos.y];
            PathCell endCell = grid[endPos.x, endPos.y];

            List<PathCell> openSet = new List<PathCell>();
            HashSet<PathCell> closedSet = new HashSet<PathCell>();
            openSet.Add(startCell);

            while (openSet.Any() && safeCheck-- > 0)
            {
                PathCell curCell = openSet[0];

                // Check if there is a cell with lower F 
                for (int i = 1; i < openSet.Count; i++)
                    if (openSet[i].GetFCost() < curCell.GetFCost() || openSet[i].GetFCost() == curCell.GetFCost() && openSet[i].H < curCell.H)
                        curCell = openSet[i];


                openSet.Remove(curCell);
                closedSet.Add(curCell);

                // End Searching earlier
                if (curCell == endCell)
                {
                    if (checkLast && curCell.CheckForCollision(checkBlocking)) { return new List<PathCell>(); }
                    return RetracePath(startCell, endCell);
                }

                List<PathCell> neigbours = getNeigbours.Invoke(curCell);

                foreach (var neigbour in neigbours)
                {
                    if (closedSet.Contains(neigbour) || neigbour.CheckForCollision(checkBlocking))
                        continue;

                    var neigbhourPos = neigbour.GridPos;
                    int newCostToNeighbour = curCell.G + GetDistanceSquareGrid(curCell, neigbour);

                    if (newCostToNeighbour < neigbour.G || openSet.Contains(neigbour) is false)
                    {
                        neigbour.G = newCostToNeighbour;
                        neigbour.H = GetDistanceSquareGrid(neigbour, endCell);
                        neigbour.Parent = curCell;

                        if (openSet.Contains(neigbour) == false)
                            openSet.Add(neigbour);
                    }
                }

            }
            return new List<PathCell>();
        }

        List<PathCell> RetracePath(PathCell startNode, PathCell endNode)
        {
            List<PathCell> path = new List<PathCell>();
            PathCell curCell = endNode;

            while (curCell != startNode)
            {
                if (curCell.Parent is null) { return new List<PathCell>(); }
                path.Add(curCell);
                curCell = curCell.Parent;
            }

            path.Reverse();
            return path;
        }

        public List<PathCell> GetFilledSpace((int x, int y) startPos, Func<(int x, int y), bool> checkBlocking, Func<PathCell, List<PathCell>> getNeigbours)
        {
            if (startPos.CheckIfInsideRectangle((0, 0), gridSize) is false)
                return new List<PathCell>();

            List<PathCell> openSet = new List<PathCell>();
            List<PathCell> closedSet = new List<PathCell>();
            PathCell startCell = grid[startPos.x, startPos.y];
            openSet.Add(startCell);
            var safetyCheck = 10000;

            while (openSet.Any() && safetyCheck-- > 0)
            {
                if (safetyCheck <= 1)
                    throw new Exception("safety check failded!");

                var neighbours = getNeigbours(openSet[0]);
                foreach (var neighbour in neighbours)
                    if (openSet.Contains(neighbour) is false && closedSet.Contains(neighbour) is false && checkBlocking(neighbour.GridPos) is false)
                        openSet.Add(neighbour);
                closedSet.Add(openSet[0]);
                openSet.Remove(openSet[0]);
            }
            return closedSet;
        }


        public List<PathCell> GetNeigbours(PathCell cell)
        {
            List<PathCell> neigbours = new List<PathCell>();
            foreach (var dir in Utils.DirToVecs)
            {
                (int x, int y) neigbhourPos = (cell.GridPos.x + dir.Value.x, cell.GridPos.y + dir.Value.y);
                if (neigbhourPos.CheckIfInRange(gridSize) && grid[neigbhourPos.x, neigbhourPos.y] != null)
                    neigbours.Add(grid[neigbhourPos.x, neigbhourPos.y]);
            }
            return neigbours;
        }

        public List<PathCell> GetNeigboursDiagonal(PathCell cell)
        {
            List<PathCell> neigbours = new List<PathCell>();
            foreach (var dir in Utils.DirDiagonalToVecs)
            {
                (int x, int y) neigbhourPos = (cell.GridPos.x + dir.Value.x, cell.GridPos.y + dir.Value.y);
                if (neigbhourPos.CheckIfInRange(gridSize) && grid[neigbhourPos.x, neigbhourPos.y] != null)
                    neigbours.Add(grid[neigbhourPos.x, neigbhourPos.y]);
            }
            return neigbours;
        }

        static public int GetDistanceSquareGrid(PathCell cellA, PathCell cellB)
        {
            int distantx = Math.Abs(cellA.GridPos.x - cellB.GridPos.x);
            int distanty = Math.Abs(cellA.GridPos.y - cellB.GridPos.y);

            if (distantx > distanty)
                return 14 * distanty + 10 * (distantx - distanty);
            else return 14 * distantx + 10 * (distanty - distantx);
        }

        public PathFinding((int x, int y) _gridSize)
        {
            gridSize = _gridSize;

            grid = new PathCell[gridSize.x, gridSize.y];

            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    grid[x, y] = new PathCell((x, y));
                }
            }
        }
    }

    public class PathCell
    {
        public (int x, int y) GridPos { get; set; }
        public PathCell? Parent { get; set; }
        public int G { get; set; } // Estimated d istance To Start Node
        public int H { get; set; } // Estimated distance To End Node
        int F => G + H;

        public int GetFCost(Func<int>? getTileCost = null) => F + getTileCost?.Invoke() ?? 0;
        public bool CheckForCollision(Func<(int x, int y), bool> checkBlocking) => checkBlocking.Invoke(GridPos);

        public PathCell((int x, int y) gridPos) => this.GridPos = gridPos;
    }
}
