using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace SpaceCore.AssetManagers.Models;

/// <summary>
/// An implementation of IRawTexture Data that uses array pooling.
/// </summary>
public class RawDataRented : IRawTextureData, IDisposable
{
    private bool disposed;
    private Color[] data;

    public RawDataRented(Color[] data, int width, int height)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        this.data = data;
        this.Width = width;
        this.Height = height;
    }

    ~RawDataRented()
    {
        this.Dispose(disposing: false);
    }

    public int Width { get; init; }

    public int Height { get; init; }

    public Color[] Data
    {
        get
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(this.Data));
            }
            return this.data;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            ArrayPool<Color>.Shared.Return(this.data);
            this.data = null!;
            this.disposed = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
