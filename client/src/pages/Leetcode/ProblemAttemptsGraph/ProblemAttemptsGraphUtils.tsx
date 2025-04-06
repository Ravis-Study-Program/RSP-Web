import dayjs from 'dayjs';
import { useRef } from 'react';
import { Coordinate } from 'recharts/types/util/types';
import { BarChart, LineChart, ScatterChart } from '@mantine/charts';
import { Paper, Text, useComputedColorScheme } from '@mantine/core';
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
export const LeetcodeScatterChart = ({
  problemAttempts,
  displayReferenceLines,
}: ProblemAttemptsGraphProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const bgColor = computedColorScheme === 'light' ? 'white' : 'dark';

  // Refs to hold metadata maps per difficulty
  const metadataRef = useRef<{
    Metadata: Map<number, { title: string; date: Date }>;
  }>({
    Metadata: new Map(),
  });

  const transform = (problemAttempts: ProblemAttemptEntity[]) => {
    const easy: Record<string, number>[] = [];
    const medium: Record<string, number>[] = [];
    const hard: Record<string, number>[] = [];

    let counter = 0;

    const sortedProblemAttempts = (problemAttempts || [])
      .map((attempt) => ({
        ...attempt,
        attemptStartDate: new Date(attempt.attemptStartDateUtc),
      }))
      .sort((a, b) => a.attemptStartDate.getTime() - b.attemptStartDate.getTime());

    sortedProblemAttempts.forEach((attempt) => {
      const date = new Date(attempt.attemptStartDateUtc);
      const minutes = attempt.timeTakenInMinutes;
      const title = attempt.leetcodeProblem?.problem?.title || 'Untitled';

      const entry = { index: counter, timeTakenInMinutes: minutes };

      switch (attempt.leetcodeProblem?.leetcodeProblemDifficulty) {
        case LeetcodeProblemDifficulty.Easy:
          easy.push(entry);
        case LeetcodeProblemDifficulty.Medium:
          medium.push(entry);
        case LeetcodeProblemDifficulty.Hard:
          hard.push(entry);
        default:
          metadataRef.current.Metadata.set(counter, { title, date });
          counter++;
      }
    });

    return [
      { name: 'Hard', color: 'red.5', data: hard },
      { name: 'Medium', color: 'yellow.5', data: medium },
      { name: 'Easy', color: 'green.5', data: easy },
    ];
  };

  let referenceLines = [
    { y: 45, label: 'Hard Goal (45)', color: 'red.7' },
    { y: 25, label: 'Medium Goal (25)', color: 'yellow.7' },
    { y: 10, label: 'Easy Goal (10)', color: 'green.7' },
  ];
  if (!displayReferenceLines) {
    referenceLines = [];
  }

  interface ChartTooltipProps {
    payload?: any[];
    coordinate?: Partial<Coordinate>;
    active?: boolean;
  }

  function ChartTooltip({ payload, coordinate }: ChartTooltipProps) {
    if (!payload || payload.length === 0 || !coordinate) return null;

    const item = payload[0];
    const { name, payload: data } = item;
    const meta = metadataRef.current.Metadata?.get(data.index);

    return (
      <Paper
        px="md"
        py="sm"
        shadow="md"
        radius="md"
        withBorder
        style={{
          position: 'absolute',
          left: (coordinate?.x ?? 0) + 12,
          top: (coordinate?.y ?? 0) - 30,
          pointerEvents: 'none',
          minWidth: 200,
        }}
      >
        <Text fz="sm">
          <strong>Problem:</strong> {meta?.title}
        </Text>
        <Text fz="sm">
          <strong>Time Taken:</strong> {data.timeTakenInMinutes} mins
        </Text>
        <Text fz="sm">
          <strong>Date:</strong> {meta ? dayjs(meta.date).format('YYYY-MM-DD HH:mm') : ''}
        </Text>
      </Paper>
    );
  }

  return (
    <ScatterChart
      h={400}
      bg={bgColor}
      data={transform(problemAttempts || [])}
      dataKey={{ x: 'index', y: 'timeTakenInMinutes' }}
      xAxisLabel="Date"
      yAxisLabel="Time Taken (mins)"
      tooltipProps={{
        content: (props) => <ChartTooltip {...props} />,
      }}
      tickLine="xy"
      xAxisProps={{
        type: 'number',
        domain: ['dataMin - 1', 'dataMax + 1'],
        tick: false,
      }}
      yAxisProps={{
        axisLine: false,
        tickLine: false,
        domain: [0, 120],
      }}
      withLegend
      referenceLines={referenceLines}
      scatterProps={{ shape: <circle r={6} /> }}
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
  displayReferenceLines: boolean;
};
