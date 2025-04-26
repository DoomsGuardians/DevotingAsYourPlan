using System.Collections.Generic;
public interface IRarityCalculatorModule
{
    Dictionary<int, int> CalculateRarity (Dictionary<RoleType, Role> roles);
}
