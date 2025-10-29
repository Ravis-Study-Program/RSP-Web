import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Stack, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  GetCurrentUserMenteesListResponseApiResponse,
  KickStudentRequest,
  KickStudentResponseApiResponse,
  MentorshipResponse,
  useGetCurrentUser,
} from '@/generated/api/client';
import { CustomRichTextEditor } from '@/shared/components/RichTextEditor';
import {
  getErrorNotification,
  getSuccessNotification,
  NOTIFICATION_MESSAGES,
} from '@/shared/constants/mantineTableProps';

const schema = z.object({
  menteeName: z.string(),
  kickReason: z.string().min(1, { message: 'Kick reason is required' }),
});

export const KickStudentModal = ({
  row: { original: mentee },
  kickStudent,
  refetchMentorships,
  seasonSlug,
  onClose,
}: KickStudentModalProps) => {
  const { data: userResponse } = useGetCurrentUser();
  const userId = userResponse?.responseBody?.user.userId ?? '';

  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      menteeName: mentee.menteeName,
      kickReason: '',
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: { kickReason: string }) => {
    try {
      const requestData: KickStudentRequest = {
        menteeEnrollmentId: mentee.menteeEnrollmentId,
        seasonSlug,
        userId,
        kickReason: values.kickReason,
      };

      await kickStudent({ data: requestData });
      await refetchMentorships();
      onClose();
      notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.MENTEE.DELETED));
    } catch (err) {
      const response = (err as any)?.response.data as KickStudentResponseApiResponse;
      notifications.show(getErrorNotification(response.error?.message));
    }
  };

  return (
    <Stack>
      <Title order={3} mt={15}>
        Kick Student
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput
          {...form.getInputProps('menteeName')}
          label="Student Name"
          withAsterisk
          disabled
        />

        <CustomRichTextEditor
          content={form.values.kickReason}
          onChange={(value) => form.setFieldValue('kickReason', value || '')}
          label="Kick Reason"
          error={form.errors.kickReason?.toString()}
          required
          maxLength={2000}
          minHeight={150}
        />

        <Flex justify="flex-end" gap="md">
          <Button variant="outline" onClick={onClose} mt="xl" mb="md">
            Cancel
          </Button>
          <Button type="submit" color="red" mt="xl" mb="md">
            Kick Student
          </Button>
        </Flex>
      </form>
    </Stack>
  );
};

type KickStudentModalProps = {
  kickStudent: UseMutateAsyncFunction<
    KickStudentResponseApiResponse,
    unknown,
    {
      data: KickStudentRequest;
    },
    unknown
  >;
  row: MRT_Row<MentorshipResponse>;
  refetchMentorships: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<GetCurrentUserMenteesListResponseApiResponse, unknown>>;
  seasonSlug: string;
  onClose: () => void;
};
