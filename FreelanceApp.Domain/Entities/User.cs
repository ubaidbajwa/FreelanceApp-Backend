namespace Freelancer.Domain.Entities;

public class User
{
    public Guid ID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; }= string.Empty;
    public string FullName { get; set; }=string.Empty;
    public string Role { get; set; } = "freelancer";
    public bool isCnicVerified { get; set; }= false;
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;

}