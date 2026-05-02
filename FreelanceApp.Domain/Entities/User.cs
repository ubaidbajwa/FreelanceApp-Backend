namespace FreelanceApp.Domain.Entities;
using FreelanceApp.Domain.Enums;

public class User
{
    public Guid ID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Freelancer;
    public bool isCnicVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}