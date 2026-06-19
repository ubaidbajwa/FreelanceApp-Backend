using FreelanceApp.Application.Interfaces.Repositories;
using FreelanceApp.Domain.Entities;
using FreelanceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreelanceApp.Infrastructure.Repositories;

public class KycRepository : IKycRepository
{
    private readonly AppDbContext _context;

    public KycRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IdentityVerification?> GetByUserIdAsync(Guid userId)
    {
        return await _context.IdentityVerifications
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(IdentityVerification verification)
    {
        await _context.IdentityVerifications.AddAsync(verification);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}