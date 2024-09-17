export type LeetcodeProblem = {
  LeetcodeId: number;
  Title: string;
  Link: string;
  LeetcodeDifficulty: LeetcodeDifficulty;
  LeetcodeCategories: LeetcodeCategory[];
};

export enum LeetcodeDifficulty {
  Easy,
  Medium,
  Hard,
}

export type LeetcodeCategory = {
  LeetcodeCategoryId: number;
  Name: string;
};

export type Problem = {
  ProblemId: number;
  StudentId: number;
  LeetcodeId: number;
  StartDateTime: Date;
  TimeTakenInMinutes: number;
  IsCompleteWithoutHelp: boolean;
};

export const mockLeetcodeCategory: LeetcodeCategory[] = [
  {
    LeetcodeCategoryId: 1,
    Name: 'Array',
  },
  {
    LeetcodeCategoryId: 2,
    Name: 'Hash Table',
  },
  {
    LeetcodeCategoryId: 3,
    Name: 'Linked List',
  },
  {
    LeetcodeCategoryId: 4,
    Name: 'Math',
  },
  {
    LeetcodeCategoryId: 5,
    Name: 'Recursion',
  },
];

export const mockLeetcodes: LeetcodeProblem[] = [
  {
    LeetcodeId: 1,
    Title: 'Two Sum',
    Link: 'https://leetcode.com/problems/two-sum/',
    LeetcodeDifficulty: LeetcodeDifficulty.Easy,
    LeetcodeCategories: [mockLeetcodeCategory[0], mockLeetcodeCategory[1]],
  },
  {
    LeetcodeId: 2,
    Title: 'Add Two Numbers',
    Link: 'https://leetcode.com/problems/add-two-numbers/',
    LeetcodeDifficulty: LeetcodeDifficulty.Medium,
    LeetcodeCategories: [mockLeetcodeCategory[2], mockLeetcodeCategory[3], mockLeetcodeCategory[4]],
  },
  {
    LeetcodeId: 4,
    Title: 'Median of Two Sorted Arrays',
    Link: 'https://leetcode.com/problems/median-of-two-sorted-arrays/',
    LeetcodeDifficulty: LeetcodeDifficulty.Hard,
    LeetcodeCategories: [mockLeetcodeCategory[0]],
  },
];

export const mockProblems: Problem[] = Array(12)
  .fill(null)
  .map((_, index) => {
    function getRandomTimestampFromDaysAgo(daysAgo: number) {
      const now = new Date().getTime();
      const daysAgoMs = now - daysAgo * 24 * 60 * 60 * 1000;
      const randomTimestamp = Math.floor(Math.random() * (now - daysAgoMs + 1)) + daysAgoMs;

      return new Date(randomTimestamp);
    }

    const leetcodeIndex = Math.floor(Math.random() * mockLeetcodes.length);
    const startDateTime = getRandomTimestampFromDaysAgo(5);
    const minutesAfter = Math.floor(Math.random() * 120);
    const IsCompleteWithoutHelp = Math.random() < 0.5;

    return {
      ProblemId: index,
      StudentId: 1,
      LeetcodeId: leetcodeIndex,
      StartDateTime: startDateTime,
      TimeTakenInMinutes: minutesAfter,
      IsCompleteWithoutHelp,
    };
  });

function formatDate(date: Date): string {
  return date.toLocaleString('default', { month: 'short', day: 'numeric' });
}

export const transformData = (problems: Problem[]): any[] => {
  const result: Record<string, any> = {};

  problems.forEach((problem) => {
    const date = formatDate(problem.StartDateTime);
    if (!result[date]) {
      result[date] = { date, Easy: 0, Medium: 0, Hard: 0 };
    }

    switch (mockLeetcodes[problem.LeetcodeId].LeetcodeDifficulty) {
      case LeetcodeDifficulty.Easy:
        result[date].Easy += 1;
        break;
      case LeetcodeDifficulty.Medium:
        result[date].Medium += 1;
        break;
      case LeetcodeDifficulty.Hard:
        result[date].Hard += 1;
        break;
    }
  });

  problems.sort((a, b) => {
    return a.StartDateTime.getTime() - b.StartDateTime.getTime();
  });

  return Object.values(result);
};
