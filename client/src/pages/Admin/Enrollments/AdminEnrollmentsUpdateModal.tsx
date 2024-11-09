import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import {
  AdminListEnrollmentResponseApiResult,
  AdminUpdateEnrollmentRequest,
  AdminUpdateEnrollmentResponseApiResult,
  EnrollmentResponse,
  SeasonEntity,
  SeasonRole,
  UserEntity,
} from '@/generated/api/client';

const schema = z.object({
  seasonId: z.string(),
  userId: z.string(),
  role: z.number(),
});

export const AdminEnrollmentsUpdateModal = ({
  table,
  row: { original: enrollment },
  updateEnrollment,
  refetchEnrollments,
  seasons,
  users,
}: AdminEnrollmentsUpdateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      seasonId: enrollment.seasonId,
      userId: enrollment.userId,
      role: enrollment.role,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: { seasonId: string; userId: string; role: SeasonRole }) => {
    try {
      const requestData: AdminUpdateEnrollmentRequest = {
        ...values,
        enrollmentId: enrollment.enrollmentId,
      };
      await updateEnrollment({ data: requestData });
      await refetchEnrollments();
      table.setEditingRow(null);
    } catch (err: unknown) {
      // eslint-disable-next-line no-console
      console.log(err);
    }
  };

  const seasonOptions =
    seasons?.map((season) => ({
      value: season.seasonId,
      label: season.name,
    })) || [];

  const roleOptions =
    Object.values(SeasonRole)?.map((role, index) => ({
      value: index.toString(),
      label: role.toString(),
    })) || [];

  const userOptions =
    users?.map((user) => ({
      value: user.userId,
      label: user.name,
    })) || [];

  return (
    <Stack>
      <Title order={3}>Update Enrollment</Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Select
          {...form.getInputProps('seasonId')}
          label="Select Season"
          placeholder="Pick a season"
          data={seasonOptions}
          withAsterisk
          searchable
        />
        <Select
          {...form.getInputProps('userId')}
          label="Select User"
          placeholder="Pick a user"
          data={userOptions}
          withAsterisk
          mt="sm"
          searchable
        />
        <Select
          {...form.getInputProps('role')}
          label="Select Role"
          placeholder="Pick a role"
          data={roleOptions}
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

type AdminEnrollmentsUpdateModalProps = {
  table: MRT_TableInstance<EnrollmentResponse>;
  updateEnrollment: UseMutateAsyncFunction<
    AdminUpdateEnrollmentResponseApiResult,
    AdminUpdateEnrollmentResponseApiResult,
    {
      data: AdminUpdateEnrollmentRequest;
    },
    unknown
  >;
  row: MRT_Row<EnrollmentResponse>;
  refetchEnrollments: (
    options?: RefetchOptions
  ) => Promise<
    QueryObserverResult<AdminListEnrollmentResponseApiResult, AdminListEnrollmentResponseApiResult>
  >;
  seasons: SeasonEntity[] | null | undefined;
  users: UserEntity[] | null | undefined;
};
