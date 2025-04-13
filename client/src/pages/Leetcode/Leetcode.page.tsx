import { useMemo, useState } from 'react';
import { Flex, Group, MultiSelect, Select } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import {
  useGetCurrentUser,
  useGetIsCurrentUserEnrolled,
  useGetSeasonWeeksBySeasonSlug,
  useListProblemAttempt,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import {
  createOptionsFilter,
  getLeetcodeCategories,
  getLeetcodeDifficulties,
  getSeasonWeeks,
} from '@/shared/table/globalFilters';
import { LeetcodeTable } from './LeetcodeTable/LeetcodeTable';
import { ProblemAttemptsGraphContainer } from './ProblemAttemptsGraph/ProblemAttemptsGraphContainer';
import classes from './Leetcode.module.css';

export enum LeetcodeGraphPreset {
  None = 'None',
  // LineChart = 'Line Graph',
  // BarChart = 'Bar Chart',
  ScatterChart = 'Scatter Chart',
}

export default function LeetcodePage() {
  const [isLeetcode, _] = useState(true);
  const { seasonSlug } = useSeasonSlug();
  const [selectedSeasonWeeks, setSelectedSeasonWeeks] = useState<string[]>([]);
  const [selectedLeetcodeDifficulties, setSelectedLeetcodeDifficulties] = useState<string[]>([]);
  const [selectedLeetcodeCategories, setSelectedLeetcodeCategories] = useState<string[]>([]);
  const [selectedGraphPreset, setSelectedGraphPreset] = useState<LeetcodeGraphPreset>(
    LeetcodeGraphPreset.ScatterChart
  );

  const { data: currentUserResponse } = useGetCurrentUser();
  const email = currentUserResponse?.responseBody?.user.email ?? '';

  // TODO: Handle error and loading states using skeleton
  // TODO: Add Custom Problems support
  const { data: userResponse } = useGetIsCurrentUserEnrolled({ seasonSlug });

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useListProblemAttempt(
    {
      SeasonId: userResponse?.responseBody?.seasonId || undefined,
      IncludeCustom: !isLeetcode,
      IncludeLeetcode: isLeetcode,
      Emails: [email],
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

  const graphPresetOptions = Object.values(LeetcodeGraphPreset)
    .map((value) => ({
      value,
      label: value,
    }))
    .sort((a, b) => a.label.localeCompare(b.label));

  return (
    <Layout>
      <Flex justify="space-between">
        <Group mb="lg" justify="flex-start">
          <Select
            label="Graph Preset"
            data={graphPresetOptions}
            filter={createOptionsFilter()}
            miw={400}
            nothingFoundMessage="Nothing found..."
            value={selectedGraphPreset}
            onChange={(value) => {
              setSelectedGraphPreset(value as LeetcodeGraphPreset);
            }}
          />
        </Group>
        <Group mb="lg" justify="flex-end">
          <MultiSelect
            classNames={{ inputField: classes.inputField }}
            label="Season Week"
            placeholder="Pick value(s)"
            data={seasonWeeksOptions}
            filter={createOptionsFilter()}
            miw={150}
            searchable
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
            filter={createOptionsFilter()}
            miw={150}
            searchable
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
            filter={createOptionsFilter()}
            miw={150}
            searchable
            nothingFoundMessage="Nothing found..."
            value={selectedLeetcodeCategories}
            onChange={(values) => {
              setSelectedLeetcodeCategories(values as string[]);
            }}
          />
        </Group>
      </Flex>
      <ProblemAttemptsGraphContainer
        problemAttempts={filteredProblemAttempts}
        graphPreset={selectedGraphPreset}
      />
      <LeetcodeTable
        refetchProblemAttempts={refetchProblemAttempts}
        problemAttempts={filteredProblemAttempts}
        enrollmentId={userResponse?.responseBody?.enrollmentId || ''}
        enableEditing
        showAuthor={false}
        showCategory
      />
    </Layout>
  );
}
