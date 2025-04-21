using UnityEngine;

public class PeopleLogicModule : IRoleLogicModule
{
    const float GrowthRate = 0.2f;
    const float DeathRate = 0.7f;
    const float BaseYield = 1f;
    const float K = 900f;

    public void Settle(Role role, int round)
    {
        float population = role.GetStat("人口数");
        float food = role.GetStat("粮储");
        float faith = role.GetStat("信众数");
        float development = role.GetStat("发展度");
        float mystery = role.GetStat("神秘性");
        float animacy = role.GetStat("神格性");

        // 粮食产出
        float foodOutput = population * BaseYield * Mathf.Max(0f, 1 + development / (K + Mathf.Abs(development)));
        float foodConsumption = population;

        float newFood = food + foodOutput - foodConsumption;
        role.SetStat("粮储", Mathf.Max(0, newFood));

        // 人口变化
        float popGrowth = 0f, popDeath = 0f;
        if (newFood >= population)
        {
            float surplusRatio = Mathf.Min(1f, (newFood - population) / population);
            popGrowth = population * GrowthRate * Mathf.Sqrt(surplusRatio);
        }
        else
        {
            float shortageRatio = Mathf.Abs(newFood - population) / population;
            popDeath = population * DeathRate * Mathf.Pow(shortageRatio, 1.5f);
        }

        float newPopulation = population + popGrowth - popDeath;
        role.SetStat("人口数", newPopulation);

        // 信众变化
        float convertPool = newPopulation - faith;
        float spreadEff = 1 + mystery * 0.001f + animacy * 0.001f;
        float faithGrowth = convertPool * 0.01f * spreadEff;
        float faithDeath = faith * (popDeath / Mathf.Max(population, 1f));
        float newFaith = Mathf.Min(newPopulation, faith + faithGrowth - faithDeath);

        role.SetStat("信众数", newFaith);
    }
}