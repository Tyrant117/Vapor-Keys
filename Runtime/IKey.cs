
namespace VaporKeys
{
    public interface IKey
    {
        int Key { get; }
        void ForceRefreshKey();
        string DisplayName { get; }
        bool IsDeprecated { get; }
    }
}
