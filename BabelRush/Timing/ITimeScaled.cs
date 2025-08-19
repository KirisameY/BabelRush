namespace BabelRush.Timing;

public interface ITimeScaled
{
    void UpdateScale(double prevValue, double newValue);
    void UpdateProcess(double delta, double scale);
}