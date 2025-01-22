import { ComboboxItem, OptionsFilter } from '@mantine/core';
import {
  LeetcodeProblemDifficulty,
  ProblemAttemptEntity,
  SeasonWeekEntity,
} from '@/generated/api/client';

export const optionsFilter: OptionsFilter = ({ options, search }) => {
  const filtered = (options as ComboboxItem[]).filter((option) =>
    option.label.toLowerCase().trim().includes(search.toLowerCase().trim())
  );

  filtered.sort((a, b) => {
    const numA = Number(a.label);
    const numB = Number(b.label);

    const isNumA = !isNaN(numA);
    const isNumB = !isNaN(numB);

    if (isNumA && isNumB) {
      return numA - numB;
    } else if (isNumA) {
      return -1;
    } else if (isNumB) {
      return 1;
    }

    return a.label.localeCompare(b.label);
  });
  return filtered;
};

export const getLeetcodeCategories = (problemAttempts: ProblemAttemptEntity[]) => {
  const leetcodeCategories =
    problemAttempts
      .flatMap((attempt) => attempt.leetcodeProblem?.leetcodeProblemCategories ?? [])
      .filter((category) => category != null) || [];

  const uniqueLeetcodeCategories = [];
  const seenIds = new Set();

  for (const obj of leetcodeCategories) {
    if (obj != null && !seenIds.has(obj.leetcodeProblemCategoryId)) {
      seenIds.add(obj.leetcodeProblemCategoryId);
      uniqueLeetcodeCategories.push(obj);
    }
  }

  return (
    uniqueLeetcodeCategories.map((category) => ({
      value: category.leetcodeProblemCategoryId,
      label: category.name,
    })) || []
  );
};

export const getLeetcodeDifficulties = () => {
  return Object.entries(LeetcodeProblemDifficulty).map(([label, value]) => ({
    label,
    value: value.toString(),
  }));
};

export const getSeasonWeeks = (seasonWeeks: SeasonWeekEntity[]) => {
  return (
    seasonWeeks?.map((seasonWeek) => ({
      value: seasonWeek.seasonWeekId,
      label: seasonWeek.weekNumber.toString(),
    })) || []
  );
};
