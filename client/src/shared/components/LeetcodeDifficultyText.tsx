import { Text } from '@mantine/core';
import { LeetcodeProblemDifficulty } from '@/generated/api/client';
import { LeetcodeProblemDifficultyReverseIndex } from '@/shared/entities/reverseIndex';
import classes from '@/shared/styles/tableStyles.module.css';

type LeetcodeDifficultyTextProps = {
  difficulty?: LeetcodeProblemDifficulty;
};

export const LeetcodeDifficultyText = ({ difficulty }: LeetcodeDifficultyTextProps) => {
  let textClass = '';
  switch (difficulty) {
    case LeetcodeProblemDifficulty.Easy:
      textClass = classes.textGreen;
      break;
    case LeetcodeProblemDifficulty.Medium:
      textClass = classes.textYellow;
      break;
    case LeetcodeProblemDifficulty.Hard:
      textClass = classes.textRed;
      break;
    default:
      textClass = '';
  }

  return (
    <Text size="sm" className={textClass}>
      {difficulty != null ? LeetcodeProblemDifficultyReverseIndex[difficulty] : '-'}
    </Text>
  );
};
