using FreelanceApp.Application.Exceptions;
using FreelanceApp.Application.Features.Kyc.DTOs;
using FreelanceApp.Application.Interfaces.Repositories;
using FreelanceApp.Application.Interfaces.Services;
using FreelanceApp.Domain.Entities;
using FreelanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FreelanceApp.Application.Features.Kyc.Services;

public class KycService(
    IUserRepository userRepository,
    IImageStorageService imageStorage,
    IKycRepository kycRepository,
    ILogger<KycService> logger) : IKycService
{
    private const string KycFolderName = "kyc-documents";

    public async Task<Guid> UploadDocumentsAsync(
        Guid userId,
        KycUploadRequest request,
        CancellationToken ct = default)
    {
        logger.LogInformation("KYC upload started for user: {UserId}", userId);

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedException("User not found");

        if (user.IsCnicVerified)
            throw new ConflictException("Identity already verified");

        if (request.DocumentType == DocumentType.Cnic && request.BackImageStream == null)
            throw new ArgumentException("Back image is required for CNIC");

        var frontUrl = await imageStorage.UploadAsync(
            request.FrontImageStream,
            $"{Guid.NewGuid()}_front_{request.FrontImageFileName}",
            KycFolderName, ct);

        var selfieUrl = await imageStorage.UploadAsync(
            request.SelfieImageStream,
            $"{Guid.NewGuid()}_selfie_{request.SelfieImageFileName}",
            KycFolderName, ct);

        string? backUrl = null;
        if (request.BackImageStream != null && request.BackImageFileName != null)
        {
            backUrl = await imageStorage.UploadAsync(
                request.BackImageStream,
                $"{Guid.NewGuid()}_back_{request.BackImageFileName}",
                KycFolderName, ct);
        }

        var verification = new IdentityVerification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentType = request.DocumentType,
            Status = KycStatus.UnderReview,
            FrontImageUrl = frontUrl,
            BackImageUrl = backUrl,
            SelfieImageUrl = selfieUrl,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await kycRepository.AddAsync(verification);
        await kycRepository.SaveChangesAsync();

        logger.LogInformation("KYC uploaded for user: {UserId} | KycId: {KycId}",
            userId, verification.Id);

        return verification.Id;
    }
}