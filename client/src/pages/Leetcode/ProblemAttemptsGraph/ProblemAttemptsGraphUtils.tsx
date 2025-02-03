import { BarChart, LineChart, ScatterChart, ScatterChartSeries } from '@mantine/charts';
import { useComputedColorScheme } from '@mantine/core';
import { LeetcodeProblemDifficulty, ProblemAttemptEntity } from '@/generated/api/client';

function formatDate(date: Date): string {
  return date.toLocaleString('default', { month: 'short', day: 'numeric' });
}

// Daily counts of problem solved by difficulty (Line Chart)
export const LeetcodeLineChart = ({ problemAttempts }: ProblemAttemptsGraphProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const bgColor = computedColorScheme === 'light' ? 'white' : 'dark';

  const transform = (problemAttempts: ProblemAttemptEntity[]) => {
    const result: Record<string, { date: string; Easy: number; Medium: number; Hard: number }> = {};

    const sortedProblemAttempts = problemAttempts
      .map((attempt) => ({
        ...attempt,
        attemptStartDate: new Date(attempt.attemptStartDateUtc),
      }))
      .sort((a, b) => a.attemptStartDate.getTime() - b.attemptStartDate.getTime());

    sortedProblemAttempts.forEach((problemAttempt) => {
      const date = formatDate(new Date(problemAttempt.attemptStartDate));
      if (!result[date]) {
        result[date] = { date, Easy: 0, Medium: 0, Hard: 0 };
      }

      switch (problemAttempt.leetcodeProblem?.leetcodeProblemDifficulty) {
        case LeetcodeProblemDifficulty.Easy:
          result[date].Easy += 1;
          break;
        case LeetcodeProblemDifficulty.Medium:
          result[date].Medium += 1;
          break;
        case LeetcodeProblemDifficulty.Hard:
          result[date].Hard += 1;
          break;
      }
    });

    return Object.values(result);
  };

  return (
    <LineChart
      h={300}
      bg={bgColor}
      data={transform(problemAttempts || [])}
      dataKey="date"
      xAxisLabel="Date"
      yAxisLabel="Count"
      series={[
        { name: 'Easy', color: 'green.6' },
        { name: 'Medium', color: 'yellow.6' },
        { name: 'Hard', color: 'red.6' },
      ]}
      curveType="linear"
      tickLine="xy"
      gridAxis="xy"
      yAxisProps={{ domain: [0, 'auto'], interval: 1, minTickGap: 1 }}
      withLegend
      legendProps={{ verticalAlign: 'top', height: 50 }}
    />
  );
};

// Time taken for each problem by difficulty (Scatter Chart)
export const LeetcodeScatterChart = ({ problemAttempts }: ProblemAttemptsGraphProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const bgColor = computedColorScheme === 'light' ? 'white' : 'dark';

  const transform = (problemAttempts: ProblemAttemptEntity[]): ScatterChartSeries[] => {
    const easy: Record<string, number>[] = [];
    const medium: Record<string, number>[] = [];
    const hard: Record<string, number>[] = [];

    const sortedProblemAttempts = problemAttempts
      .map((attempt) => ({
        ...attempt,
        attemptStartDate: new Date(attempt.attemptStartDateUtc),
      }))
      .sort((a, b) => a.attemptStartDate.getTime() - b.attemptStartDate.getTime());

    sortedProblemAttempts.forEach((problemAttempt) => {
      switch (problemAttempt.leetcodeProblem?.leetcodeProblemDifficulty) {
        case LeetcodeProblemDifficulty.Easy:
          easy.push({
            date: problemAttempt.attemptStartDate.getTime(),
            minutes: problemAttempt.timeTakenInMinutes,
          });
          break;
        case LeetcodeProblemDifficulty.Medium:
          medium.push({
            date: problemAttempt.attemptStartDate.getTime(),
            minutes: problemAttempt.timeTakenInMinutes,
          });
          break;
        case LeetcodeProblemDifficulty.Hard:
          hard.push({
            date: problemAttempt.attemptStartDate.getTime(),
            minutes: problemAttempt.timeTakenInMinutes,
          });
          break;
      }
    });

    const groups: ScatterChartSeries[] = [
      {
        color: 'red.5',
        name: 'Hard',
        data: hard,
      },
      {
        color: 'yellow.5',
        name: 'Medium',
        data: medium,
      },
      {
        color: 'green.5',
        name: 'Easy',
        data: easy,
      },
    ];

    return groups;
  };

  return (
    <ScatterChart
      h={300}
      bg={bgColor}
      data={transform(problemAttempts || [])}
      dataKey={{ x: 'date', y: 'minutes' }}
      xAxisLabel="Date"
      yAxisLabel="Minutes"
      valueFormatter={{
        x: (value) => formatDate(new Date(value)),
        y: (value) => value.toString(),
      }}
      tickLine="xy"
      yAxisProps={{ domain: [0, 120] }}
      xAxisProps={{ interval: 0, domain: ['auto', 'auto'] }}
      withLegend
      referenceLines={[
        { y: 45, label: 'Hard Goal', color: 'red.7' },
        { y: 25, label: 'Medium Goal', color: 'yellow.7' },
        { y: 10, label: 'Easy Goal', color: 'green.7' },
      ]}
    />
  );
};

// Daily counts of problem solved by difficulty (Bar Chart)
export const LeetcodeBarChart = ({ problemAttempts }: ProblemAttemptsGraphProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const bgColor = computedColorScheme === 'light' ? 'white' : 'dark';

  const transform = (problemAttempts: ProblemAttemptEntity[]) => {
    const result: Record<string, { date: string; Easy: number; Medium: number; Hard: number }> = {};

    const sortedProblemAttempts = problemAttempts
      .map((attempt) => ({
        ...attempt,
        attemptStartDate: new Date(attempt.attemptStartDateUtc),
      }))
      .sort((a, b) => a.attemptStartDate.getTime() - b.attemptStartDate.getTime());

    sortedProblemAttempts.forEach((problemAttempt) => {
      const date = formatDate(new Date(problemAttempt.attemptStartDate));
      if (!result[date]) {
        result[date] = { date, Easy: 0, Medium: 0, Hard: 0 };
      }

      switch (problemAttempt.leetcodeProblem?.leetcodeProblemDifficulty) {
        case LeetcodeProblemDifficulty.Easy:
          result[date].Easy += 1;
          break;
        case LeetcodeProblemDifficulty.Medium:
          result[date].Medium += 1;
          break;
        case LeetcodeProblemDifficulty.Hard:
          result[date].Hard += 1;
          break;
      }
    });

    return Object.values(result);
  };

  return (
    <BarChart
      bg={bgColor}
      h={300}
      data={transform(problemAttempts || [])}
      dataKey="date"
      xAxisLabel="Date"
      yAxisLabel="Count"
      type="stacked"
      series={[
        { name: 'Easy', color: 'green.6' },
        { name: 'Medium', color: 'yellow.6' },
        { name: 'Hard', color: 'red.6' },
      ]}
      tickLine="y"
    />
  );
};

type ProblemAttemptsGraphProps = {
  problemAttempts: ProblemAttemptEntity[] | null | undefined;
};
