/// <summary>
/// cfd library namespace.
/// </summary>
namespace Cfd
{
  /**
   * @brief network type
   */
  public enum CfdNetworkType
  {
    Mainnet = 0,      //!< btc mainnet
    Testnet,          //!< btc testnet
    Regtest,          //!< btc regtest
    Liquidv1 = 10,    //!< liquidv1
    ElementsRegtest,  //!< elements regtest
    CustomChain,      //!< elements custom chain
  };

  /**
   * @brief address type
   */
  public enum CfdAddressType
  {
    P2sh = 1,   //!< Legacy address (Script Hash)
    P2pkh,      //!< Legacy address (PublicKey Hash)
    P2wsh,      //!< Native segwit address (Script Hash)
    P2wpkh,     //!< Native segwit address (PublicKey Hash)
    P2shP2wsh,  //!< P2sh wrapped address (Script Hash)
    P2shP2wpkh  //!< P2sh wrapped address (Pubkey Hash)
  };

  /**
   * @brief hash type
   */
  public enum CfdHashType
  {
    P2sh = 1,   //!< Script Hash
    P2pkh,      //!< PublicKey Hash
    P2wsh,      //!< Native segwit Script Hash
    P2wpkh,     //!< Native segwit PublicKey Hash
    P2shP2wsh,  //!< P2sh wrapped segwit Script Hash
    P2shP2wpkh  //!< P2sh wrapped segwit Pubkey Hash
  };

  /**
   * @brief witness version
   */
  public enum CfdWitnessVersion
  {
    VersionNone = -1,  //!< Missing WitnessVersion
    Version0 = 0,      //!< version 0
    Version1,          //!< version 1 (for future use)
    Version2,          //!< version 2 (for future use)
    Version3,          //!< version 3 (for future use)
    Version4,          //!< version 4 (for future use)
    Version5,          //!< version 5 (for future use)
    Version6,          //!< version 6 (for future use)
    Version7,          //!< version 7 (for future use)
    Version8,          //!< version 8 (for future use)
    Version9,          //!< version 9 (for future use)
    Version10,         //!< version 10 (for future use)
    Version11,         //!< version 11 (for future use)
    Version12,         //!< version 12 (for future use)
    Version13,         //!< version 13 (for future use)
    Version14,         //!< version 14 (for future use)
    Version15,         //!< version 15 (for future use)
    Version16          //!< version 16 (for future use)
  };

  /**
   * @brief sighash type
   */
  public enum CfdSighashType
  {
    All = 0x01,    //!< SIGHASH_ALL
    None = 0x02,   //!< SIGHASH_NONE
    Single = 0x03  //!< SIGHASH_SINGLE
  };

  /* for cfd cg-v0.0.11 or p2pderivatives-v0.0.4
public class Address
{
  private readonly string address;
  private readonly string lockingScript;
  private readonly string p2shSegwitLockingScript;
  private readonly string hash;
  private readonly CfdNetworkType network;
  private readonly CfdAddressType addressType;
  private readonly CfdWitnessVersion witnessVersion;

  public Address()
  {
  }

  public Address(string addressString)
  {
    using (var handle = new ErrorHandle())
    {
      Initialize(handle, addressString);
      address = addressString;
    }
  }

  public Address(Pubkey pubkey, CfdAddressType type, CfdNetworkType network)
  {
    using (var handle = new ErrorHandle())
    {
      var ret = CAddress.CfdCreateAddress(
        handle.GetHandle(),
        (int)type,
        pubkey.ToHexString(),
        "",
        (int)network,
        out IntPtr outputAddress,
        out IntPtr outputLockingScript,
        out IntPtr outputP2shSegwitLockingScript);
      if (ret != CfdErrorCode.Success)
      {
        handle.ThrowError(ret);
      }
      address = CCommon.ConvertToString(outputAddress);
      lockingScript = CCommon.ConvertToString(outputLockingScript);
      p2shSegwitLockingScript = CCommon.ConvertToString(outputP2shSegwitLockingScript);

      Initialize(handle, address);
      addressType = type;
    }
  }

  public Address(Script script, CfdAddressType type, CfdNetworkType network)
  {
    using (var handle = new ErrorHandle())
    {
      var ret = CAddress.CfdCreateAddress(
        handle.GetHandle(),
        (int)type,
        "",
        script.ToHexString(),
        (int)network,
        out IntPtr outputAddress,
        out IntPtr outputLockingScript,
        out IntPtr outputP2shSegwitLockingScript);
      if (ret != CfdErrorCode.Success)
      {
        handle.ThrowError(ret);
      }
      address = CCommon.ConvertToString(outputAddress);
      lockingScript = CCommon.ConvertToString(outputLockingScript);
      p2shSegwitLockingScript = CCommon.ConvertToString(outputP2shSegwitLockingScript);

      Initialize(handle, address);
      addressType = type;
    }
  }

  public static Address GetAddressByLockingScript(Script inputLockingScript, CfdNetworkType network)
  {
    string address = "";
    using (var handle = new ErrorHandle())
    {
      var ret = CAddress.CfdGetAddressFromLockingScript(
        handle.GetHandle(),
        inputLockingScript.ToHexString(),
        (int)network,
        out IntPtr outputAddress);
      if (ret != CfdErrorCode.Success)
      {
        handle.ThrowError(ret);
      }
      address = CCommon.ConvertToString(outputAddress);
    }
    return new Address(address);
  }

  public string ToAddressString()
  {
    return address;
  }

  public string GetLockingScript()
  {
    return lockingScript;
  }

  public string GetHash()
  {
    return hash;
  }

  public string GetP2shLockingScript()
  {
    return p2shSegwitLockingScript;
  }

  public CfdNetworkType GetNetwork()
  {
    return network;
  }

  public CfdAddressType GetAddressType()
  {
    return addressType;
  }

  public CfdWitnessVersion GetWitnessVersion()
  {
    return witnessVersion;
  }

  public void SetAddressType(CfdAddressType addrType)
  {
    addressType = addrType;
  }

  private void Initialize(ErrorHandle handle, string addressString)
  {
      var ret = CAddress.CfdGetAddressInfo(
        handle.GetHandle(),
        addressString,
        out int networkType,
        out int hashType,
        out int segwitVersion,
        out IntPtr outputLockingScript,
        out IntPtr hashString);
      if (ret != CfdErrorCode.Success)
      {
        handle.ThrowError(ret);
      }
      network = (CfdNetworkType)networkType;
      addressType = (CfdAddressType)hashType;
      witnessVersion = (CfdWitnessVersion)segwitVersion;
      lockingScript = CCommon.ConvertToString(outputLockingScript);
      hash = CCommon.ConvertToString(hashString);
  }

}
*/
}
