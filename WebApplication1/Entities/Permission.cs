using System.Collections.Generic;

namespace Lexa.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<RolePermission> RolePermissions { get; set; } = new();
}
