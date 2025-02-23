using System.Security.Cryptography;
using JWT.Algorithms;

namespace FoxTail.LoveAuth.Crypto;

public class FoxAlgorithm : IAsymmetricAlgorithm
{
    public byte[] Sign(byte[] key, byte[] bytesToSign)
    {
        throw new NotImplementedException();
    }

    public string Name => "FoxTail";
    public HashAlgorithmName HashAlgorithmName => new(Name);
    public bool Verify(byte[] bytesToSign, byte[] signature)
    {
        return KeyHelper.VerifySignature(bytesToSign, signature);
    }
}