public class CardRuntime
{
    public CardData data;
    public int remainingLife;

    public CardRuntime(CardData data)
    {
        this.data = data;
        this.remainingLife = data.maxLife;
    }

    public void TickLife()
    { 
            remainingLife--;
    }

    public bool IsExpired() => remainingLife <= 0;
}
