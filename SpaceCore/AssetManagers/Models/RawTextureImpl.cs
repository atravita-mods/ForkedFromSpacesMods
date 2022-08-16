using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace SpaceCore.AssetManagers.Models;

public static class IRawDataExtensions
{
    public static IRawTextureData CopyToRawTexture(this Texture2D texture)
        => new RawTextureImpl(texture);
}

public sealed class RawTextureImpl : IRawTextureData
{
    public int Width { get; init; }

    public int Height { get; init; }

    public Color[] Data { get; init; }

    public RawTextureImpl(Texture2D texture)
    {
        if (texture is null)
            throw new ArgumentNullException(nameof(texture));

        this.Data = GC.AllocateUninitializedArray<Color>(texture.Width * texture.Height);
        texture.GetData(this.Data);

        this.Width = texture.Width;
        this.Height = texture.Height;
    }

    public RawTextureImpl(int width, int height, Color[] data)
    {
        this.Width = width;
        this.Height = height;
        this.Data = data;
    }
}
