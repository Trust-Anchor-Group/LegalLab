using Waher.Networking.XMPP;

namespace LegalLabMaui.Models.Network;

/// <summary>
/// Base model for components that need an XMPP connection.
/// </summary>
public abstract class ConnectionSensitiveModel : PersistedModel, IDisposable
{
    private bool disposed = false;

    public virtual void Dispose()
    {
        if (!this.disposed)
        {
            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
