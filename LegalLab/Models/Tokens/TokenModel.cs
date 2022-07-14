using LegalLab.Extensions;
using LegalLab.Models.Items;
using NeuroFeatures;
using NeuroFeatures.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Waher.Content.Markdown;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Token model.
	/// </summary>
	public class TokenModel : SelectableItem
	{
		private readonly Token token;
		private TokenDetail[] details;
		private BitmapImage glyph;

		/// <summary>
		/// Token model.
		/// </summary>
		/// <param name="Token">Neuro-Feature token</param>
		private TokenModel(Token Token)
		{
			this.token = Token;
		}

		public static async Task<TokenModel> CreateAsync(NeuroFeaturesClient Client, Token Token)
		{
			TokenModel Result = new TokenModel(Token);

			List<TokenDetail> Details = new List<TokenDetail>()
			{
				new TokenDetail("Token ID", Result.token.TokenId),
				new TokenDetail("Token ID Method", Result.token.TokenIdMethod),
				new TokenDetail("Ordinal", Result.token.Ordinal),
				new TokenDetail("Batch Size", Result.token.BatchSize),
				new TokenDetail("Created", Result.token.Created),
				new TokenDetail("Updated", Result.token.Updated),
				new TokenDetail("Value", Result.token.Value),
				new TokenDetail("Currency", Result.token.Currency),
				new TokenDetail("Expires", Result.token.Expires),
				new TokenDetail("Archivig time (Required)", Result.token.ArchiveRequired),
				new TokenDetail("Archivig time (Optional)", Result.token.ArchiveOptional),
				new TokenDetail("Signature Timestamp", Result.token.SignatureTimestamp),
				new TokenDetail("Signature", Convert.ToBase64String(Result.token.Signature)),
				new TokenDetail("Definition Schema Digest", Convert.ToBase64String(Result.token.DefinitionSchemaDigest)),
				new TokenDetail("Definition Schema Hash Function", Result.token.DefinitionSchemaHashFunction),
				new TokenDetail("Creator Can Destroy", Result.token.CreatorCanDestroy),
				new TokenDetail("Owner Can Destroy Batch", Result.token.OwnerCanDestroyBatch),
				new TokenDetail("Owner Can Destroy Individual", Result.token.OwnerCanDestroyIndividual),
				new TokenDetail("Certifier Can Destroy", Result.token.CertifierCanDestroy),
				new TokenDetail("Friendly Name", Result.token.FriendlyName),
				new TokenDetail("Category", Result.token.Category),
				new TokenDetail("Description", await MarkdownToXaml(Result.token.Description)),
				new TokenDetail("Glyph", Convert.ToBase64String(Result.token.Glyph)),
				new TokenDetail("Glyph Content Type", Result.token.GlyphContentType),
				new TokenDetail("Glyph Width", Result.token.GlyphWidth),
				new TokenDetail("Glyph Height", Result.token.GlyphHeight),
				new TokenDetail("Visibility", Result.token.Visibility),
				new TokenDetail("Creator", Result.token.Creator),
				new TokenDetail("CreatorJid", Result.token.CreatorJid),
				new TokenDetail("Owner", Result.token.Owner),
				new TokenDetail("OwnerJid", Result.token.OwnerJid),
				new TokenDetail("TrustProvider", Result.token.TrustProvider),
				new TokenDetail("TrustProviderJid", Result.token.TrustProviderJid),
				new TokenDetail("Reference", Result.token.Reference),
				new TokenDetail("Definition", Result.token.Definition),
				new TokenDetail("DefinitionNamespace", Result.token.DefinitionNamespace),
				new TokenDetail("CreationContract", Result.token.CreationContract),
				new TokenDetail("OwnershipContract", Result.token.OwnershipContract)
			};

			foreach (string s in Result.token.Witness)
				Details.Add(new TokenDetail("Witness", s));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in Result.token.Certifier)
				Details.Add(new TokenDetail("Certifier", s));

			foreach (string s in Result.token.Valuator)
				Details.Add(new TokenDetail("Valuator", s));

			foreach (string s in Result.token.Assessor)
				Details.Add(new TokenDetail("Assessor", s));

			foreach (TokenTag Tag in Result.token.Tags)
				Details.Add(new TokenDetail(Tag.Name, Tag.Value));

			if (Result.token.HasStateMachine)
			{
				ReportEventArgs e = await Client.GenerateDescriptionAsync(Token.TokenId, ReportFormat.Xaml);
				if (e.Ok)
					Details.Add(new TokenDetail("State-Machine Description", e.ReportText.ParseSimple()));

				e = await Client.GeneratePresentReportAsync(Token.TokenId, ReportFormat.Xaml);
				if (e.Ok)
					Details.Add(new TokenDetail("State-Machine Present State", e.ReportText.ParseSimple()));

				e = await Client.GenerateHistoryReportAsync(Token.TokenId, ReportFormat.Xaml);
				if (e.Ok)
					Details.Add(new TokenDetail("State-Machine History", e.ReportText.ParseSimple()));

				e = await Client.GenerateStateDiagramAsync(Token.TokenId, ReportFormat.Xaml);
				if (e.Ok)
					Details.Add(new TokenDetail("State-Machine State Diagram", e.ReportText.ParseSimple()));

				e = await Client.GenerateProfilingReportAsync(Token.TokenId, ReportFormat.Xaml);
				if (e.Ok)
					Details.Add(new TokenDetail("State-Machine Profiling", e.ReportText.ParseSimple()));
			}

			Result.details = Details.ToArray();

			return Result;
		}

		private static async Task<object> MarkdownToXaml(string Markdown)
		{
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			string Xaml = await Doc.GenerateXAML();
			return Xaml.ParseSimple();
		}

		/// <summary>
		/// Token glyph.
		/// </summary>
		public BitmapImage Glyph
		{
			get
			{
				if (this.glyph is null && !(this.token.Glyph is null))
				{
					BitmapImage Result = new BitmapImage();
					Result.BeginInit();
					Result.CreateOptions = BitmapCreateOptions.None;
					Result.CacheOption = BitmapCacheOption.Default;
					Result.StreamSource = new MemoryStream(this.token.Glyph);
					Result.EndInit();

					this.glyph = Result;
				}

				return this.glyph;
			}
		}

		/// <summary>
		/// Friendly Name of token
		/// </summary>
		public string FriendlyName => this.token.FriendlyName;

		/// <summary>
		/// Category of token
		/// </summary>
		public string Category => this.token.Category;

		/// <summary>
		/// Description of token
		/// </summary>
		public string Description => this.token.Description;

		/// <summary>
		/// ID of token
		/// </summary>
		public string TokenId => this.token.TokenId;

		/// <summary>
		/// When token expires
		/// </summary>
		public DateTime Expires => this.token.Expires;

		/// <summary>
		/// Current value of token
		/// </summary>
		public decimal Value => this.token.Value;

		/// <summary>
		/// Currency
		/// </summary>
		public string Currency => this.token.Currency;

		/// <summary>
		/// When Token was last updated (or created)
		/// </summary>
		public DateTime Updated => this.token.Updated;

		/// <summary>
		/// Creator of token
		/// </summary>
		public string Creator => this.token.Creator;

		/// <summary>
		/// Visibility of token
		/// </summary>
		public ContractVisibility Visibility => this.token.Visibility;

		/// <summary>
		/// Token details.
		/// </summary>
		public TokenDetail[] Details => this.details;

	}
}
