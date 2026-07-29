using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.ProjectEmitter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ScratchScript.Compiler.ProjectEmitter.Helpers;

public static class CostumeHelper
{
    public static byte[] GetEmptyImage()
    {
        var image = new Image<Rgba32>(1, 1);
        var stream = new MemoryStream();
        image.Save(stream, new PngEncoder());
        return stream.ToArray();
    }

    public static Costume GetEmptyCostume()
    {
        var image = GetEmptyImage();
        var checksum = EnumerableExtensions.ToMd5Checksum(image);

        return new Costume
        {
            Name = "empty",
            DataFormat = "png",
            AssetId = checksum,
            Md5Extension = checksum + ".png",
            Data = image
        };
    }
}