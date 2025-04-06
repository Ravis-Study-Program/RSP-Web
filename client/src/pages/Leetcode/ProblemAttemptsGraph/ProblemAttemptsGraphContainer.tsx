import { Container, Flex, Switch, Text, useComputedColorScheme } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { ProblemAttemptEntity } from '@/generated/api/client';
import { LeetcodeGraphPreset } from '../Leetcode.page';
import { LeetcodeScatterChart } from './ProblemAttemptsGraphUtils';
import classes from './ProblemAttemptsGraphContainer.module.css';

export const ProblemAttemptsGraphContainer = ({
  problemAttempts,
  graphPreset,
}: ProblemAttemptsGraphContainerProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const bgColor = computedColorScheme === 'light' ? 'white' : 'dark';
  const [showReferenceLines, setShowReferenceLines] = useLocalStorage({
    key: 'show-reference-lines',
    defaultValue: true,
  });

  const renderGraph = () => {
    switch (graphPreset) {
      case LeetcodeGraphPreset.ScatterChart:
        return (
          <LeetcodeScatterChart
            problemAttempts={problemAttempts}
            displayReferenceLines={showReferenceLines}
          />
        );
      default:
        return null;
    }
  };

  const graphTitle = () => {
    switch (graphPreset) {
      case LeetcodeGraphPreset.ScatterChart:
        return 'LeetCode Time by Difficulty';
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
        <Container bg={bgColor} fluid className={classes.graphContainer}>
          <Flex justify="space-between" mb={20}>
            <Text fw={600} size="md">
              {graphTitle()}
            </Text>
            <Switch
              checked={showReferenceLines}
              onChange={(event) => setShowReferenceLines(event.currentTarget.checked)}
              label="Show Reference Lines"
            />
          </Flex>
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
