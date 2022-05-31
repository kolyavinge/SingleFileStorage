namespace SingleFileStorage.Maintenance;

public static class DefragmentatorFactory
{
    public static IDefragmentator Make()
    {
        return new Defragmentator(new FileSystem());
    }
}
