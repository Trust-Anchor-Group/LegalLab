using LegalLab.Models.Items;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.HttpFileUpload;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Wrapper around a <see cref="LegalIdentity"/>, for purposes of displaying it to the user.
	/// </summary>
	public class IdentityWrapper
		: SelectableItem
	{
		private readonly LegalModel legalModel;
		private readonly string domain;
		private LegalIdentity identity;

		private readonly Command uploadAttachment;
		private readonly Command readyForApproval;
		private readonly Command quickLogin;

		/// <summary>
		/// Wrapper around a <see cref="LegalIdentity"/>, for purposes of displaying it to the user.
		/// </summary>
		///	<param name="Domain">Domain of XMPP Broker.</param>
		/// <param name="Identity">Legal Identity object.</param>
		/// <param name="LegalModel">Legal model.</param>
		public IdentityWrapper(string Domain, LegalIdentity Identity, LegalModel LegalModel)
		{
			this.domain = Domain;
			this.identity = Identity;
			this.legalModel = LegalModel;

			this.uploadAttachment = new Command(this.CanExecuteUploadAttachment, this.ExecuteUploadAttachment);
			this.readyForApproval = new Command(this.CanExecuteReadyForApproval, this.ExecuteReadyForApproval);
			this.quickLogin = new Command(this.CanExecuteQuickLogin, this.ExecuteQuickLogin);
		}

		/// <summary>
		/// Legal Identity ID
		/// </summary>
		public string Id => this.identity.Id;

		/// <summary>
		/// When object was created
		/// </summary>
		public DateTime Created => this.identity.Created;

		/// <summary>
		/// Current state of object
		/// </summary>
		public IdentityState State => this.identity.State;

		/// <summary>
		/// URL to Legal Identity
		/// </summary>
		public string URL => ContractsClient.LegalIdUriString(this.identity.Id);

		/// <summary>
		/// If the identity is in the Created state.
		/// </summary>
		public bool IsCreated => this.identity.State == IdentityState.Created;

		/// <summary>
		/// If the identity is in the Approved state.
		/// </summary>
		public bool IsApproved => this.identity.State == IdentityState.Approved;

		/// <summary>
		/// URL for displaying a QR-code for the <see cref="URL"/>.
		/// </summary>
		public string QrCodeUrl
		{
			get
			{
				if (string.IsNullOrEmpty(this.domain))
					return "https://lab.tagroot.io/QR/" + this.URL + "?w=400&h=400&q=2";
				else
					return "https://" + this.domain + "/QR/" + this.URL + "?w=400&h=400&q=2";
			}
		}

		/// <summary>
		/// Upload Attachment command
		/// </summary>
		public ICommand UploadAttachment => this.uploadAttachment;

		private bool CanExecuteUploadAttachment()
		{
			return this.identity is not null &&
				this.legalModel is not null &&
				this.legalModel.Contracts.Client.TryGetExtension<HttpFileUploadClient>(out _) &&
				this.identity.State == IdentityState.Created &&
				this.legalModel.Contracts.Client.State == XmppState.Connected;
		}

		private async Task ExecuteUploadAttachment()
		{
			try
			{
				if (!this.legalModel.Contracts.Client.TryGetExtension(out HttpFileUploadClient UploadClient))
					return;

				OpenFileDialog Dialog = new()
				{
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = string.Empty,
					Filter =
						"Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.webp)|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.webp|" +
						"PDF Files (*.pdf)|*.pdf|" +
						"XML Files (*.xml)|*.xml|" +
						"All Files (*.*)|*.*",
					Multiselect = false,
					ShowReadOnly = true,
					Title = "Upload attachment"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (!Result.HasValue || !Result.Value)
					return;

				MainWindow.MouseHourglass();

				using FileStream fs = File.OpenRead(Dialog.FileName);
				string FileName = Path.GetFileName(Dialog.FileName);
				string ContentType;

				if (FileName.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase))
					ContentType = "application/pdf";
				else if (!InternetContent.TryGetContentType(Path.GetExtension(FileName), out ContentType))
					throw new Exception("Unsupported file type: " + FileName);

				this.identity = await this.legalModel.Contracts.UploadLegalIdAttachmentAsync(
					this.identity.Id, FileName, fs, ContentType);

				MainWindow.SuccessBox("Attachment uploaded successfully.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
			finally
			{
				MainWindow.MouseDefault();
			}
		}

		/// <summary>
		/// Ready for approval command
		/// </summary>
		public ICommand ReadyForApproval => this.readyForApproval;

		private bool CanExecuteReadyForApproval()
		{
			return this.identity is not null &&
				this.legalModel is not null &&
				this.identity.State == IdentityState.Created &&
				this.legalModel.Contracts.Client.State == XmppState.Connected;
		}

		private async Task ExecuteReadyForApproval()
		{
			try
			{
				await this.legalModel.Contracts.ReadyForApprovalAsync(this.identity.Id);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Quick-Login command
		/// </summary>
		public ICommand QuickLogin => this.quickLogin;

		private bool CanExecuteQuickLogin()
		{
			return this.identity is not null &&
				this.legalModel is not null &&
				this.identity.State == IdentityState.Approved &&
				this.legalModel.Contracts.Client.State == XmppState.Connected;
		}

		private async Task ExecuteQuickLogin()
		{
			try
			{
				string TagSign = MainWindow.PromptUser("Quick-Login", 
					"Enter a tagsign URI to initiate Quick-Login:");

				if (string.IsNullOrEmpty(TagSign))
					return;

				Uri Uri = new(TagSign);
				if (Uri.Scheme != "tagsign")
					throw new Exception("Invalid tagsign URI.");

				string s = Uri.AbsolutePath;
				int i = s.IndexOf(',');
				
				if (i < 0)
					throw new Exception("Invalid tagsign URI.");

				string JID = System.Web.HttpUtility.UrlDecode(s[..i]);
				string Key = s[(i + 1)..];

				if (this.identity is null)
					throw new Exception("No selected Legal Identity.");

				if (this.legalModel is null)
					throw new Exception("No Legal Model.");

				if (this.identity.State	!= IdentityState.Approved)
					throw new Exception("Legal Identity not Approved.");

				if (this.legalModel.Contracts.Client.State != XmppState.Connected)
					throw new Exception("Not connected to XMPP network.");

				StringBuilder Xml = new();

				Xml.Append("<ql xmlns='https://tagroot.io/schema/Signature' key='");
				Xml.Append(XML.Encode(Key));
				Xml.Append("' legalId='");
				Xml.Append(XML.Encode(this.identity.Id));
				Xml.Append("'/>");

				await this.legalModel.Contracts.Client.IqSetAsync(JID, Xml.ToString());
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

	}
}
