using NeuroFeatures;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Token model.
	/// </summary>
	public class TokenModel
	{
		private readonly Token token;
		private BitmapImage glyph;

		/// <summary>
		/// Token model.
		/// </summary>
		/// <param name="Token">Neuro-Feature token</param>
		public TokenModel(Token Token)
		{
			this.token = Token;
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

	}
}
