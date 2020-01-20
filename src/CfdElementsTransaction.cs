using System;
using System.Runtime.InteropServices;

namespace Cfd
{
  public class BlindFactor
  {
    private hex_data;

    public BlindFactor() {
      hex_data = "0000000000000000000000000000000000000000000000000000000000000000";
    }

    public BlindFactor(string blind_factor_hex) {
      hex_data = blind_factor_hex;
    }

    public string ToHexString() {
      return hex_data;
    }
  }

  public struct AssetValueData {
    string asset;
    long satoshi_value;
    BlindFactor asset_blind_factor;
    BlindFactor amount_blind_factor;
  }

  public struct UnblindIssuanceData {
    AssetValueData asset_data;
    AssetValueData token_data;
  }

  public struct IssuanceKeys {
    Privkey asset_key;
    Privkey token_key;
  }

  public class ConfidentialTransaction
  {
    private string tx;
    private string last_getinfo_tx = "";
    private string txid = "";
    private string wtxid = "";
    private string wit_hash = "";
    private uint tx_size = 0;
    private uint tx_vsize = 0;
    private uint tx_weight = 0;
    private uint tx_version = 0;
    private uint tx_locktime = 0;

    public ConfidentialTransaction(uint version, uint locktime) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdInitializeConfidentialTx(
          handle.GetHandle(),
          version,
          locktime,
          out IntPtr tx_string);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);
      }
    }

    public ConfidentialTransaction(string tx_hex) {
      tx = tx_hex;
    }

    public void AddTxIn(string txid, uint vout, uint sequence) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdAddConfidentialTxIn(
          handle.GetHandle(), tx, txid, vout, sequence, out IntPtr tx_string);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);
      }
    }

    public void AddTxOut(string asset, long satoshi_value, string address) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdAddConfidentialTxOut(
            handle.GetHandle(), tx,
            asset,
            satoshi_value,
            "",
            address,
            "",
            "",
            out IntPtr tx_string);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);
      }
    }

    public void AddTxOut(string asset, string value_commitment, string address) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdAddConfidentialTxOut(
            handle.GetHandle(), tx,
            asset,
            (long)0,
            value_commitment,
            address,
            "",
            "",
            out IntPtr tx_string);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);
      }
    }

    public void AddFeeTxOut(long satoshi_value) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdAddConfidentialTxOut(
            handle.GetHandle(), tx,
            "",
            satoshi_value,
            "",
            "",
            "",
            "",
            out IntPtr tx_string);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);
      }
    }

    public void AddDestroyAmountTxOut(string asset, long satoshi_value) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdAddConfidentialTxOut(
            handle.GetHandle(), tx,
            asset,
            satoshi_value,
            "",
            "",
            "6a",  // OP_RETURN
            "",
            out IntPtr tx_string);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);
      }
    }

    public void BlindTxOut(IDictionary<OutPoint, AssetValueData> utxos,
        IDictionary<uint, Pubkey> confidential_keys) {
      BlindTransaction(utxos, new Dictionary<OutPoint, IssuanceKeys>(),
          confidential_keys)
    }

    public void BlindTransaction(IDictionary<OutPoint, AssetValueData> utxos,
        IDictionary<OutPoint, IssuanceKeys> issuance_keys,
        IDictionary<uint, Pubkey> confidential_keys) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdInitializeBlindTx(
          handle.GetHandle(), out IntPtr blind_handle);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        try {
          foreach (var outpoint in utxos.Keys) {
            AssetValueData data = utxos[outpoint];
            string asset_key = "";
            string token_key = "";
            if (issuance_keys.ContainsKey(outpoint)) {
              IssuanceKeys keys = issuance_keys[outpoint];
              asset_key = keys.asset_key.ToHexString();
              token_key = keys.token_key.ToHexString();
            }

            ret = CElementsTransaction.CfdAddBlindTxInData(
              handle.GetHandle(), blind_handle,
              outpoint.GetTxid(), outpoint.GetVout(),
              data.asset,
              data.asset_blind_factor,
              data.amount_blind_factor,
              data.satoshi_value,
              asset_key,
              token_key);
            if (ret != CfdErrorCode.Success) {
              CUtil.ThrowError(handle, ret);
            }
          }

          foreach (var index in confidential_keys.Keys) {
            ret = CElementsTransaction.CfdAddBlindTxOutData(
              handle.GetHandle(), blind_handle,
              index, confidential_keys[index]);
            if (ret != CfdErrorCode.Success) {
              CUtil.ThrowError(handle, ret);
            }
          }

          ret = CElementsTransaction.CfdFinalizeBlindTx(
            handle.GetHandle(), blind_handle, tx,
            out IntPtr tx_hex_string);
          script_items[index] = CUtil.ConvertToString(script_item);
          if (ret != CfdErrorCode.Success) {
            CUtil.ThrowError(handle, ret);
          }
          tx = CUtil.ConvertToString(tx_hex_string);
        } finally {
          CElementsTransaction.CfdFreeBlindHandle(handle.GetHandle(), blind_handle);
        }
      }
    }

    public AssetValueData UnblindTxOut(uint txout_index, Privkey blinding_key) {
      AssetValueData result;
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdUnblindTxOut(
            handle.GetHandle(), tx,
            txout_index,
            blinding_key.ToHexString(),
            out IntPtr asset,
            out long value,
            out IntPtr asset_blind_factor,
            out IntPtr amount_blind_factor);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);

        string abf = CUtil.ConvertToString(asset_blind_factor);
        string vbf = CUtil.ConvertToString(amount_blind_factor);
        result.asset = CUtil.ConvertToString(asset);
        result.satoshi_value = value;
        result.asset_blind_factor = new BlindFactor(abf);
        result.amount_blind_factor = new BlindFactor(vbf);
      }
      return result;
    }

    public UnblindIssuanceData UnblindIssuance(uint txin_index, Privkey asset_blinding_key, Privkey token_blinding_key) {
      UnblindIssuanceData result;
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdUnblindIssuance(
            handle.GetHandle(), tx,
            txin_index,
            asset_blinding_key.ToHexString(),
            token_blinding_key.ToHexString(),
            out IntPtr asset,
            out long asset_value,
            out IntPtr asset_blind_factor,
            out IntPtr asset_amount_blind_factor,
            out IntPtr token,
            out long token_value,
            out IntPtr token_blind_factor,
            out IntPtr token_amount_blind_factor);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        tx = CUtil.ConvertToString(tx_string);

        string asset_abf = CUtil.ConvertToString(asset_blind_factor);
        string asset_vbf = CUtil.ConvertToString(asset_amount_blind_factor);
        string token_abf = CUtil.ConvertToString(token_blind_factor);
        string token_vbf = CUtil.ConvertToString(token_blind_factor);
        result.asset_data.asset = CUtil.ConvertToString(asset);
        result.token_data.asset = CUtil.ConvertToString(token);

        result.asset_data.satoshi_value = asset_value;
        result.asset_data.asset_blind_factor = new BlindFactor(asset_abf);
        result.asset_data.amount_blind_factor = new BlindFactor(asset_vbf);
        result.token_data.satoshi_value = token_value;
        result.token_data.asset_blind_factor = new BlindFactor(token_abf);
        result.token_data.amount_blind_factor = new BlindFactor(token_vbf);
      }
      return result;
    }

    public void GetSignatureHash() {
      // FIXME implement
    }

    public void AddSign() {
      // FIXME implement
    }

    public void AddMultisigSign() {
      // FIXME implement
    }

    public void VerifySignature() {
      // FIXME implement
    }

    /*
    public TxIn GetTxIn() {
      // witness, issuanceも
    }

    public TxIn GetTxIns() {
      // witness, issuanceも
    }

    public TxOut GetTxOut() {
    }

    public TxOut GetTxOuts() {
    }
    */

    public string GetHexString() {
      return tx;
    }

    public string GetTxid() {
      UpdateTxInfoCache();
      return tx;
    }

    public string GetWtxid() {
      UpdateTxInfoCache();
      return wtxid;
    }

    public string GetWitHash() {
      UpdateTxInfoCache();
      return wit_hash;
    }

    public uint GetSize() {
      UpdateTxInfoCache();
      return tx_size;
    }

    public uint GetVsize() {
      UpdateTxInfoCache();
      return tx_vsize;
    }

    public uint GetWeight() {
      UpdateTxInfoCache();
      return tx_weight;
    }

    public uint GetVersion() {
      UpdateTxInfoCache();
      return tx_version;
    }

    public uint GetLockTime() {
      UpdateTxInfoCache();
      return tx_locktime;
    }

    public static Privkey GetIssuanceBlindingKey(
        Privkey master_blinding_key, string txid, uint vout) {
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdGetIssuanceBlindingKey(
            handle.GetHandle(),
            master_blinding_key.ToHexString(),
            txid,
            vout,
            out IntPtr blinding_key);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        string key = CUtil.ConvertToString(blinding_key);
        return new Privkey(key);
      }
    }

    private void UpdateTxInfoCache() {
      if (tx != last_getinfo_tx) {
        return;
      }
      using(ErrorHandle handle = new ErrorHandle())
      {
        CfdErrorCode ret = CElementsTransaction.CfdGetConfidentialTxInfo(
            handle.GetHandle(),
            tx,
            out IntPtr out_txid,
            out IntPtr out_wtxid,
            out IntPtr out_wit_hash,
            out uint out_size,
            out uint out_vsize,
            out uint out_weight,
            out uint out_version,
            out uint out_locktime);
        if (ret != CfdErrorCode.Success) {
          CUtil.ThrowError(handle, ret);
        }
        txid = CUtil.ConvertToString(out_txid);
        wtxid = CUtil.ConvertToString(out_wtxid);
        wit_hash = CUtil.ConvertToString(out_wit_hash);
        tx_size = out_size;
        tx_vsize = out_vsize;
        tx_weight = out_weight;
        tx_version = out_version;
        tx_locktime = out_locktime;
        last_getinfo_tx = tx;
      }
    }
  }

  internal class CElementsTransaction
  {
    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdInitializeConfidentialTx(
        [In] IntPtr handle,
        [In] uint version,
        [In] uint locktime,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdAddConfidentialTxIn(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] string txid,
        [In] uint vout,
        [In] uint sequence,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdAddConfidentialTxOut(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] string asset_string,
        [In] long value_satoshi,
        [In] string value_commitment,
        [In] string address,
        [In] string direct_locking_script,
        [In] string nonce,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdUpdateConfidentialTxOut(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint index,
        [In] string asset_string,
        [In] long value_satoshi,
        [In] string value_commitment,
        [In] string address,
        [In] string direct_locking_script,
        [In] string nonce,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetConfidentialTxInfo(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [Out] out IntPtr txid,
        [Out] out IntPtr wtxid,
        [Out] out IntPtr wit_hash,
        [Out] out uint size,
        [Out] out uint vsize,
        [Out] out uint weight,
        [Out] out uint version,
        [Out] out uint locktime);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetConfidentialTxIn(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint index,
        [Out] out IntPtr txid,
        [Out] out uint vout,
        [Out] out uint sequence,
        [Out] out IntPtr script_sig);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetConfidentialTxInWitness(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint txin_index,
        [In] uint stack_index,
        [Out] out IntPtr stack_data);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetTxInIssuanceInfo(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint index,
        [Out] out IntPtr entropy,
        [Out] out IntPtr nonce,
        [Out] out long asset_amount,
        [Out] out IntPtr asset_value,
        [Out] out long token_amount,
        [Out] out IntPtr token_value,
        [Out] out IntPtr asset_rangeproof,
        [Out] out IntPtr token_rangeproof);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetConfidentialTxOut(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint index,
        [Out] out IntPtr asset_string,
        [Out] out long value_satoshi,
        [Out] out IntPtr value_commitment,
        [Out] out IntPtr nonce,
        [Out] out IntPtr locking_script,
        [Out] out IntPtr surjection_proof,
        [Out] out IntPtr rangeproof);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetConfidentialTxInCount(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [Out] out uint count);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetConfidentialTxInWitnessCount(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint txin_index,
        [Out] out uint count);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetConfidentialTxOutCount(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [Out] out uint count);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdSetRawReissueAsset(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] string txid,
        [In] uint vout,
        [In] long asset_amount,
        [In] string blinding_nonce,
        [In] string entropy,
        [In] string address,
        [In] string direct_locking_script,
        [Out] out IntPtr asset_string,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdGetIssuanceBlindingKey(
        [In] IntPtr handle,
        [In] string master_blinding_key,
        [In] string txid,
        [In] uint vout,
        [Out] out IntPtr blinding_key);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdInitializeBlindTx(
        [In] IntPtr handle,
        [Out] out IntPtr blind_handle);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdAddBlindTxInData(
        [In] IntPtr handle,
        [In] IntPtr blind_handle,
        [In] string txid,
        [In] uint vout,
        [In] string asset_string,
        [In] string asset_blind_factor,
        [In] string amount_blind_factor,
        [In] long value_satoshi,
        [In] string asset_key,
        [In] string token_key);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdAddBlindTxOutData(
        [In] IntPtr handle,
        [In] IntPtr blind_handle,
        [In] uint index,
        [In] string confidential_key);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdFinalizeBlindTx(
        [In] IntPtr handle,
        [In] IntPtr blind_handle,
        [In] string tx_hex_string,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdFreeBlindHandle(
        [In] IntPtr handle,
        [In] IntPtr blind_handle);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdAddConfidentialTxSign(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] string txid,
        [In] uint vout,
        [In] bool is_witness,
        [In] string sign_data_hex,
        [In] bool clear_stack,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdAddConfidentialTxDerSign(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] string txid,
        [In] uint vout,
        [In] bool is_witness,
        [In] string signature,
        [In] int sighash_type,
        [In] bool sighash_anyone_can_pay,
        [In] bool clear_stack,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdFinalizeElementsMultisigSign(
        [In] IntPtr handle,
        [In] IntPtr multisign_handle,
        [In] string tx_hex_string,
        [In] string txid,
        [In] uint vout,
        [In] int hash_type,
        [In] string witness_script,
        [In] string redeem_script,
        [In] bool clear_stack,
        [Out] out IntPtr tx_string);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdCreateConfidentialSighash(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] string txid,
        [In] uint vout,
        [In] int hash_type,
        [In] string pubkey,
        [In] string redeem_script,
        [In] long value_satoshi,
        [In] string value_commitment,
        [In] int sighash_type,
        [In] bool sighash_anyone_can_pay,
        [Out] out IntPtr sighash);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdUnblindTxOut(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint tx_out_index,
        [In] string blinding_key,
        [Out] out IntPtr asset,
        [Out] out long value,
        [Out] out IntPtr asset_blind_factor,
        [Out] out IntPtr amount_blind_factor);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdUnblindIssuance(
        [In] IntPtr handle,
        [In] string tx_hex_string,
        [In] uint tx_in_index,
        [In] string asset_blinding_key,
        [In] string token_blinding_key,
        [Out] out IntPtr asset,
        [Out] out long asset_value,
        [Out] out IntPtr asset_blind_factor,
        [Out] out IntPtr asset_amount_blind_factor,
        [Out] out IntPtr token,
        [Out] out long token_value,
        [Out] out IntPtr token_blind_factor,
        [Out] out IntPtr token_amount_blind_factor);

    [DllImport("cfd", CallingConvention = CallingConvention.StdCall)]
    internal static extern CfdErrorCode CfdVerifyConfidentialTxSignature(
        [In] IntPtr handle,
        [In] string tx_hex,
        [In] string signature,
        [In] string pubkey,
        [In] string script,
        [In] string txid,
        [In] uint vout,
        [In] int sighash_type,
        [In] bool sighash_anyone_can_pay,
        [In] long value_satoshi,
        [In] string value_commitment,
        [In] int witness_version);
  }
}
