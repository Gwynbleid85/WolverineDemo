using Mapster;

namespace MapsterChecker;

public class MapsterTests
{
    public void Test()
    {
        var src = new Source();
        var dest = src.Adapt<Destination>();
    }
}

public class Source
{
    public int Id { get; set; }
    public int Name { get; set; }
}

public class Destination
{
    public int Id { get; set; }
    public string Name { get; set; }
}