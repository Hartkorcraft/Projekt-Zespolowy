
public struct MapInfo
{
    public (int x, int y) bottomLeft;
    public (int x, int y) topRight;

    public MapInfo((int x, int y) bottomLeft, (int x, int y) topRight)
    {
        this.bottomLeft = bottomLeft;
        this.topRight = topRight;
    }
}
