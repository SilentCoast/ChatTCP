namespace ChatTCP.Classes.Converters
{
    public sealed class ReverseBooleanConverter : BooleanConverter<bool>
    {
        public ReverseBooleanConverter() :
            base(false, true)
        { }
    }
}
