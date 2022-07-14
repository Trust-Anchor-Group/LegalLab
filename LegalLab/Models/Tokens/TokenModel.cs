using LegalLab.Models.Items;
using NeuroFeatures;
using NeuroFeatures.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Token model.
	/// </summary>
	public class TokenModel : SelectableItem
	{
		private readonly Token token;
		private readonly TokenDetail[] details;
		private BitmapImage glyph;

		/// <summary>
		/// Token model.
		/// </summary>
		/// <param name="Token">Neuro-Feature token</param>
		public TokenModel(Token Token)
		{
			this.token = Token;

			List<TokenDetail> Details = new List<TokenDetail>()
			{
				new TokenDetail("Token ID", this.token.TokenId),
				new TokenDetail("Token ID Method", this.token.TokenIdMethod),
				new TokenDetail("Ordinal", this.token.Ordinal),
				new TokenDetail("Batch Size", this.token.BatchSize),
				new TokenDetail("Created", this.token.Created),
				new TokenDetail("Updated", this.token.Updated),
				new TokenDetail("Value", this.token.Value),
				new TokenDetail("Currency", this.token.Currency),
				new TokenDetail("Expires", this.token.Expires),
				new TokenDetail("Archivig time (Required)", this.token.ArchiveRequired),
				new TokenDetail("Archivig time (Optional)", this.token.ArchiveOptional),
				new TokenDetail("Signature Timestamp", this.token.SignatureTimestamp),
				new TokenDetail("Signature", Convert.ToBase64String(this.token.Signature)),
				new TokenDetail("Definition Schema Digest", Convert.ToBase64String(this.token.DefinitionSchemaDigest)),
				new TokenDetail("Definition Schema Hash Function", this.token.DefinitionSchemaHashFunction),
				new TokenDetail("Creator Can Destroy", this.token.CreatorCanDestroy),
				new TokenDetail("Owner Can Destroy Batch", this.token.OwnerCanDestroyBatch),
				new TokenDetail("Owner Can Destroy Individual", this.token.OwnerCanDestroyIndividual),
				new TokenDetail("Certifier Can Destroy", this.token.CertifierCanDestroy),
				new TokenDetail("Friendly Name", this.token.FriendlyName),
				new TokenDetail("Category", this.token.Category),
				new TokenDetail("Glyph", Convert.ToBase64String(this.token.Glyph)),
				new TokenDetail("Glyph Content Type", this.token.GlyphContentType),
				new TokenDetail("Glyph Width", this.token.GlyphWidth),
				new TokenDetail("Glyph Height", this.token.GlyphHeight),
				new TokenDetail("Visibility", this.token.Visibility),
				new TokenDetail("Creator", this.token.Creator),
				new TokenDetail("CreatorJid", this.token.CreatorJid),
				new TokenDetail("Owner", this.token.Owner),
				new TokenDetail("OwnerJid", this.token.OwnerJid),
				new TokenDetail("TrustProvider", this.token.TrustProvider),
				new TokenDetail("TrustProviderJid", this.token.TrustProviderJid),
				new TokenDetail("Reference", this.token.Reference),
				new TokenDetail("Definition", this.token.Definition),
				new TokenDetail("DefinitionNamespace", this.token.DefinitionNamespace),
				new TokenDetail("CreationContract", this.token.CreationContract),
				new TokenDetail("OwnershipContract", this.token.OwnershipContract)
			};

			foreach (string s in this.token.Witness)
				Details.Add(new TokenDetail("Witness", s));

			foreach (string s in this.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in this.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in this.token.Certifier)
				Details.Add(new TokenDetail("Certifier", s));

			foreach (string s in this.token.Valuator)
				Details.Add(new TokenDetail("Valuator", s));

			foreach (string s in this.token.Assessor)
				Details.Add(new TokenDetail("Assessor", s));

			foreach (TokenTag Tag in this.token.Tags)
				Details.Add(new TokenDetail(Tag.Name, Tag.Value));

			this.details = Details.ToArray();
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
