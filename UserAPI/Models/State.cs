using System;
using System.Collections.Generic;

namespace UserAPI.Models;

public partial class State
{
    public int StateId { get; set; }

    public string? StateName { get; set; }

    public int? CountryId { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual Country? Country { get; set; }

    public virtual ICollection<UsersRegister> UsersRegisters { get; set; } = new List<UsersRegister>();
}
