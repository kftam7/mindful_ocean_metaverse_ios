public class Range
{
    public float Max { get; set; }
    public float Min { get; set; }

    public float Mean
    {
        get { return (Max + Min) / 2; }
    }

    public Range() { }

    public Range(float max, float min)
    {
        this.Max = max;
        this.Min = min;
    }

    public bool InclusiveRange(float value)
    {
        return value >= Min && value <= Max;
    }

    public bool ExclusiveRange(float value)
    {
        return value > Min && value < Max;
    }
}