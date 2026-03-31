using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items;

public class ServerSignatureInfo(ServerSignature Signature)
{
    private readonly ServerSignature signature = Signature;

    public DateTime Timestamp => this.signature.Timestamp;
    public byte[] Signature => this.signature.DigitalSignature;
}
