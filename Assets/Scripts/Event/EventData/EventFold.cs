using UnityEngine;
using UnityEngine.EventSystems;

public class EventFold : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private EventInstance evt;
    
    private bool isFold = true;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        evt.ToggleCardShow(isFold);
        if (evt.animancer != null)
        {
            if (isFold)
            {
                evt.animancer.Play(evt.clips[0]);
            }
            else
            {
                evt.animancer.Play(evt.clips[1]);
            }
        }
        isFold = !isFold;
        evt.cardHolder.isFold = isFold;
    }
}
