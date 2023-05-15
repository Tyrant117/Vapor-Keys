
namespace VaporKeys
{
    public interface IKey
    {
        public static int EmptyKey = 371857150;
        int Key { get; }
        string DisplayName { get; }
        string InternalID { get; }
        bool IsDeprecated { get; }
    }
}
