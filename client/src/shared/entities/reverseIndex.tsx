import { LeetcodeProblemDifficulty, SeasonRole } from '@/generated/api/client';

export const LeetcodeProblemDifficultyReverseIndex = Object.fromEntries(
  Object.entries(LeetcodeProblemDifficulty).map(([key, value]) => [value, key])
) as { [key: number]: keyof typeof LeetcodeProblemDifficulty };

export const SeasonRoleReverseIndex = Object.fromEntries(
  Object.entries(SeasonRole).map(([key, value]) => [value, key])
) as { [key: number]: keyof typeof SeasonRole };
