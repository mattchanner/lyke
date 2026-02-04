using Lyke.Core.Enums;

namespace Lyke.Core.Entities;

public class Creator : BaseEntity
{
    public Guid UserId { get; set; }
    public required string DisplayName { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public string? SocialLinks { get; set; } // JSON
    public string? PayoutDetails { get; set; } // Encrypted

    // Verification workflow
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotSubmitted;
    public string? VerificationNotes { get; set; } // Creator's notes when submitting
    public string? VerificationDocumentUrls { get; set; } // JSON array of document URLs
    public DateTime? VerificationRequestedAt { get; set; }
    public DateTime? VerificationReviewedAt { get; set; }
    public Guid? VerificationReviewedByUserId { get; set; }
    public string? VerificationRejectionReason { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public User? VerificationReviewedByUser { get; set; }
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<CreatorEarning> Earnings { get; set; } = new List<CreatorEarning>();
}
