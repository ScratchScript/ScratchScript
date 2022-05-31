using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ScratchScript.Helpers;

public class EmptySpriteDrawer
{
	public static string Generate(string directory)
	{
		Log.Debug("Creating empty sprite in {Directory}", directory);
		var image = new Image<Rgba32>(1, 1);

		var stream = new MemoryStream();
		image.Save(stream, new PngEncoder());

		var checksum = Md5Checksum.FromStream(stream);
		var path = Path.Join(directory, checksum + ".png");
		image.SaveAsPng(path);
		Log.Debug("Empty sprite created as {EmptySpritePath}", path);
		return path;
	}
}