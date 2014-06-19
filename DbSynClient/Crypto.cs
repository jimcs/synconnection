using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DbSynClient
{
    public static class Crypto
    {
        /// <summary>
        /// 编码格式
        /// </summary>
        public static readonly Encoding Encoding = Encoding.UTF8;
        public static class AES
        {
            #region 配置

            /// <summary>
            /// 加密模式
            /// </summary>
            private const CipherMode cipherMode = CipherMode.CBC;
            /// <summary>
            /// 填充模式
            /// </summary>
            private const PaddingMode paddingMode = PaddingMode.PKCS7;

            /// <summary>
            /// 默认密钥
            /// </summary>
            private static readonly byte[] appkeykey = Encoding.GetBytes("DBA0DB7D82DB4A68A632E0AA3192E57F");
            /// <summary>
            /// 密钥长度非标准时用此密钥对应字节来填充
            /// </summary>
            private static readonly byte[] initkey = Encoding.GetBytes("00000000000000000000000000000000");
            #endregion
            public static string Encrypt(string orgidata, byte[] key = null, CryptoStringFormat format = CryptoStringFormat.Base64, string iv = null)
            {
                if (orgidata == null)
                {
                    return null;
                }
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {
                    key = GetCheckKey(key);
                    aes.Key = key == null ? appkeykey : key;
                    byte[] _iv = new byte[aes.BlockSize / 8];
                    if (string.IsNullOrEmpty(iv))
                    {
                        Array.Copy(aes.Key, 0, _iv, 0, _iv.Length);
                    }
                    else
                    {
                        byte[] pIV = Encoding.GetBytes(iv);
                        if (pIV.Length == _iv.Length) { _iv = pIV; }
                        else if (pIV.Length > _iv.Length)
                        {
                            Array.Copy(pIV, 0, _iv, 0, _iv.Length);
                        }
                        else if (pIV.Length < _iv.Length)
                        {
                            Array.Copy(pIV, 0, _iv, 0, pIV.Length);
                            for (int i = pIV.Length; i < _iv.Length; i++) {
                                _iv[i] = 48;
                            }
                        }
                    }
                    aes.Mode = cipherMode;
                    aes.Padding = paddingMode;
                    aes.IV = _iv;
                    using (ICryptoTransform transform = aes.CreateEncryptor())
                    {
                        byte[] data = null;
                        try
                        {
                            byte[] orgi = Encoding.GetBytes(orgidata);
                            data = transform.TransformFinalBlock(orgi, 0, orgi.Length);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteException(ex);
                        }
                        aes.Clear();
                        if (data != null)
                        {
                            switch (format)
                            {
                                case CryptoStringFormat.Base64:
                                    return Convert.ToBase64String(data);
                                case CryptoStringFormat.Encoding:
                                    return Encoding.GetString(data);
                            }
                        }
                        return null;
                    }
                }
            }
            private static byte[] GetCheckKey(byte[] key)
            {
                if (key != null)
                {
                    byte[] newKey = initkey;
                    if (key.Length == 32) { return key; }
                    else if (key.Length > 32)
                    {
                        Array.Copy(key, 0, newKey, 0, 32);
                    }
                    else if (key.Length < 32)
                    {
                        Array.Copy(key, 0, newKey, 0, key.Length);
                    }
                    return newKey;
                }
                return null;
            }
            public static string Decrypt(string orgidata, byte[] key = null, CryptoStringFormat format = CryptoStringFormat.Base64, string iv = null)
            {
                if (orgidata == null)
                {
                    return null;
                }
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {
                    key = GetCheckKey(key);
                    aes.Key = key == null ? appkeykey : key;
                    byte[] _iv = new byte[aes.BlockSize / 8];
                    if (string.IsNullOrEmpty(iv))
                    {
                        Array.Copy(aes.Key, 0, _iv, 0, _iv.Length);
                    }
                    else
                    {
                        byte[] pIV = Encoding.GetBytes(iv);
                        if (pIV.Length == _iv.Length) { _iv = pIV; }
                        else if (pIV.Length > _iv.Length)
                        {
                            Array.Copy(pIV, 0, _iv, 0, _iv.Length);
                        }
                        else if (pIV.Length < _iv.Length)
                        {
                            Array.Copy(pIV, 0, _iv, 0, pIV.Length);
                            for (int i = pIV.Length; i < _iv.Length; i++)
                            {
                                _iv[i] = 48;
                            }
                        }
                    }
                    aes.Mode = cipherMode;
                    aes.Padding = paddingMode;
                    aes.IV = _iv;
                    using (ICryptoTransform transform = aes.CreateDecryptor())
                    {
                        byte[] data = null;
                        try
                        {
                            byte[] orgi = null;
                            switch (format)
                            {
                                case CryptoStringFormat.Base64:
                                    orgi = Convert.FromBase64String(orgidata);
                                    break;
                                case CryptoStringFormat.Encoding:
                                    orgi = Encoding.GetBytes(orgidata);
                                    break;
                            }
                            data = transform.TransformFinalBlock(orgi, 0, orgi.Length);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteException(ex);
                            return null;
                        }
                        aes.Clear();
                        if (data != null)
                            return Encoding.GetString(data);
                        return null;
                    }
                }
            }
        }
        public static class DES
        {
            /// <summary>
            /// 密钥长度非标准时用此密钥对应字节来填充
            /// </summary>
            private static readonly byte[] initkey = Encoding.GetBytes("00000000");
            /// <summary>
            /// 向量
            /// </summary>
            private static readonly byte[] _iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            private static byte[] GetCheckKey(byte[] key)
            {
                if (key != null)
                {
                    byte[] newKey = initkey;
                    if (key.Length == 8) { return key; }
                    else if (key.Length > 8)
                    {
                        Array.Copy(key, 0, newKey, 0, 8);
                    }
                    else if (key.Length < 8)
                    {
                        Array.Copy(key, 0, newKey, 0, key.Length);
                    }
                    return newKey;
                }
                return null;
            }
            public static string Encrypt(string orgidata, byte[] key = null, CryptoStringFormat format = CryptoStringFormat.Base64, string iv = null)
            {
                if (orgidata == null)
                {
                    return null;
                }
                try
                {
                    byte[] orgi = Encoding.GetBytes(orgidata);
                    using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                    {
                        des.Key = GetCheckKey(key);
                        des.IV = string.IsNullOrEmpty(iv) ? _iv : Encoding.GetBytes(iv);
                        using (ICryptoTransform transform = des.CreateEncryptor())
                        {
                            byte[] data = transform.TransformFinalBlock(orgi, 0, orgi.Length);
                            if (data != null)
                            {
                                des.Clear();
                                switch (format)
                                {
                                    case CryptoStringFormat.Base64:
                                        return Convert.ToBase64String(data);
                                    case CryptoStringFormat.Encoding:
                                        return Encoding.GetString(data);
                                }
                            }
                        }
                    }
                    orgi = null;
                }
                catch (Exception ex)
                {
                    Log.WriteException(ex);
                }
                return null;
            }
            public static string Decrypt(string orgidata, byte[] key = null, CryptoStringFormat format = CryptoStringFormat.Base64, string iv = null)
            {
                if (orgidata == null)
                {
                    return null;
                }
                try
                {
                    byte[] orgi = null;
                    switch (format)
                    {
                        case CryptoStringFormat.Base64:
                            orgi = Convert.FromBase64String(orgidata);
                            break;
                        case CryptoStringFormat.Encoding:
                            orgi = Encoding.GetBytes(orgidata);
                            break;
                    }
                    using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                    {
                        des.Key = GetCheckKey(key);
                        des.IV = string.IsNullOrEmpty(iv) ? _iv : Encoding.GetBytes(iv);
                        using (ICryptoTransform transform = des.CreateDecryptor())
                        {
                            byte[] data = transform.TransformFinalBlock(orgi, 0, orgi.Length);
                            if (data != null)
                            {
                                des.Clear();
                                if (data != null)
                                    return Encoding.GetString(data);
                            }
                        }
                    }
                    orgi = null;
                }
                catch (Exception ex)
                {
                    Log.WriteException(ex);
                }
                return null;
            }
        }
        public enum CryptoStringFormat
        {
            Base64,
            Encoding
        }
        public enum CryptoType
        {
            AES,
            DES
        }
    }
}
