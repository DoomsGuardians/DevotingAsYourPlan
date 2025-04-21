using UnityEngine;

public class StatDefinitionRangeAttribute : PropertyAttribute
{
    public string definitionTableFieldName;

    public StatDefinitionRangeAttribute(string definitionTableFieldName)
    {
        this.definitionTableFieldName = definitionTableFieldName;
    }
}