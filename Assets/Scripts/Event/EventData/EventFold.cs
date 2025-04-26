using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class EventFold : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private EventInstance evt;
    
    private bool isFold = true;

    public async void OnPointerClick(PointerEventData eventData)
    {
        isFold = !isFold;
        await evt.ToggleCardShow(isFold);
        evt.cardHolder.isFold = isFold;
    }
}
