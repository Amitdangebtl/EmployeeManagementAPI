using System;
using System.Collections.Generic;

namespace UserAPI.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<UsersRegister> UsersRegisters { get; set; } = new List<UsersRegister>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
