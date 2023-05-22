
namespace VaporKeys
{
    public interface IKey
    {
        public const int EmptyKey = 371857150;

        int Key { get; }
        void ForceRefreshKey();
        string DisplayName { get; }
        string InternalID { get; }
        bool IsDeprecated { get; }
    }
}
