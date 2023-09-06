namespace HorizonsToMechanics;

public record BodyData
(
    int ID,
    string? Name,
    double? Mass,
    // sometimes "GM= n.a."
    //double? GM,
    double? PX,
    double? PY,
    double? PZ,
    double? VX,
    double? VY,
    double? VZ
);
