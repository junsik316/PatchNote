// CardManager manages card state; receiver owns and applies its own stats.
public interface ICardEffectReceiver
{
    EffectTarget Target { get; }
    void RefreshCardEffects(CardManager manager);
}
