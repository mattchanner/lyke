export interface FollowedCreatorResponse {
  creatorId: string;
  displayName: string;
  isVerified: boolean;
  profileImageUrl: string | null;
  followedAt: string;
}
