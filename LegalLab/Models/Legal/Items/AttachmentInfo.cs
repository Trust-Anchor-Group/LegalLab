using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Temporary;

namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Contains information about an attachment
	/// </summary>
	public class AttachmentInfo : Model
	{
		private readonly ContractModel contractModel;
		private readonly Attachment attachment;
		private readonly Property<string> id;
		private readonly Property<string> legalId;
		private readonly Property<string> contentType;
		private readonly Property<string> fileName;
		private readonly Property<string> url;
		private readonly Property<byte[]> signature;
		private readonly Property<DateTime> timestamp;

		private readonly Command downloadAttachment;
		private readonly Command removeAttachment;

		/// <summary>
		/// Contains information about a client signature
		/// </summary>
		/// <param name="ContractModel">Contract Model hosting the parameter</param>
		/// <param name="Attachment">Client Signature</param>
		public AttachmentInfo(ContractModel ContractModel, Attachment Attachment)
		{
			this.contractModel = ContractModel;
			this.attachment = Attachment;

			this.id = new Property<string>(nameof(this.Id), Attachment.Id, this);
			this.legalId = new Property<string>(nameof(this.LegalId), Attachment.LegalId, this);
			this.contentType = new Property<string>(nameof(this.ContentType), Attachment.ContentType, this);
			this.fileName = new Property<string>(nameof(this.FileName), Attachment.FileName, this);
			this.url = new Property<string>(nameof(this.Url), Attachment.Url, this);
			this.signature = new Property<byte[]>(nameof(this.Signature), Attachment.Signature, this);
			this.timestamp = new Property<DateTime>(nameof(this.Timestamp), Attachment.Timestamp, this);

			this.downloadAttachment = new Command(this.CanExecuteDownloadAttachment, this.ExecuteDownloadAttachment);
			this.removeAttachment = new Command(this.CanExecuteRemoveAttachment, this.ExecuteRemoveAttachment);
		}

		/// <summary>
		/// Reference to attachment object.
		/// </summary>
		public Attachment Attachment => this.attachment;

		/// <summary>
		/// ID of attachment
		/// </summary>
		public string Id
		{
			get => this.id.Value;
			set => this.id.Value = value;
		}

		/// <summary>
		/// Legal ID
		/// </summary>
		public string LegalId
		{
			get => this.legalId.Value;
			set => this.legalId.Value = value;
		}

		/// <summary>
		/// Internet Content-Type of attachment.
		/// </summary>
		public string ContentType
		{
			get => this.contentType.Value;
			set => this.contentType.Value = value;
		}

		/// <summary>
		/// File name of attachment.
		/// </summary>
		public string FileName
		{
			get => this.fileName.Value;
			set => this.fileName.Value = value;
		}

		/// <summary>
		/// URL of content.
		/// </summary>
		public string Url
		{
			get => this.url.Value;
			set => this.url.Value = value;
		}

		/// <summary>
		/// Digital Signature
		/// </summary>
		public byte[] Signature
		{
			get => this.signature.Value;
			set => this.signature.Value = value;
		}

		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTime Timestamp
		{
			get => this.timestamp.Value;
			set => this.timestamp.Value = value;
		}

		/// <summary>
		/// Allows the user to download an attachment.
		/// </summary>
		public ICommand DownloadAttachment => this.downloadAttachment;

		/// <summary>
		/// If the user is allowed to download an attachment.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteDownloadAttachment()
		{
			return this.contractModel?.CanDownloadAttachment ?? false;
		}

		/// <summary>
		/// Downloads an attachment.
		/// </summary>
		public async Task ExecuteDownloadAttachment()
		{
			try
			{
				string Extension = Path.GetExtension(this.FileName);
				if (Extension.StartsWith("."))
					Extension = Extension[1..];

				SaveFileDialog Dialog = new()
				{
					AddExtension = true,
					CheckFileExists = false,
					CheckPathExists = true,
					CreatePrompt = false,
					DefaultExt = Extension,
					Filter = "Similar Files (*." + Extension + ")|*." + Extension,
					OverwritePrompt = true,
					Title = "Save Attachment"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (!Result.HasValue || !Result.Value)
					return;

				MainWindow.MouseHourglass();

				KeyValuePair<string, TemporaryFile> P = await this.contractModel.ContractsClient.GetAttachmentAsync(this.Url, SignWith.CurrentKeys);

				using TemporaryFile Temp = P.Value;
				using FileStream Destination = File.Create(Dialog.FileName);

				Temp.Position = 0;
				await Temp.CopyToAsync(Destination);

				MainWindow.SuccessBox("Attachment successfully downloaded.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Allows the user to remove an attachment.
		/// </summary>
		public ICommand RemoveAttachment => this.removeAttachment;

		/// <summary>
		/// If the user is allowed to remove an attachment.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteRemoveAttachment()
		{
			return this.contractModel?.CanUploadAttachment ?? false;
		}

		/// <summary>
		/// Removes an attachment.
		/// </summary>
		public async Task ExecuteRemoveAttachment()
		{
			try
			{
				if (MessageBox.Show("Are you sure you want to remove the attachment " + this.Id + "?", "Confirm",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
				{
					return;
				}
				
				MainWindow.MouseHourglass();

				Contract Contract = await this.contractModel.ContractsClient.RemoveContractAttachmentAsync(this.Id);

				await this.contractModel.SetContract(Contract);

				MainWindow.SuccessBox("Attachment successfully removed.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}
	}
}
