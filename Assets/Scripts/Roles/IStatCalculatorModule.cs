using System.Collections.Generic;
public interface IStatCalculatorModule
{
    float Calculate(Dictionary<RoleType, Role> roles);
}
