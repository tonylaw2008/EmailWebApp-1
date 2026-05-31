using System;
using System.Text;

public static class Base64Helper
{
    /// <summary>
    /// 判断字符串是否为 Base64，如果是则解码为文本返回，否则直接返回原字符串
    /// </summary>
    /// <param name="content">输入文本</param>
    /// <returns>解码后的文本 或 原文本</returns>
    public static string TryDecodeBase64(string content)
    {
        // 空值直接返回
        if (string.IsNullOrWhiteSpace(content))
            return content;

        try
        {
            // 1. 先判断是否符合 Base64 基本格式
            // 移除可能存在的换行、空格
            var base64Clean = content.Replace("\r", "").Replace("\n", "").Replace(" ", "");

            // 检查长度、字符集、填充符是否合法
            if (IsBase64Valid(base64Clean))
            {
                // 2. 尝试解码
                byte[] bytes = Convert.FromBase64String(base64Clean);
                string decodedText = Encoding.UTF8.GetString(bytes);

                // 3. 解码成功，返回明文
                return decodedText;
            }
        }
        catch
        {
            // 解码失败 → 直接返回原内容
            return content;
        }

        // 不是 Base64 → 返回原内容
        return content;
    }

    /// <summary>
    /// 基础格式校验：判断字符串是否符合 Base64 结构
    /// </summary>
    private static bool IsBase64Valid(string base64)
    {
        // Base64 字符只能是：A-Z a-z 0-9 + / =
        if (string.IsNullOrWhiteSpace(base64))
            return false;

        // 长度必须是 4 的倍数
        if (base64.Length % 4 != 0)
            return false;

        // 检查是否只包含合法字符
        foreach (char c in base64)
        {
            if (!char.IsLetterOrDigit(c) && c != '+' && c != '/' && c != '=')
                return false;
        }

        return true;
    }
}