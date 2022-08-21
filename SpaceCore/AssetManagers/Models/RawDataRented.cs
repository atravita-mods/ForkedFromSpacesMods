using System;
using System.Buffers;
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

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="data">Color data. EXPECTS A RENTED ARRAY.</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <exception cref="ArgumentNullException"></exception>
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

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Size => this.Width * this.Height;

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

    // the following methods are copied from SMAPI and adapted for RawDataRented.
    // Primarily optimized for use by JA.

    public void Shrink(int x, int y)
    {
        if (x > this.Width || y > this.Height)
            throw new InvalidOperationException();

        this.Width = x;
        this.Height = y;
    }

    public void PatchImage(IRawTextureData source, Rectangle? sourceArea, Rectangle targetArea)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        sourceArea ??= new Rectangle(0, 0, source.Width, source.Height);
        this.PatchImageImpl(source, sourceArea.Value, targetArea);
    }

    private void PatchImageImpl(IRawTextureData source, Rectangle sourceArea, Rectangle targetArea)
    {
        // validate
        if (sourceArea.X < 0 || sourceArea.Y < 0 || sourceArea.Right > source.Width || sourceArea.Bottom > source.Height)
            throw new ArgumentOutOfRangeException(nameof(sourceArea));
        if (targetArea.X < 0 || targetArea.Y < 0 || targetArea.Right > this.Width || targetArea.Bottom > this.Height)
            throw new ArgumentOutOfRangeException(nameof(targetArea));
        if (sourceArea.Width != targetArea.Width || sourceArea.Height != targetArea.Height)
            throw new InvalidOperationException("Source and destination must be the same size and shape.");

        // fast path - copy continguous chunk of data
        // Pretty sure this is just fruit trees.
        if (sourceArea.Width == source.Width && targetArea.Width == this.Width && sourceArea.X == 0 && targetArea.X == 0)
        {
            int sourceIndex = sourceArea.Y * source.Width;
            int targetIndex = targetArea.Y * this.Width;

            Array.Copy(source.Data, sourceIndex, this.Data, targetIndex, sourceArea.Height * sourceArea.Width);
        }
        else
        {
            // slower line-by-line copy
            for (int y = 0; y < source.Height; y++)
            {
                int sourceIndex = (y + sourceArea.Y) * source.Width + sourceArea.X;
                int targetIndex = (y + targetArea.Y) * this.Width + targetArea.X;

                Array.Copy(source.Data, sourceIndex, this.Data, targetIndex, sourceArea.Width);
            }
        }
    }
}
