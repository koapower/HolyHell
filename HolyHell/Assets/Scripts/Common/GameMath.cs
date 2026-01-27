public static class GameMath
{
    /// <summary>
    /// Rounding Half Up. 0.5 and above rounds up.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int RoundToInt(float value)
    {
        return (int)(value + 0.5f);
    }
}