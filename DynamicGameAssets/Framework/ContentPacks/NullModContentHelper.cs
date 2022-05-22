using StardewModdingAPI;

namespace DynamicGameAssets.Framework.ContentPacks;

internal class NullModContentHelper : IModContentHelper
{
    public string ModID => string.Empty;

    public IAssetName GetInternalAssetName(string relativePath)
    {
        return default;
    }

    public IAssetData GetPatchHelper<T>(T data, string relativePath = null) where T : notnull
    {
        return default;
    }

    public T Load<T>(string relativePath) where T : notnull
    {
        return default;
    }
}

