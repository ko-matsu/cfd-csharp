using System;

namespace Cfd
{
  /// <summary>
  /// Schnorr public key class.
  /// </summary>
  public class SchnorrPubkey : IEquatable<SchnorrPubkey>
  {
    public static readonly uint Size = 32;
    private readonly string data;

    /// <summary>
    /// Constructor. (empty)
    /// </summary>
    public SchnorrPubkey()
    {
      data = "";
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="bytes">byte array</param>
    public SchnorrPubkey(byte[] bytes)
    {
      if ((bytes == null) || (bytes.Length != Size))
      {
        CfdCommon.ThrowError(CfdErrorCode.IllegalArgumentError, "Failed to pubkey size.");
      }
      data = StringUtil.FromBytes(bytes);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="hex">hex string</param>
    public SchnorrPubkey(string hex)
    {
      if ((hex == null) || (hex.Length != Size * 2))
      {
        CfdCommon.ThrowError(CfdErrorCode.IllegalArgumentError, "Failed to pubkey size.");
      }
      data = hex;
    }

    /// <summary>
    /// Get Schnorr public key from private key.
    /// </summary>
    /// <param name="privkey">private key</param>
    /// <param name="parity">parity flag</param>
    public static SchnorrPubkey GetPubkeyFromPrivkey(Privkey privkey, out bool parity)
    {
      if (privkey is null)
      {
        throw new ArgumentNullException(nameof(privkey));
      }
      using (var handle = new ErrorHandle())
      {
        var ret = NativeMethods.CfdGetSchnorrPubkeyFromPrivkey(
            handle.GetHandle(), privkey.ToHexString(),
            out IntPtr pubkey, out parity);
        if (ret != CfdErrorCode.Success)
        {
          handle.ThrowError(ret);
        }
        string data = CCommon.ConvertToString(pubkey);
        return new SchnorrPubkey(data);
      }
    }

    /// <summary>
    /// hex string.
    /// </summary>
    /// <returns>hex string</returns>
    public string ToHexString()
    {
      return data;
    }

    /// <summary>
    /// byte array.
    /// </summary>
    /// <returns>byte array</returns>
    public byte[] ToBytes()
    {
      return StringUtil.ToBytes(data);
    }

    public bool IsValid()
    {
      return data.Length == Size;
    }

    public bool Equals(SchnorrPubkey other)
    {
      if (other is null)
      {
        return false;
      }
      if (ReferenceEquals(this, other))
      {
        return true;
      }
      return data.Equals(other.data, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
      if (obj is null)
      {
        return false;
      }
      if ((obj as SchnorrPubkey) != null)
      {
        return Equals((SchnorrPubkey)obj);
      }
      return false;
    }

    public override int GetHashCode()
    {
      return data.GetHashCode(StringComparison.Ordinal);
    }

    public static bool operator ==(SchnorrPubkey lhs, SchnorrPubkey rhs)
    {
      if (lhs is null)
      {
        if (rhs is null)
        {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(SchnorrPubkey lhs, SchnorrPubkey rhs)
    {
      return !(lhs == rhs);
    }
  }
}
