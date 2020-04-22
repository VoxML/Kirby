using UnityEngine;
using System;

using VoxSimPlatform.Global;
using VoxSimPlatform.Network;

public enum RedisEventType
{
    Command,
    Response
}

public class RedisEventArgs : EventArgs
{
    public RedisEventType Type { get; set; }
    public string Content { get; set; }

    public RedisEventArgs(RedisEventType type, string content)
    {
        this.Type = type;
        this.Content = content;
    }
}

public class RedisSocket : SocketConnection
{
    public event EventHandler UpdateReceived;

    public void OnUpdateReceived(object sender, EventArgs e)
    {
        if (UpdateReceived != null)
        {
            UpdateReceived(this, e);
        }
    }

    public RedisSocket()
    {
        IOClientType = typeof(RedisIOClient);
    }

    public void Write(byte[] content)
    {
        // Check to see if this NetworkStream is writable.
        if (_client.GetStream().CanWrite)
        {
            byte[] writeBuffer = content;
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(writeBuffer);
            }

            _client.GetStream().Write(writeBuffer, 0, writeBuffer.Length);
            Debug.Log(string.Format("Written to this NetworkStream: {0} ({1})", writeBuffer.Length,
                GlobalHelper.PrintByteArray(writeBuffer)));
        }
        else
        {
            Debug.Log("Sorry.  You cannot write to this NetworkStream.");
        }
    }
}
