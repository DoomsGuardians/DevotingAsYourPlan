using UnityEngine;

[CreateAssetMenu(fileName = "New Card Entry", menuName = "Cards/Entry")]
public class CardEntry : ScriptableObject
{
    public string entryName;
    [TextArea] public string description;
}
