using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public Gender Gender { get; set; } = Gender.Unknown;

    [Range(5, 100)]
    public int Age { get; set; }

    [StringLength(500)]
    public string AvatarUrl { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid AppRoleId { get; set; }
    public AppRole AppRole { get; set; } = null!;

    public string[]? Allergies { get; set; } = new string[] { };

    public ICollection<Meal>? DislikedMeals { get; set; }

    public ICollection<NutritionLog> NutritionLogs { get; set; } = new List<NutritionLog>();

    [Range(80, 220)]
    public int HeightCm { get; set; }

    [Range(20, 250)]
    public decimal WeightKg { get; set; }

    public FitnessGoal Goal { get; set; } = FitnessGoal.Maintain;

    public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.ModeratelyActive;

    public DietPreference DietPreference { get; set; } = DietPreference.None;

    public int? CaloriesInDay { get; set; }

    [StringLength(10000)]
    public string? Hobbies { get; set; }
}
