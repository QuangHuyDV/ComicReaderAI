using System;

namespace Crai.Application.Contracts.Infrastructure;

public class ResourceDescriptor
{
    /// <summary>
    /// Mã định danh duy nhất của tài nguyên.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Tên mô tả của tài nguyên.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Kiểu dữ liệu thực tế của tài nguyên.
    /// </summary>
    public Type Type { get; }

    public ResourceDescriptor(string id, string name, Type type)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }
}
