using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.DXGI;

namespace RGB.Util
{
    internal class DesktopDuplication : IDisposable
    {
        private bool disposed = false;

        private readonly Adapter adapter;
        private readonly SharpDX.Direct3D11.Device device;
        private readonly Output output;
        private readonly Output1 output1;
        private readonly Texture2D stagingTexture, smallerTexture;
        private readonly ShaderResourceView smallerTextureView;
        private readonly OutputDuplication duplicatedOutput;
        private int width, height, mipWidth, mipHeight, mipLevel;
        public Bitmap bitmap;

        public DesktopDuplication()
        {
            adapter = new Factory1().GetAdapter(0);
            device = new SharpDX.Direct3D11.Device(adapter);
            output = adapter.GetOutput(0);
            output1 = output.QueryInterface<Output1>();
            width = output.Description.DesktopBounds.Right - output.Description.DesktopBounds.Left;
            height = output.Description.DesktopBounds.Bottom - output.Description.DesktopBounds.Top;
            mipLevel = 6;
            mipWidth = (int)(width / Math.Pow(2, mipLevel - 1));
            mipHeight = (int)(height / Math.Pow(2, mipLevel - 1));

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = mipWidth,
                Height = mipHeight,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            stagingTexture = new Texture2D(device, textureDesc);

            // Create Staging texture CPU-accessible
            var smallerTextureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                MipLevels = mipLevel,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default
            };
            smallerTexture = new Texture2D(device, smallerTextureDesc);
            smallerTextureView = new ShaderResourceView(device, smallerTexture);

            duplicatedOutput = output1.DuplicateOutput(device);
            bitmap = new Bitmap(mipWidth, mipHeight, PixelFormat.Format32bppArgb);
        }
        object l = new object();
        public int UpdateFrame()
        {
            lock(l)
            {
                if(disposed)
                {
                    return 0;
                }

                SharpDX.DXGI.Resource screenResource;
                OutputDuplicateFrameInformation duplicateFrameInformation;

                // Try to get duplicated frame within given time
                if (duplicatedOutput.TryAcquireNextFrame(1000, out duplicateFrameInformation, out screenResource) != Result.Ok) return -1;

                // copy resource into memory that can be accessed by the CPU
                using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    device.ImmediateContext.CopySubresourceRegion(screenTexture2D, 0, null, smallerTexture, 0);

                // Generates the mipmap of the screen
                device.ImmediateContext.GenerateMips(smallerTextureView);

                // Copy the mipmap 1 of smallerTexture (size/2) to the staging texture
                device.ImmediateContext.CopySubresourceRegion(smallerTexture, mipLevel - 1, null, stagingTexture, 0);

                // Get the desktop capture texture
                var mapSource = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                // Create Drawing.Bitmap
                var boundsRect = new System.Drawing.Rectangle(0, 0, mipWidth, mipHeight);

                // Copy pixels from screen capture Texture to GDI bitmap
                var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                var sourcePtr = mapSource.DataPointer;
                var destPtr = mapDest.Scan0;
                for (int y = 0; y < mipHeight; y++)
                {
                    // Copy a single line 
                    Utilities.CopyMemory(destPtr, sourcePtr, mipWidth * 4);

                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }

                // Release source and dest locks
                bitmap.UnlockBits(mapDest);
                device.ImmediateContext.UnmapSubresource(stagingTexture, 0);


                screenResource.Dispose();
                duplicatedOutput.ReleaseFrame();

                return duplicateFrameInformation.AccumulatedFrames;
            }
        }

        public void Dispose()
        {
            lock(l)
            {
                disposed = true;
                
                duplicatedOutput.Dispose();
                smallerTexture.Dispose();
                stagingTexture.Dispose();
                smallerTextureView.Dispose();
                output1.Dispose();
                output.Dispose();
                device.Dispose();
                adapter.Dispose();
            }
        }
    }
}
