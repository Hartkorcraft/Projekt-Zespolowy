
using System;
using System.Collections.Generic;
using Godot;

public static class Utils
{
    public static Random rng = new Random();

    public static float RandomFloat(float minimum, float maximum)
        => (float)rng.NextDouble() * (maximum - minimum) + minimum;

    public static bool InDistance((int x, int y) vec1, (int x, int y) vec2, int range = 1)
        => Math.Abs(vec2.x - vec1.x) <= range && Math.Abs(vec2.y - vec1.y) <= range;

    public static bool CheckIfInRange(this (int x, int y) pos, (int x, int y) range)
        => (pos.x >= 0 && pos.y >= 0 && pos.x < range.x && pos.y < range.y);

    public static bool CheckIfInsideRectangle(this (int x, int y) pos, (int x, int y) range1, (int x, int y) range2)
        => (pos.x >= range1.x && pos.y >= range1.y && pos.x < range2.x && pos.y < range2.y);


    public static float Lerp(float firstFloat, float secondFloat, float by)
        => firstFloat * (1 - by) + secondFloat * by;

    public static Vector2 Lerp(Vector2 firstVector, Vector2 secondVector, float by)
        => new Vector2(Lerp(firstVector.x, secondVector.x, by), Lerp(firstVector.y, secondVector.y, by));

    public static string ReplaceStringAt(this string str, int index, int length, string replace)
        => str.Remove(index, Math.Min(length, str.Length - index)).Insert(index, replace);

    public static string[] DivideStringIntoLines(string input)
        => input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    public static List<string> DivideStringIntoLinesList(string input)
        => new List<string>(input.Split(','));

    public static string NewLine
        => "\r\n";

    public static int RoundUp(float n)
        => (int)Math.Floor(n + 0.5f);
    public static int RoundDown(float n)
        => (int)Math.Ceiling(n - 0.5f);
    public static double RoundUp(double n)
        => Math.Ceiling(n + 0.5f);
    public static double RoundDown(double n)
        => Math.Floor(n - 0.5);

    public static KeyValuePair<T1, T2> ToPair<T1, T2>(this Tuple<T1, T2> source)
        => new KeyValuePair<T1, T2>(source.Item1, source.Item2);

    public static Tuple<T1, T2> ToTuple<T1, T2>(this KeyValuePair<T1, T2> source)
        => Tuple.Create(source.Key, source.Value);

    private static readonly Dictionary<Dir, (int x, int y)> dictionary = new Dictionary<Dir, (int x, int y)>()
        {
            {   Dir.Up,  (0,1)                   },
            {   Dir.Down,  (0,-1)                },
            {   Dir.Left,  (-1,0)                },
            {   Dir.Right,  (1,0)                },
        };
    public static readonly Dictionary<Dir, (int x, int y)> DirToVecs = dictionary;

    public static readonly Dictionary<DirDiagonal, (int x, int y)> DirDiagonalToVecs = new Dictionary<DirDiagonal, (int x, int y)>()
        {
            {   DirDiagonal.Up,         (0,-1)   },
            {   DirDiagonal.Down,       (0, 1)  },
            {   DirDiagonal.Left,       (-1,0)  },
            {   DirDiagonal.Right,      (1,0)   },
            {   DirDiagonal.UpLeft,     (-1,-1)  },
            {   DirDiagonal.UpRight,    (1,-1)   },
            {   DirDiagonal.DownLeft,   (-1,1) },
            {   DirDiagonal.DownRight,  (1,1)  },
            {   DirDiagonal.Invalid,    (0,0)  },
        };

    public static readonly Dictionary<(int x, int y), DirDiagonal> NormalVecsToDirections = new Dictionary<(int x, int y), DirDiagonal>()
        {
            {   ( 0,-1) , DirDiagonal.Up        },
            {   ( 0, 1) , DirDiagonal.Down      },
            {   (-1, 0) , DirDiagonal.Left      },
            {   ( 1, 0) , DirDiagonal.Right     },
            {   (-1,-1) , DirDiagonal.UpLeft    },
            {   ( 1,-1) , DirDiagonal.UpRight   },
            {   (-1, 1) , DirDiagonal.DownLeft  },
            {   ( 1, 1) , DirDiagonal.DownRight },
            {   ( 0, 0) , DirDiagonal.Invalid   }
        };

    public static System.Array GetEnumValues<TEnum>() => Enum.GetValues(typeof(TEnum));

    public enum Axis { Horizontal, Vertical }
    public enum Dir { Up, Right, Down, Left }
    public enum DirDiagonal { Down, DownLeft, Left, UpLeft, Up, UpRight, Right, DownRight, Invalid }

    public static (int x, int y) GetVecDirToPos((int x, int y) origin, (int x, int y) lookAt)
    {
        int w = lookAt.x - origin.x;
        int h = lookAt.y - origin.y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        (int x, int y) newPos = (0, 0);
        numerator += shortest;
        if (!(numerator < longest))
        {
            numerator -= longest;
            newPos.x += dx1;
            newPos.y += dy1;
        }
        else
        {
            newPos.x += dx2;
            newPos.y += dy2;
        }
        return newPos;
    }
}
