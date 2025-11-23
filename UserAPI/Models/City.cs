using System;
using System.Collections.Generic;

namespace UserAPI.Models;

public partial class City
{
    public int CityId { get; set; }

    public string? CityName { get; set; }

    public int? StateId { get; set; }

    public virtual State? State { get; set; }

    public virtual ICollection<UsersRegister> UsersRegisters { get; set; } = new List<UsersRegister>();
}
