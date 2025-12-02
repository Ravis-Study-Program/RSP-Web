import { SeasonStudentRolePromotion } from '@/generated/api/client';

export const SeasonStudentRolePromotionColors: Record<SeasonStudentRolePromotion, string> = {
  [SeasonStudentRolePromotion.NotApplicable]: '#a3a7b1',
  [SeasonStudentRolePromotion.Novice]: '#27922b',
  [SeasonStudentRolePromotion.Beginner]: '#a3a7b1',
  [SeasonStudentRolePromotion.Intermediate]: '#498cff',
  [SeasonStudentRolePromotion.Advanced]: '#e44ffd',
};

export const MockInterviewScoreColors: Record<number, string> = {
  0: '#e27168',
  1: '#e88366',
  2: '#ee9663',
  3: '#f3a961',
  4: '#f9bc5e',
  5: '#ffd05b',
  6: '#d9cb62',
  7: '#b4c569',
  8: '#90bf70',
  9: '#6eb977',
  10: '#4db37f',
};
