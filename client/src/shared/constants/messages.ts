export const NOTIFICATION_MESSAGES = {
  USER: {
    CREATED: 'User created successfully.',
    UPDATED: 'User updated successfully.',
    DELETED: 'User deleted successfully.',
    SLUG_UPDATED: 'Your profile slug has been updated successfully! Refreshing page...',
    SLUG_UPDATE_FAILED: 'Failed to update slug. Please try again.',
    SLUG_GENERATED: 'A new random slug has been generated for you!',
    SLUG_GENERATE_FAILED: 'Failed to generate random slug. Please try again.',
  },
  SEASON: {
    CREATED: 'Season created successfully.',
    UPDATED: 'Season updated successfully.',
    DELETED: 'Season deleted successfully.',
  },
  SEASON_WEEK: {
    CREATED: 'Season Week created successfully.',
    UPDATED: 'Season Week updated successfully.',
    DELETED: 'Season Week deleted successfully.',
  },
  ENROLLMENT: {
    CREATED: 'Enrollment created successfully.',
    UPDATED: 'Enrollment updated successfully.',
    DELETED: 'Enrollment deleted successfully.',
  },
  MENTORSHIP: {
    CREATED: 'Mentorship created successfully.',
    UPDATED: 'Mentorship updated successfully.',
    DELETED: 'Mentorship deleted successfully.',
  },
  MOCK_INTERVIEW: {
    CREATED: 'Mock interview created successfully.',
    UPDATED: 'Mock interview updated successfully.',
    DELETED: 'Mock interview deleted successfully.',
    REVIEW_UPDATED: 'Review status updated successfully.',
  },
  PROBLEM_ATTEMPT: {
    CREATED: 'Problem attempt created successfully.',
    UPDATED: 'Problem attempt updated successfully.',
    DELETED: 'Problem attempt deleted successfully.',
  },
  MENTEE: {
    DELETED: 'Mentee deleted successfully.',
  },
} as const;

export const CONFIRMATION_MESSAGES = {
  USER: {
    DELETE_TITLE: 'Delete User',
    DELETE_TEXT: 'Are you sure you want to delete this user? This action cannot be undone.',
  },
  SEASON: {
    DELETE_TITLE: 'Delete Season',
    DELETE_TEXT: 'Are you sure you want to delete this season? This action cannot be undone.',
  },
  SEASON_WEEK: {
    DELETE_TITLE: 'Delete Season Week',
    DELETE_TEXT: 'Are you sure you want to delete this season week? This action cannot be undone.',
  },
  ENROLLMENT: {
    DELETE_TITLE: 'Delete Enrollment',
    DELETE_TEXT: 'Are you sure you want to delete this enrollment? This action cannot be undone.',
  },
  MENTORSHIP: {
    DELETE_TITLE: 'Delete Mentorship',
    DELETE_TEXT: 'Are you sure you want to delete this mentorship? This action cannot be undone.',
  },
  MOCK_INTERVIEW: {
    DELETE_TITLE: 'Delete Mock Interview',
    DELETE_TEXT:
      'Are you sure you want to delete this mock interview? This action cannot be undone.',
  },
  PROBLEM_ATTEMPT: {
    DELETE_TITLE: 'Delete Problem Attempt',
    DELETE_TEXT:
      'Are you sure you want to delete this problem attempt? This action cannot be undone.',
  },
  MENTEE: {
    DELETE_TITLE: 'Delete Mentee',
    DELETE_TEXT: 'Are you sure you want to delete this mentee? This action cannot be undone.',
  },
} as const;
