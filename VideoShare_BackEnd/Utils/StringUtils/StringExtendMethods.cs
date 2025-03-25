using System.Security.Cryptography;
using System.Text;

namespace VideoShare_BackEnd.Utils.StringUtils;

public static class StringExtendMethods
{
    /// <summary>
    /// 获取传入string变量的sha256加密结果
    /// </summary>
    /// <param name="str">待加密串</param>
    /// <returns></returns>
    public async static Task<string> Sha256CryptAsync(this string str)
    {
        MemoryStream stream = new MemoryStream();
        await stream.WriteAsync(Encoding.Default.GetBytes(str));
        stream.Position = 0;

        byte[] cryptBytes = await SHA256.Create().ComputeHashAsync(stream);

        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < cryptBytes.Length; i++)
        {
            stringBuilder.Append(cryptBytes[i].ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取传入string变量的md5加密结果
    /// </summary>
    /// <param name="str">待加密串</param>
    /// <returns></returns>
    public async static Task<string> Md5CryptAsync(this string str)
    {
        MemoryStream stream = new MemoryStream();
        await stream.WriteAsync(Encoding.Default.GetBytes(str));
        stream.Position = 0;

        byte[] cryptBytes = await MD5.Create().ComputeHashAsync(stream);

        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < cryptBytes.Length; i++)
        {
            stringBuilder.Append(cryptBytes[i].ToString("x2"));
        }

        return stringBuilder.ToString();
    }
}