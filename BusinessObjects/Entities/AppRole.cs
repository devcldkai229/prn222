using BusinessObjects.Entities;
using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class AppRole
{
    public Guid Id { get; set; }

    [Required]
    public Role Name { get; set; } = Role.User;

}
