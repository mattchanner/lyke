export interface FollowedCreatorResponse {
  creatorId: string;
  displayName: string;
  isVerified: boolean;
  profileImageUrl: string | null;
  followedAt: string;
}

export interface FollowedUserResponse {
  userId: string;
  displayName: string;
  userType: string;
  isVerified: boolean;
  creatorId: string | null;
  profileImageUrl: string | null;
  followedAt: string;
}
