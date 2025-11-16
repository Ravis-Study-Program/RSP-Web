import { IconExternalLink } from '@tabler/icons-react';
import { Anchor, Flex, Text } from '@mantine/core';
import classes from '@/shared/styles/tableStyles.module.css';

interface ProfileLinkProps {
  userName: string | undefined | null;
  userSlug: string | undefined | null;
}

export const ProfileLink = ({ userName, userSlug }: ProfileLinkProps) => {
  if (userName != null && userSlug != null) {
    return (
      <Anchor
        href={`/profile?user=${userSlug}`}
        target="_blank"
        className={classes.title}
        underline="always"
      >
        <Flex align="center" gap="xs">
          <Text size="sm">{userName}</Text>
          <IconExternalLink size={12} style={{ flexShrink: 0, opacity: 0.7 }} />
        </Flex>
      </Anchor>
    );
  }

  return <Text size="sm">No User Found</Text>;
};
