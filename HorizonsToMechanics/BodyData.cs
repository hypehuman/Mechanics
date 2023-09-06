namespace HorizonsToMechanics;

public record BodyData
(
    int ID,
    string? Name,
    double? Mass/*,
    // sometimes "GM= n.a."
    double? GM*/
);
