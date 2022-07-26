using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace WinHue.LED
{
    internal sealed partial class OpcDriver : IDisposable
    {
        public IPixelBatcher GetPixelBatcher(byte channel = 0)
        {
            return new PixelBatcher(this, channel);
        }

        public void Start()
        {
            if (mDisposed) throw new ObjectDisposedException(nameof(OpcDriver));
            if (mStarted) return;
            mStarted = true;

            OnRemoteAddressChanged();
            OnListenPortChanged();
        }

        public void Stop()
        {
            if (mDisposed) throw new ObjectDisposedException(nameof(OpcDriver));
            if (!mStarted) return;
            mStarted = false;
            mRemoteClient.Dispose();
            mRemoteClient = new();
            mListener?.Stop();
            mListener = null;
            lock (mClients)
            {
                foreach (var client in mClients)
                {
                    client.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (mDisposed) return;
            Stop();
            mDisposed = true;
        }

        public string? RemoteAddress
        {
            get => mRemoteAddress;
            set
            {
                if (mRemoteAddress == value) return;
                mRemoteAddress = value;
                OnRemoteAddressChanged();
            }
        }

        public ushort? ListenPort
        {
            get => mListenPort;
            set
            {
                if (mListenPort == value) return;
                mListenPort = value;
                OnListenPortChanged();
            }
        }

        private void OnRemoteAddressChanged()
        {
            if (mDisposed) throw new ObjectDisposedException(nameof(OpcDriver));
            if (!mStarted) return;
            mRemoteClient.Close();

            if (mRemoteAddress == null) return;

            var parts = mRemoteAddress.Split(':');

            if (parts.Length != 2)
            {
                throw new InvalidOperationException("Remote address must specify a port.");
            }

            mRemoteClient = new TcpClient();
            mRemoteClient.BeginConnect(parts[0], int.Parse(parts[1]), (result) =>
            {
                if (result.AsyncState is not TcpClient client) throw new ArgumentNullException();
                client.EndConnect(result);
            }, mRemoteClient);
        }

        private void OnListenPortChanged()
        {
            if (mDisposed) throw new ObjectDisposedException(nameof(OpcDriver));
            if (!mStarted) return;
            mListener?.Stop();

            if (mListenPort == null) return;

            mListener = new TcpListener(IPAddress.Any, (int)mListenPort);
            mListener.Start();
            mListener.BeginAcceptTcpClient((result) =>
            {
                if (result.AsyncState is not TcpListener listener) throw new ArgumentNullException();
                var client = listener.EndAcceptTcpClient(result);
                lock (mClients)
                {
                    mClients.Add(client);
                }
            }, mListener);
        }

        private void Send(Span<byte> data)
        {
            if (mRemoteClient.Connected)
            {
                mRemoteClient.GetStream().Write(data);
            }

            lock (mClients)
            {
                mClients.RemoveAll((client) => !client.Connected);
                foreach (var client in mClients)
                {
                    client.GetStream().Write(data);
                }
            }
        }

        private string? mRemoteAddress;
        private ushort? mListenPort;

        private TcpClient mRemoteClient = new();
        private TcpListener? mListener;
        private readonly List<TcpClient> mClients = new();

        private bool mStarted = false;
        private bool mDisposed = false;
    }
}
