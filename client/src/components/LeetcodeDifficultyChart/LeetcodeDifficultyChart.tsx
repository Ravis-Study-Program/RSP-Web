import { useMemo } from 'react';
import { PieChart } from '@mantine/charts';
import { Center, Container, Text, useComputedColorScheme } from '@mantine/core';
import { LeetcodeProblemDifficulty, ProblemAttemptEntity } from '@/generated/api/client';
import chartClasses from '@/shared/styles/chartContainer.module.css';

interface LeetcodeDifficultyChartProps {
  problemAttempts: ProblemAttemptEntity[] | undefined;
}

interface ChartData {
  name: string;
  value: number;
  color: string;
}

export const LeetcodeDifficultyChart = ({ problemAttempts }: LeetcodeDifficultyChartProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const bgColor = computedColorScheme === 'light' ? 'white' : 'dark';
  const totalProblems = problemAttempts?.length ?? 0;

  const toPercent = (count: number) =>
    Number(((count / totalProblems) * 100).toFixed(1));

  const chartData: ChartData[] = useMemo(() => {
    if (!problemAttempts || problemAttempts.length === 0) {
      return [];
    }

    const difficultyCounts = { Easy: 0, Medium: 0, Hard: 0 };
    problemAttempts.forEach((attempt) => {
      if (attempt.leetcodeProblem?.leetcodeProblemDifficulty !== undefined) {
        switch (attempt.leetcodeProblem.leetcodeProblemDifficulty) {
          case LeetcodeProblemDifficulty.Easy:
            difficultyCounts.Easy += 1;
            break;
          case LeetcodeProblemDifficulty.Medium:
            difficultyCounts.Medium += 1;
            break;
          case LeetcodeProblemDifficulty.Hard:
            difficultyCounts.Hard += 1;
            break;
        }
      }
    });

    return [
      {
        name: 'Easy',
        value: toPercent(difficultyCounts.Easy),
        color: 'var(--mantine-color-green-6)',
      },
      {
        name: 'Medium',
        value: toPercent(difficultyCounts.Medium),
        color: 'var(--mantine-color-yellow-6)',
      },
      {
        name: 'Hard',
        value: toPercent(difficultyCounts.Hard),
        color: 'var(--mantine-color-red-6)',
      },
    ].filter((item) => item.value > 0);
  }, [problemAttempts]);

  return (
    <Container bg={bgColor} fluid className={chartClasses.chartContainer}>
      <Text fw={600} size="md" mb={20}>
        LeetCode Difficulty Breakdown
      </Text>
      <Center h="calc(100% - 100px)">
        {totalProblems > 0 && (
          <PieChart data={chartData}
            withLabelsLine={false}
            labelsPosition="inside"
            withTooltip
            withLabels
            labelsType="percent"
            strokeWidth={2}
            size={200}
          />
        )}
      </Center>
      <Text c="dimmed" size="xs" ta="center" mt="md">
        Total:{' '}
        <Text span fw={500}>
          {totalProblems}
        </Text>{' '}
        problems
      </Text>
    </Container>
  );
};
