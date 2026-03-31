using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items;

public class AttachmentInfo : Model
{
    private readonly Attachment attachment;
    private readonly Command downloadAttachment;
    private readonly Command removeAttachment;
    private readonly Func<AttachmentInfo, Task> onDownload;
    private readonly Func<AttachmentInfo, Task> onRemove;
    private readonly bool canDownload;
    private readonly bool canRemove;

    public AttachmentInfo(Attachment Attachment, bool CanDownload, bool CanRemove,
        Func<AttachmentInfo, Task> OnDownload, Func<AttachmentInfo, Task> OnRemove)
    {
        this.attachment = Attachment;
        this.canDownload = CanDownload;
        this.canRemove = CanRemove;
        this.onDownload = OnDownload;
        this.onRemove = OnRemove;
        this.downloadAttachment = new Command(() => this.canDownload, () => this.onDownload(this));
        this.removeAttachment = new Command(() => this.canRemove, () => this.onRemove(this));
    }

    public string Id => this.attachment.Id;
    public string FileName => this.attachment.FileName;
    public string ContentType => this.attachment.ContentType;
    public DateTime Timestamp => this.attachment.Timestamp;
    public string LegalId => this.attachment.LegalId;
    public byte[] Signature => this.attachment.Signature;

    public bool CanDownloadAttachment => this.canDownload;
    public bool CanRemoveAttachment => this.canRemove;

    public ICommand DownloadAttachment => this.downloadAttachment;
    public ICommand RemoveAttachment => this.removeAttachment;
}
