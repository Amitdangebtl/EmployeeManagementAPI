using System;
using System.Collections.Generic;

namespace UserAPI.Models;

public partial class Country
{
    public int CountryId { get; set; }

    public string? CountryName { get; set; }

    public virtual ICollection<State> States { get; set; } = new List<State>();

    public virtual ICollection<UsersRegister> UsersRegisters { get; set; } = new List<UsersRegister>();
}
