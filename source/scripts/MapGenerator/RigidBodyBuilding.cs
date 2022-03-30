using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class RigidBodyBuilding : RigidBody2D
{
    public BSPNode? RoomTree { get; private set; }
    public (int x, int y) BuildingSize { get; private set; }
    public (float x, float y) offset { get; private set; }

    CollisionShape2D? collisionShape2D { get; set; } = null!;
    int tileSize = 8;
    bool draw = false;

    public void GenerateBuilding(Vector2 pos, (int x, int y) buildingSize, (int x, int y) minRoomSize, float solverBias = 0.1f, int tileSize = 8, int maxRoomNum = 100)
    {
        //Stworzenie drzewa pokoi 
        RoomTree = new BSPNode((0, 0), buildingSize);
        bool finishedDividing = false;

        while (finishedDividing is false)
        {
            var nodes = RoomTree.GetTree();

            // Sprawdzenie czy można dzielić dalej
            var unDivided = nodes.Where((node) => node.CanBeDivided);
            if (unDivided.Any() is false) finishedDividing = true;

            foreach (var n in unDivided)
            {
                // Skończenie dzielenia jeżeli osiągnięto maksymalną ilość pokoi 
                if (RoomTree.GetLowestNodes().Count + 1 > maxRoomNum)
                {
                    finishedDividing = true;
                    break;
                }
                n.Divide(minRoomSize);
            }
        }

        // Ustawianie collidera i fizyki 
        Mode = ModeEnum.Character;
        GravityScale = 0;
        collisionShape2D = GetChild<CollisionShape2D>(0);
        Position = pos;

        this.tileSize = tileSize;
        this.BuildingSize = buildingSize;
        this.offset = (((float)BuildingSize.x / 2), ((float)BuildingSize.y / 2));

        var shape = new RectangleShape2D();
        shape.CustomSolverBias = solverBias;
        shape.Extents = buildingSize.ToVec2() * tileSize / 2 + new Vector2(16, 16);
        collisionShape2D.Shape = shape;
        //collisionShape2D.Position = buildingSize.ToVec2() * tileSize / 2;

        Update();
    }

    public override void _Draw()
    {
        if (draw is false) return;

        DrawCircle(Vector2.Zero, 3, new Color(1, 1, 1));
        if (RoomTree is not null)
        {

            foreach (var node in RoomTree.GetLowestNodes())
            {
                var color = new Color(node.Color.r, node.Color.g, node.Color.b, 0.8f);

                // Rysowanie pokoju
                DrawRect(new Rect2((node.pos.x - offset.x) * tileSize, (node.pos.y - offset.y) * tileSize, node.size.x * tileSize, node.size.y * tileSize), color);

                //Rysowanie Ścian pokoju
                for (int x = 0; x < node.size.x; x++)
                    for (int y = 0; y < node.size.y; y++)
                        if (x == 0 || y == 0)
                            DrawRect(new Rect2((node.pos.x + x - offset.x) * tileSize, (node.pos.y + y - offset.y) * tileSize, tileSize, tileSize), color);

            }

            //Rysowanie krawędzi budynku
            for (int x = 0; x < RoomTree.size.x; x++)
                for (int y = 0; y < RoomTree.size.y; y++)
                    if (x == 0 || y == 0 || x == RoomTree.size.x - 1 || y == RoomTree.size.y - 1)
                        DrawRect(new Rect2((RoomTree.pos.x + x - offset.x) * tileSize, (RoomTree.pos.y + y - offset.y) * tileSize, tileSize, tileSize), new Color(0, 0, 1, 0.5f));
        }

    }


    public class BSPNode
    {
        public readonly (int x, int y) pos;
        public readonly (int x, int y) size;
        public bool CanBeDivided = true;
        public Color Color;

        // Dzielenie gałęzi drzewa na pół
        public bool Divide((int x, int y) minSize)
        {
            if (CanBeDivided is false) throw new Exception("Can't be divided");
            if (size.x < minSize.x) throw new Exception("Size too smal x");
            if (size.y < minSize.y) throw new Exception("Size too small y");

            var canDivideX = minSize.x * 2 <= size.x;
            var canDivideY = minSize.y * 2 <= size.y;

            if (canDivideX is false && canDivideY is false)
            {
                CanBeDivided = false;
                return false;
            }

            // Wybieranie czy dzielić horyzontalnie czy wertykalnie 
            var divideX = canDivideX && canDivideY ? Convert.ToBoolean(Utils.rng.Next(2)) : canDivideX ? true : canDivideY ? false : throw new Exception("can't divide");

            var newPosChild1 = pos;
            if (divideX)
            {
                (int x, int y) newSizeChild1 = (Utils.rng.Next(minSize.x, size.x - minSize.x), size.y);
                (int x, int y) newSizeChild2 = (size.x - newSizeChild1.x, size.y);
                var newPosChild2 = (newPosChild1.x + newSizeChild1.x, newPosChild1.y);
                Child1 = new BSPNode(newPosChild1, newSizeChild1);
                Child2 = new BSPNode(newPosChild2, newSizeChild2);
            }
            else
            {
                (int x, int y) newSizeChild1 = (size.x, Utils.rng.Next(minSize.y, size.y - minSize.y));
                (int x, int y) newSizeChild2 = (size.x, size.y - newSizeChild1.y);
                var newPosChild2 = (newPosChild1.x, newPosChild1.y + newSizeChild1.y);
                Child1 = new BSPNode(newPosChild1, newSizeChild1);
                Child2 = new BSPNode(newPosChild2, newSizeChild2);
            }//TODO refactor

            CanBeDivided = false;
            return true;
        }

        BSPNode? Child1 = null;
        BSPNode? Child2 = null;

        // Zwraca całe drzewo
        public List<BSPNode> GetTree()
        {
            List<BSPNode> result = new List<BSPNode>();
            result.Add(this);
            if (Child1 is not null)
                result.AddRange(Child1.GetTree());
            if (Child2 is not null)
                result.AddRange(Child2.GetTree());
            return result;
        }

        // Zrwaca tylko najniższe gałęzie 
        public List<BSPNode> GetLowestNodes()
        {
            List<BSPNode> result = new List<BSPNode>();
            if (Child1 is null && Child2 is null) result.Add(this);
            if (Child1 is not null)
                result.AddRange(Child1.GetLowestNodes());
            if (Child2 is not null)
                result.AddRange(Child2.GetLowestNodes());
            return result;
        }

        public BSPNode((int x, int y) pos, (int x, int y) size)
        {
            this.pos = pos;
            this.size = size;
            this.Color = new Color(((float)Utils.rng.NextDouble()), ((float)Utils.rng.NextDouble()), ((float)Utils.rng.NextDouble()));
        }
    }
}