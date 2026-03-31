using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items;

public class ClientSignatureInfo(ClientSignature Signature)
{
    private readonly ClientSignature signature = Signature;

    public string BareJid => this.signature.BareJid;
    public string LegalId => this.signature.LegalId;
    public string Role => this.signature.Role;
    public DateTime Timestamp => this.signature.Timestamp;
    public bool Transferable => this.signature.Transferable;
    public byte[] Signature => this.signature.DigitalSignature;
}
