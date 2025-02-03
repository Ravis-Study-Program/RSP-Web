import { Container, Text, useComputedColorScheme } from '@mantine/core';
import { ProblemAttemptEntity } from '@/generated/api/client';
import { LeetcodeGraphPreset } from '../Leetcode.page';
import {
  LeetcodeBarChart,
  LeetcodeLineChart,
  LeetcodeScatterChart,
} from './ProblemAttemptsGraphUtils';
import classes from './ProblemAttemptsGraphContainer.module.css';

export const ProblemAttemptsGraphContainer = ({
  problemAttempts,
  graphPreset,
}: ProblemAttemptsGraphContainerProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const bgColor = computedColorScheme === 'light' ? 'white' : 'dark';

  const renderGraph = () => {
    switch (graphPreset) {
      case LeetcodeGraphPreset.ScatterChart:
        return <LeetcodeScatterChart problemAttempts={problemAttempts} />;
      case LeetcodeGraphPreset.BarChart:
        return <LeetcodeBarChart problemAttempts={problemAttempts} />;
      case LeetcodeGraphPreset.LineChart:
        return <LeetcodeLineChart problemAttempts={problemAttempts} />;
      default:
        return null;
    }
  };

  const graphTitle = () => {
    switch (graphPreset) {
      case LeetcodeGraphPreset.ScatterChart:
        return 'Time Taken of Solved LeetCodes By Difficulty';
      case LeetcodeGraphPreset.BarChart:
        return 'Daily Counts Of Solved LeetCodes By Difficulty';
      case LeetcodeGraphPreset.LineChart:
        return 'Daily Counts of Solved LeetCodes By Difficulty';
      default:
        return '';
    }
  };

  if (
    problemAttempts != null &&
    problemAttempts.length > 0 &&
    graphPreset !== LeetcodeGraphPreset.None
  ) {
    return (
      <>
        <Text fw={500} size="sm" mb={10}>
          {graphTitle()}
        </Text>
        <Container bg={bgColor} fluid className={classes.graphContainer}>
          {renderGraph()}
        </Container>
      </>
    );
  }
};

type ProblemAttemptsGraphContainerProps = {
  problemAttempts: ProblemAttemptEntity[] | null | undefined;
  graphPreset: LeetcodeGraphPreset;
};
