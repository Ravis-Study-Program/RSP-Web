import { useEffect } from 'react';
import { IconCheck, IconDice, IconX } from '@tabler/icons-react';
import { Alert, Button, Card, Grid, Group, Text, TextInput } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  useGenerateRandomSlug,
  useGetCurrentUser,
  useUpdateUserSlug,
} from '@/generated/api/client';
import { NOTIFICATION_MESSAGES } from '@/shared/constants/messages';
import classes from './Settings.module.css';

export function ProfileSettings() {
  const { data: currentUserResponse } = useGetCurrentUser();
  const { mutateAsync: updateUserSlug, isPending } = useUpdateUserSlug();
  const { mutateAsync: generateRandomSlug, isPending: isGeneratingSlug } = useGenerateRandomSlug();

  const form = useForm({
    initialValues: {
      slug: currentUserResponse?.responseBody?.user?.slug || '',
    },
    validate: {
      slug: (value) => {
        if (!value) {
          return 'Slug is required';
        }
        if (!/^[a-z0-9-]+$/.test(value)) {
          return 'Slug must contain only lowercase letters, numbers, and hyphens';
        }
        if (value.length < 3 || value.length > 50) {
          return 'Slug must be between 3 and 50 characters long';
        }
        return null;
      },
    },
  });

  const handleSubmit = async (values: { slug: string }) => {
    try {
      await updateUserSlug({ data: { slug: values.slug } });
      notifications.show({
        title: 'Success',
        message: NOTIFICATION_MESSAGES.USER.SLUG_UPDATED,
        color: 'green',
        icon: <IconCheck size={16} />,
      });

      // Reload the page after 2 seconds to refresh cached data
      setTimeout(() => {
        window.location.reload();
      }, 2000);
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message:
          error.response?.data?.error?.message || NOTIFICATION_MESSAGES.USER.SLUG_UPDATE_FAILED,
        color: 'red',
        icon: <IconX size={16} />,
      });
    }
  };

  const handleGenerateRandomSlug = async () => {
    try {
      const result = await generateRandomSlug({ data: {} });
      form.setFieldValue('slug', result.responseBody?.slug || '');

      notifications.show({
        title: 'Random slug generated',
        message: NOTIFICATION_MESSAGES.USER.SLUG_GENERATED,
        color: 'blue',
        icon: <IconDice size={16} />,
      });
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message:
          error.response?.data?.error?.message || NOTIFICATION_MESSAGES.USER.SLUG_GENERATE_FAILED,
        color: 'red',
        icon: <IconX size={16} />,
      });
    }
  };

  // Update form when user data is loaded
  useEffect(() => {
    if (currentUserResponse?.responseBody?.user?.slug) {
      form.setFieldValue('slug', currentUserResponse.responseBody.user.slug);
    }
  }, [currentUserResponse?.responseBody?.user?.slug]);

  return (
    <Card withBorder radius="md" p="xl" shadow="sm" className={classes.card}>
      <Text fz="lg" className={classes.title} fw={700}>
        Profile Settings
      </Text>
      <Text fz="xs" c="dimmed" mt={3} mb="xl">
        Customize your profile information
      </Text>

      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput
          {...form.getInputProps('slug')}
          label="Profile Slug"
          description="This will be used in your profile URL: /profile?user=your-slug. Max 20 characters."
          placeholder="Enter your custom slug"
          maxLength={20}
          mb="md"
          rightSection={
            <Button
              size="sm"
              variant="subtle"
              leftSection={<IconDice size={14} />}
              onClick={handleGenerateRandomSlug}
              loading={isGeneratingSlug}
              disabled={isPending}
            >
              Random
            </Button>
          }
          rightSectionWidth={112}
        />

        {currentUserResponse?.responseBody?.user?.slug && (
          <Alert variant="light" color="blue" mb="md">
            Current profile URL:{' '}
            <code>/profile?user={currentUserResponse.responseBody.user.slug}</code>
          </Alert>
        )}

        <Group justify="flex-end">
          <Button type="submit" loading={isPending} disabled={!form.isDirty()}>
            Update Slug
          </Button>
        </Group>
      </form>
    </Card>
  );
}

export default function SettingsPage() {
  return (
    <Grid gutter={{ base: 5, xs: 'md', md: 'xl' }}>
      <Grid.Col span={{ base: 12, sm: 6, md: 6, lg: 6 }}>
        <ProfileSettings />
      </Grid.Col>
    </Grid>
  );
}
