import { Card, Grid, Group, Switch, Text } from '@mantine/core';
import classes from './Settings.module.css';

const data = [
  {
    title: 'Monday Updates',
    description: 'Receive reminders on Sundays to prepare for Monday Updates',
  },
  {
    title: 'Role Promotion Deadline',
    description: 'Get notified 2 weeks before the role promotion ends',
  },
];

export function NotificationSettings() {
  const items = data.map((item) => (
    <Group justify="space-between" className={classes.item} wrap="nowrap" gap="xl" key={item.title}>
      <div>
        <Text>{item.title}</Text>
        <Text size="xs" c="dimmed">
          {item.description}
        </Text>
      </div>
      <Switch className={classes.switch} size="md" />
    </Group>
  ));

  return (
    <Card withBorder radius="md" p="xl" shadow="sm" className={classes.card}>
      <Text fz="lg" className={classes.title} fw={700}>
        Notifications
      </Text>
      <Text fz="xs" c="dimmed" mt={3} mb="xl">
        Choose what notifications you want to receive
      </Text>
      {items}
    </Card>
  );
}

export default function SettingsPage() {
  return (
    <Grid gutter={{ base: 5, xs: 'md', md: 'xl' }}>
      <Grid.Col span={{ base: 12, sm: 6, md: 6, lg: 6 }}>
        <NotificationSettings />
      </Grid.Col>
    </Grid>
  );
}
