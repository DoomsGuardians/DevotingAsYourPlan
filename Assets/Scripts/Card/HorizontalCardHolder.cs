using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class HorizontalCardHolder : MonoBehaviour
{
    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;

    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject emptySlotPrefab;
    private RectTransform rect;

    [Header("Spawn Settings")]
    // [SerializeField] private int cardsToSpawn = 7;
    public List<Card> cards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    private void OnEnable()
    {
        CardHolderManager.Register(this);
    }

    private void OnDisable()
    {
        CardHolderManager.Unregister(this);
    }

    public void RefreshCardsInfo()
    {
        foreach (var card in cards)
        {
            card.RefreshCardInfo();
        }
    }
    
    public void AddCard(CardRuntime cardRuntime)
    {
        GameObject cardSlot = Instantiate(slotPrefab, transform);
        Card card = cardSlot.GetComponentInChildren<Card>();
        card.Initialize(cardRuntime);
        cards.Add(card);
        card.PointerEnterEvent.AddListener(CardPointerEnter);
        card.PointerExitEvent.AddListener(CardPointerExit);
        card.BeginDragEvent.AddListener(BeginDrag);
        card.EndDragEvent.AddListener(EndDrag);
    }
    
    public void TransferCard(Card card)
    {
        card.transform.parent.SetParent(transform);
        card.transform.parent.transform.localPosition = Vector3.zero;
        cards.Add(card);

        card.PointerEnterEvent.AddListener(CardPointerEnter);
        card.PointerExitEvent.AddListener(CardPointerExit);
        card.BeginDragEvent.AddListener(BeginDrag);
        card.EndDragEvent.AddListener(EndDrag);
        
        if (card.cardVisual != null)
            card.cardVisual.UpdateIndex(transform.childCount);
    }
    
    public void RemoveCard(Card card)
    {
        card.PointerEnterEvent.RemoveListener(CardPointerEnter);
        card.PointerExitEvent.RemoveListener(CardPointerExit);
        card.BeginDragEvent.RemoveListener(BeginDrag);
        card.EndDragEvent.RemoveListener(EndDrag);
        cards.Remove(card); 
        // Destroy(card.transform.parent.gameObject,.1f);
        
        if (card.cardVisual != null)
            card.cardVisual.UpdateIndex(transform.childCount);
    }
    
    public void DestroyCard(Card card)
    {
        card.PointerEnterEvent.RemoveListener(CardPointerEnter);
        card.PointerExitEvent.RemoveListener(CardPointerExit);
        card.BeginDragEvent.RemoveListener(BeginDrag);
        card.EndDragEvent.RemoveListener(EndDrag);
        cards.Remove(card); 
        Destroy(card.transform.parent.gameObject);
        
        if (card.cardVisual != null)
            card.cardVisual.UpdateIndex(transform.childCount);
    }
    
    void Start()
    {
        // for (int i = 0; i < cardsToSpawn; i++)
        // {
        //     Instantiate(slotPrefab, transform);
        // }
        rect = GetComponent<RectTransform>();
        // cards = GetComponentsInChildren<Card>().ToList();
        //
        // int cardCount = 0;
        //
        // foreach (Card card in cards)
        // {
        //     card.PointerEnterEvent.AddListener(CardPointerEnter);
        //     card.PointerExitEvent.AddListener(CardPointerExit);
        //     card.BeginDragEvent.AddListener(BeginDrag);
        //     card.EndDragEvent.AddListener(EndDrag);
        //     cardCount++;
        // }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }

    private void BeginDrag(Card card)
    {
        Debug.Log("添加了开始拖拽的方法");
        selectedCard = card;
    }


    void EndDrag(Card card)
    {
        Debug.Log("添加了拖拽结束的方法");
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0,selectedCard.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        for (int i = 0; i < cards.Count; i++)
        {

            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }
    }

}
