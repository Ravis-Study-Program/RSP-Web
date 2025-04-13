import Fuse from 'fuse.js';
import { ComboboxItem, OptionsFilter } from '@mantine/core';
import {
  GraduateDto,
  LeetcodeProblemDifficulty,
  ProblemAttemptEntity,
  SeasonWeekEntity,
} from '@/generated/api/client';

type CreateOptionsFilterParams = {
  fuzzy?: boolean;
  sort?: boolean;
};

export const createOptionsFilter = ({
  fuzzy = true,
  sort = true,
}: CreateOptionsFilterParams = {}): OptionsFilter => {
  return ({ options, search }) => {
    const typedOptions = options as ComboboxItem[];

    let filtered: ComboboxItem[];

    if (fuzzy && search.trim()) {
      const fuse = new Fuse(typedOptions, {
        keys: ['label'],
        threshold: 0.45,
      });

      filtered = fuse.search(search.trim()).map((r) => r.item);
    } else {
      filtered = typedOptions.filter((option) =>
        option.label.toLowerCase().includes(search.toLowerCase())
      );
    }

    if (sort) {
      filtered.sort((a, b) => {
        const numA = Number(a.label);
        const numB = Number(b.label);
        const isNumA = !isNaN(numA);
        const isNumB = !isNaN(numB);

        if (isNumA && isNumB) {
          return numA - numB;
        }
        if (isNumA) {
          return -1;
        }
        if (isNumB) {
          return 1;
        }

        return a.label.localeCompare(b.label);
      });
    }

    return filtered;
  };
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
    uniqueLeetcodeCategories
      .map((category) => ({
        value: category.leetcodeProblemCategoryId,
        label: category.name,
      }))
      .sort((a, b) => a.label.localeCompare(b.label)) || []
  );
};

export const getLeetcodeDifficulties = () => {
  return Object.entries(LeetcodeProblemDifficulty)
    .map(([label, value]) => ({
      label,
      value: value.toString(),
    }))
    .sort((a, b) => a.label.localeCompare(b.label));
};

export const getSeasonWeeks = (seasonWeeks: SeasonWeekEntity[]) => {
  return (
    seasonWeeks
      ?.map((seasonWeek) => ({
        value: seasonWeek.seasonWeekId,
        label: seasonWeek.weekNumber.toString(),
      }))
      .sort((a, b) => Number(a.label) - Number(b.label)) || []
  );
};

export const getGraduates = (graduates: GraduateDto[]) => {
  return graduates
    ?.map((graduate) => ({
      value: graduate.name,
      label: graduate.name,
    }))
    .sort((a, b) => a.label.localeCompare(b.label));
};
