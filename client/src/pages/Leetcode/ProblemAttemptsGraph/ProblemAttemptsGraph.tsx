import { LineChart } from '@mantine/charts';
import { Container, useComputedColorScheme } from '@mantine/core';
import { LeetcodeProblemDifficulty, ProblemAttemptEntity } from '@/generated/api/client';
import classes from './ProblemAttemptsGraph.module.css';

function formatDate(date: Date): string {
  return date.toLocaleString('default', { month: 'short', day: 'numeric' });
}

export const ProblemAttemptsGraph = ({ problemAttempts }: ProblemAttemptsGraphProps) => {
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

  if (problemAttempts != null && problemAttempts.length > 0) {
    return (
      <Container bg={bgColor} fluid className={classes.graphContainer}>
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
      </Container>
    );
  }
};

type ProblemAttemptsGraphProps = {
  problemAttempts: ProblemAttemptEntity[] | null | undefined;
};
