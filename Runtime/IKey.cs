
namespace VaporKeys
{
    public interface IKey
    {
        int Key { get; }
        string DisplayName { get; }
        string InternalID { get; }
        bool IsDeprecated { get; }
    }
}
