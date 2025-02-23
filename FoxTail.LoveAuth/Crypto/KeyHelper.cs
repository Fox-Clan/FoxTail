using System.Runtime.CompilerServices;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;

namespace FoxTail.LoveAuth.Crypto;

internal static class KeyHelper
{
    private static Ed25519PublicKeyParameters _key;

    internal static void SetKey(string data)
    {
        using StringReader reader = new(data);
        using PemReader pemReader = new(reader);
        
        object? keyObject = pemReader.ReadObject();

        if (keyObject is not Ed25519PublicKeyParameters publicKey)
        {
            throw new Exception("Key is not Ed25519, got " + keyObject.GetType().Name);
        }

        _key = publicKey;
    }

    private static bool VerifySignature(Ed25519PublicKeyParameters publicKey, byte[] message, byte[] signature)
    {
        Ed25519Signer verifier = new();
        verifier.Init(false, publicKey);
        verifier.BlockUpdate(message, 0, message.Length);
        return verifier.VerifySignature(signature);
    }

    public static bool VerifySignature(byte[] message, byte[] signature)
    {
        return VerifySignature(_key, message, signature);
    }
}