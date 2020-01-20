using System;
using System.Runtime.InteropServices;

namespace Cfd
{
  //! txin sequence locktime
  public enum CfdSequenceLockTime : uint {
    /// disable locktime
    Disable = 0xffffffff,
    /// enable locktime (maximum time)
    EnableMax = 0xfffffffe,
  };

  public class OutPoint {
    private string txid;
    private uint vout;

    public OutPoint() {
    }

    public OutPoint(string txid, uint vout) {
      this.txid = txid;
      this.vout = vout;
    }

    public string GetTxid() {
      return txid;
    }

    public uint GetVout() {
      return vout;
    }

    public override int GetHashCode() {
      return txid.GetHashCode() + vout.GetHashCode();
    }

    public override bool Equals(object obj) {
      var item = obj as OutPoint;
      if(item == null) {
        return false;
      }
      return (txid == item.txid) && (item.vout == item.vout);
    }

    public static bool operator ==(OutPoint src, OutPoint dst) {
        if (src is null || dst is null) {
            return false;
        }
        return src.Equals(dst);
    }

    public static bool operator !=(OutPoint src, OutPoint dst) {
        return !(src == dst);
    }
  };

/*
  internal class CTransaction
  {
    CFDC_API int CfdInitializeMultisigSign(void* handle, void** multisign_handle);

    CFDC_API int CfdAddMultisigSignData(
        void* handle, void* multisign_handle, const char* signature,
        const char* related_pubkey);

    CFDC_API int CfdAddMultisigSignDataToDer(
        void* handle, void* multisign_handle, const char* signature,
        int sighash_type, bool sighash_anyone_can_pay, const char* related_pubkey);

    CFDC_API int CfdFreeMultisigSignHandle(void* handle, void* multisign_handle);
  }
*/
}
