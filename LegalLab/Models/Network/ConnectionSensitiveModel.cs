using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.XMPP;

namespace LegalLab.Models.Network
{
	/// <summary>
	/// A model that is sensitive to the connection state.
	/// </summary>
	public class ConnectionSensitiveModel : PersistedModel
	{
		private readonly Property<XmppState> connectionState;
		private readonly Property<bool> connected;
		private NetworkModel networkModel;

		/// <summary>
		/// A model that is sensitive to the connection state.
		/// </summary>
		public ConnectionSensitiveModel()
		{
			this.connected = new Property<bool>(nameof(this.Connected), false, this);
			this.connectionState = new Property<XmppState>(nameof(this.ConnectionState), XmppState.Offline, this);
		}

		/// <summary>
		/// If connected or not.
		/// </summary>
		public bool Connected
		{
			get => this.connected.Value;
		}

		/// <summary>
		/// Current connection state.
		/// </summary>
		public XmppState ConnectionState
		{
			get => this.connectionState.Value;
			set
			{
				this.connectionState.Value = value;
				this.connected.Value = value == XmppState.Connected;
			}
		}

		/// <inheritdoc/>
		public override async Task Start()
		{
			await base.Start();

			this.networkModel = await MainWindow.InstantiateModel<NetworkModel>();
			this.networkModel.OnStateChanged += NetworkModel_OnStateChanged;

			MainWindow.UpdateGui(() => this.ConnectionState = this.networkModel.State);
		}

		/// <inheritdoc/>
		public override async Task Stop()
		{
			this.networkModel.OnStateChanged -= NetworkModel_OnStateChanged;
			await base.Stop();
		}

		private Task NetworkModel_OnStateChanged(object Sender, XmppState NewState)
		{
			MainWindow.UpdateGui(async () =>
			{
				try
				{
					this.ConnectionState = this.networkModel.State;
					await this.StateChanged(NewState);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			});

			return Task.CompletedTask;
		}

		/// <summary>
		/// Method called when connection state has changed.
		/// </summary>
		/// <param name="NewState">New connection state.</param>
		protected virtual async Task StateChanged(XmppState NewState)
		{
			StateChangedEventHandler h = this.OnStateChanged;
			if (!(h is null))
				await h(this, NewState);
		}

		/// <summary>
		/// Event raised when connection state changes.
		/// </summary>
		public event StateChangedEventHandler OnStateChanged;
	}
}
