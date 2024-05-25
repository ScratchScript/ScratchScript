using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ScratchScript.Compiler.Helpers;

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
        var checksum = image.ToMd5Checksum();
        
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