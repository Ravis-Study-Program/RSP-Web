import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  GetCurrentUserMenteesListResponseApiResponse,
  MentorshipResponse,
  SeasonStudentRolePromotion,
  UpdateStudentRolePromotionRequest,
  UpdateStudentRolePromotionResponseApiResponse,
  useGetCurrentUser,
} from '@/generated/api/client';
import { SeasonStudentRolePromotionReverseIndex } from '@/shared/entities/reverseIndex';

const schema = z.object({
  name: z.string(),
  studentRolePromotion: z.string(),
});

export const StudentRolePromotionUpdateModal = ({
  table,
  row: { original: mentee },
  updateStudentRolePromotion,
  refetchMentees,
  seasonSlug,
}: studentRolePromotionUpdateModalProps) => {
  const { data: userResponse } = useGetCurrentUser();
  const email = userResponse?.responseBody?.user.email ?? '';

  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: mentee.menteeName,
      studentRolePromotion: SeasonStudentRolePromotionReverseIndex[mentee.studentRolePromotion],
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: { studentRolePromotion: string }) => {
    try {
      const requestData: UpdateStudentRolePromotionRequest = {
        menteeEnrollmentId: mentee.menteeEnrollmentId,
        seasonSlug,
        email,
        studentRolePromotion:
          SeasonStudentRolePromotion[
            values.studentRolePromotion as keyof typeof SeasonStudentRolePromotion
          ],
      };
      await updateStudentRolePromotion({ data: requestData });
      await refetchMentees();
      table.setEditingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Mentorship updated successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as UpdateStudentRolePromotionResponseApiResponse;
      notifications.show({
        color: 'red',
        title: 'Error',
        autoClose: false,
        message: response.error?.message,
      });
    }
  };

  const studentRolePromotionOptions = Object.entries(SeasonStudentRolePromotion).map(
    ([key, _]) => ({
      value: key,
      label: key,
    })
  );

  return (
    <Stack>
      <Title order={3} mt={15}>
        Update Student Role Promotion
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput {...form.getInputProps('name')} label="Mentee Name" withAsterisk disabled />
        <Select
          {...form.getInputProps('studentRolePromotion')}
          label="Select Student Role Promotion"
          placeholder="Pick a student role promotion"
          data={studentRolePromotionOptions}
          withAsterisk
          mt="sm"
          searchable
        />
        <Flex justify="flex-end">
          <Button type="submit" mt="xl" mb="md">
            Submit
          </Button>
        </Flex>
      </form>
    </Stack>
  );
};

type studentRolePromotionUpdateModalProps = {
  table: MRT_TableInstance<MentorshipResponse>;
  updateStudentRolePromotion: UseMutateAsyncFunction<
    UpdateStudentRolePromotionResponseApiResponse,
    unknown,
    {
      data: UpdateStudentRolePromotionRequest;
    },
    unknown
  >;
  row: MRT_Row<MentorshipResponse>;
  refetchMentees: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<GetCurrentUserMenteesListResponseApiResponse, unknown>>;
  seasonSlug: string;
};
