namespace InterpolatedStringFormatter
{
    public static class Extensions
    {
        public static string Interpolate(this string s, params object[] variables) 
        {
            return new InterpolatedStringFormatter(s, variables).ToString();
        }
    }
}
