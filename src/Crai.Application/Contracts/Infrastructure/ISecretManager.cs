using System;

namespace Crai.Application.Contracts.Infrastructure;

public interface ISecretManager
{
    /// <summary>
    /// Lưu trữ một giá trị bí mật (ví dụ: API Key) đã được mã hóa an toàn trên local disk.
    /// </summary>
    void StoreSecret(string key, string secretValue);

    /// <summary>
    /// Giải mã và lấy giá trị bí mật theo key.
    /// </summary>
    string? GetSecret(string key);

    /// <summary>
    /// Xóa bí mật khỏi bộ nhớ lưu trữ cục bộ.
    /// </summary>
    void RemoveSecret(string key);
}
