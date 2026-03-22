using MathNet.Spatial.Euclidean;

namespace MechanicsCore.Arrangements;

public record SamplerLattice(
    Vector3D Min,
    Vector3D Max,
    int NumCellsX,
    int NumCellsY,
    int NumCellsZ
)
{
    public IEnumerable<Body> GenerateSamplerBodies(int nextBodyID)
    {
        throw new NotImplementedException();
    }
}
