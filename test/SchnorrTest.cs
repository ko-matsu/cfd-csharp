using Xunit;

namespace Cfd.xTests
{
  public class SchnorrTest
  {
    [Fact]
    public void AdaptorUtilTest()
    {
      var msg = new ByteData("024bdd11f2144e825db05759bdd9041367a420fad14b665fd08af5b42056e5e2");
      var adaptor = new Pubkey("038d48057fc4ce150482114d43201b333bf3706f3cd527e8767ceb4b443ab5d349");
      var sk = new Privkey("90ac0d5dc0a1a9ab352afb02005a5cc6c4df0da61d8149d729ff50db9b5a5215");
      var pubkey = new Pubkey("03490cec9a53cd8f2f664aea61922f26ee920c42d2489778bb7c9d9ece44d149a7");
      var adaptorSig = new ByteData("00cbe0859638c3600ea1872ed7a55b8182a251969f59d7d2da6bd4afedf25f5021a49956234cbbbbede8ca72e0113319c84921bf1224897a6abd89dc96b9c5b208");
      var adaptorProof = new ByteData("00b02472be1ba09f5675488e841a10878b38c798ca63eff3650c8e311e3e2ebe2e3b6fee5654580a91cc5149a71bf25bcbeae63dea3ac5ad157a0ab7373c3011d0fc2592a07f719c5fc1323f935569ecd010db62f045e965cc1d564eb42cce8d6d");
      var adaptorSig2 = new ByteData("01099c91aa1fe7f25c41085c1d3c9e73fe04a9d24dac3f9c2172d6198628e57f47bb90e2ad6630900b69f55674c8ad74a419e6ce113c10a21a79345a6e47bc74c1");
      // var sigDer = new ByteData("30440220099c91aa1fe7f25c41085c1d3c9e73fe04a9d24dac3f9c2172d6198628e57f4702204d13456e98d8989043fd4674302ce90c432e2f8bb0269f02c72aafec60b72de101");
      var sig = new ByteData("099c91aa1fe7f25c41085c1d3c9e73fe04a9d24dac3f9c2172d6198628e57f474d13456e98d8989043fd4674302ce90c432e2f8bb0269f02c72aafec60b72de1");
      var secret = new Privkey("475697a71a74ff3f2a8f150534e9b67d4b0b6561fab86fcaa51f8c9d6c9db8c6");

      var pair = EcdsaAdaptorUtil.Sign(msg, sk, adaptor);
      Assert.Equal(adaptorSig.ToHexString(), pair.Signature.ToHexString());
      Assert.Equal(adaptorProof.ToHexString(), pair.Proof.ToHexString());

      var isVerify = EcdsaAdaptorUtil.Verify(pair.Signature, pair.Proof, adaptor, msg, pubkey);
      Assert.True(isVerify);

      var adaptSig = EcdsaAdaptorUtil.Adapt(adaptorSig2, secret);
      Assert.Equal(sig.ToHexString(), adaptSig.ToHexString());

      var adaptorSecret = EcdsaAdaptorUtil.ExtractSecret(adaptorSig2, adaptSig, adaptor);
      Assert.Equal(secret.ToHexString(), adaptorSecret.ToHexString());
    }

    [Fact]
    public void SchnorrUtilTest()
    {
      var msg = new ByteData("e48441762fb75010b2aa31a512b62b4148aa3fb08eb0765d76b252559064a614");
      var sk = new Privkey("688c77bc2d5aaff5491cf309d4753b732135470d05b7b2cd21add0744fe97bef");
      var pubkey = new Pubkey("02b33cc9edc096d0a83416964bd3c6247b8fecd256e4efa7870d2c854bdeb33390");
      var auxRand = new ByteData("02cce08e913f22a36c5648d6405a2c7c50106e7aa2f1649e381c7f09d16b80ab");
      var nonce = new ByteData("8c8ca771d3c25eb38de7401818eeda281ac5446f5c1396148f8d9d67592440fe");
      var schnorrNonce = new ByteData("f14d7e54ff58c5d019ce9986be4a0e8b7d643bd08ef2cdf1099e1a457865b547");
      var signature = new SchnorrSignature("f14d7e54ff58c5d019ce9986be4a0e8b7d643bd08ef2cdf1099e1a457865b5477c988c51634a8dc955950a58ff5dc8c506ddb796121e6675946312680c26cf33");

      var sig = SchnorrUtil.Sign(msg, sk, auxRand);
      Assert.Equal(signature.ToHexString(), sig.ToHexString());

      var expectedSig =
          "5da618c1936ec728e5ccff29207f1680dcf4146370bdcfab0039951b91e3637a50a2a860b130d009405511c3eafe943e157a0df2c2020e3e50df05adb175332f";
      var sig2 = SchnorrUtil.SignWithNonce(msg, sk, nonce);
      Assert.Equal(expectedSig, sig2.ToHexString());

      string expectedSigPoint =
          "020d17280b8d2c2bd3b597b4446419c151dc237353d0fb9ec03d4eb7e8de7ee0a8";
      var sigPoint = SchnorrUtil.ComputeSigPoint(msg, schnorrNonce, pubkey);
      Assert.Equal(expectedSigPoint, sigPoint.ToHexString());

      var isVerify = SchnorrUtil.Verify(signature, msg, pubkey);
      Assert.True(isVerify);

      var expectedNonce =
          "f14d7e54ff58c5d019ce9986be4a0e8b7d643bd08ef2cdf1099e1a457865b547";
      var expectedPrivkey =
          "7c988c51634a8dc955950a58ff5dc8c506ddb796121e6675946312680c26cf33";
      Assert.Equal(expectedNonce, sig.GetNonce().ToHexString());
      Assert.Equal(expectedPrivkey, sig.GetKey().ToHexString());
    }
  }
}
