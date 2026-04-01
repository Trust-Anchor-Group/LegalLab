using System.IO;

namespace LegalLabMaui.Models.Design
{
	public sealed class ExampleContractOption(string RelativePath)
	{
		public string RelativePath { get; } = RelativePath;
		public string AssetPath => "ExampleContracts/" + Path.GetFileName(this.RelativePath);
		public string DisplayName => this.RelativePath;
	}
}