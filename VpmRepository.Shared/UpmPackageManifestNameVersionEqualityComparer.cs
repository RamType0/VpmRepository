namespace VpmRepository;

public sealed class UpmPackageManifestNameVersionEqualityComparer : IEqualityComparer<UpmPackageManifest>
{
    public static UpmPackageManifestNameVersionEqualityComparer Instance { get; } = new UpmPackageManifestNameVersionEqualityComparer();
    public bool Equals(UpmPackageManifest? x, UpmPackageManifest? y)
    {
        if (x is null && y is null)
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        return x.Name == y.Name && x.Version == y.Version;
    }
    public int GetHashCode(UpmPackageManifest obj)
    {
        return HashCode.Combine(obj.Name, obj.Version);
    }
}