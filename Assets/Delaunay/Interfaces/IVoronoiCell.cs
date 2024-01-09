using System.Collections.Generic;

namespace Delaunay
{
    public interface IVoronoiCell
    {
        IPoint[] Points { get; }
        int Index { get; }
    }
}
