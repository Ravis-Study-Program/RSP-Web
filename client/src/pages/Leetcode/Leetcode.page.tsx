import { useMemo, useState } from 'react';
import { Group, MultiSelect } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import {
  useGetCurrentUser,
  useGetIsCurrentUserEnrolled,
  useGetSeasonWeeksBySeasonSlug,
  useListProblemAttempt,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import {
  getLeetcodeCategories,
  getLeetcodeDifficulties,
  getSeasonWeeks,
  optionsFilter,
} from '@/shared/table/globalFilters';
import { LeetcodeTable } from './LeetcodeTable/LeetcodeTable';
import { ProblemAttemptsGraph } from './ProblemAttemptsGraph/ProblemAttemptsGraph';
import classes from './Leetcode.module.css';

export default function LeetcodePage() {
  const [isLeetcode, _] = useState(true);
  const { seasonSlug } = useSeasonSlug();
  const [selectedSeasonWeeks, setSelectedSeasonWeeks] = useState<string[]>([]);
  const [selectedLeetcodeDifficulties, setSelectedLeetcodeDifficulties] = useState<string[]>([]);
  const [selectedLeetcodeCategories, setSelectedLeetcodeCategories] = useState<string[]>([]);

  const { data: currentUserResponse } = useGetCurrentUser();
  const email = currentUserResponse?.responseBody?.user.email ?? '';

  // TODO: Handle error and loading states using skeleton
  // TODO: Add Custom Problems support
  const { data: userResponse } = useGetIsCurrentUserEnrolled({ seasonSlug });

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useListProblemAttempt(
    {
      EnrollmentId: userResponse?.responseBody?.enrollmentId || undefined,
      IncludeCustom: !isLeetcode,
      IncludeLeetcode: isLeetcode,
      Email: email,
    },
    { query: { enabled: email !== '' } }
  );

  const { data: seasonWeeksResponse } = useGetSeasonWeeksBySeasonSlug(
    {
      SeasonSlug: seasonSlug,
    },
    { query: { enabled: seasonSlug !== '' } }
  );

  const seasonWeeksOptions = getSeasonWeeks(seasonWeeksResponse?.responseBody?.seasonWeeks || []);
  const leetcodeDifficultyOptions = getLeetcodeDifficulties();
  const leetcodeCategoryOptions = getLeetcodeCategories(
    problemAttemptsResponse?.responseBody?.problemAttempts || []
  );

  const filteredProblemAttempts = useMemo(() => {
    const attempts = problemAttemptsResponse?.responseBody?.problemAttempts || [];
    return attempts.filter(
      (attempt) =>
        // If no filters are applied, include all attempts
        (selectedSeasonWeeks.length === 0 &&
          selectedLeetcodeDifficulties.length === 0 &&
          selectedLeetcodeCategories.length === 0) ||
        // Season Weeks Filter
        ((selectedSeasonWeeks.length === 0 ||
          (attempt.seasonWeekId != null &&
            selectedSeasonWeeks.includes(attempt.seasonWeekId.toString()))) &&
          // Difficulty Filter
          (selectedLeetcodeDifficulties.length === 0 ||
            (attempt.leetcodeProblem?.leetcodeProblemDifficulty != null &&
              selectedLeetcodeDifficulties.includes(
                attempt.leetcodeProblem.leetcodeProblemDifficulty.toString()
              ))) &&
          // Categories Filter
          (selectedLeetcodeCategories.length === 0 ||
            (attempt.leetcodeProblem?.leetcodeProblemCategories?.some((category) =>
              selectedLeetcodeCategories.includes(category.leetcodeProblemCategoryId)
            ) ??
              false)))
    );
  }, [
    problemAttemptsResponse,
    selectedSeasonWeeks,
    selectedLeetcodeDifficulties,
    selectedLeetcodeCategories,
  ]);

  return (
    <Layout>
      <Group mb="lg" justify="flex-end">
        <MultiSelect
          classNames={{ inputField: classes.inputField }}
          label="Season Week"
          placeholder="Pick value(s)"
          data={seasonWeeksOptions}
          filter={optionsFilter}
          searchable
          clearable
          nothingFoundMessage="Nothing found..."
          value={selectedSeasonWeeks}
          onChange={(values) => {
            setSelectedSeasonWeeks(values as string[]);
          }}
        />
        <MultiSelect
          label="Difficulty"
          classNames={{ inputField: classes.inputField }}
          placeholder="Pick value(s)"
          data={leetcodeDifficultyOptions}
          filter={optionsFilter}
          searchable
          clearable
          nothingFoundMessage="Nothing found..."
          value={selectedLeetcodeDifficulties}
          onChange={(values) => {
            setSelectedLeetcodeDifficulties(values as string[]);
          }}
        />
        <MultiSelect
          label="Categories"
          classNames={{ inputField: classes.inputField }}
          placeholder="Pick value(s)"
          data={leetcodeCategoryOptions}
          filter={optionsFilter}
          searchable
          clearable
          nothingFoundMessage="Nothing found..."
          value={selectedLeetcodeCategories}
          onChange={(values) => {
            setSelectedLeetcodeCategories(values as string[]);
          }}
        />
      </Group>

      <ProblemAttemptsGraph problemAttempts={filteredProblemAttempts} />
      <LeetcodeTable
        refetchProblemAttempts={refetchProblemAttempts}
        problemAttempts={filteredProblemAttempts}
        enrollmentId={userResponse?.responseBody?.enrollmentId || ''}
        enableEditing
      />
    </Layout>
  );
}
