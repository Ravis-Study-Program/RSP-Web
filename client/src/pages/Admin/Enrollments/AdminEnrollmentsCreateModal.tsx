import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import {
  AdminCreateEnrollmentRequest,
  AdminCreateEnrollmentResponseApiResult,
  AdminListEnrollmentResponseApiResult,
  EnrollmentResponse,
  SeasonEntity,
  SeasonRole,
  UserEntity,
} from '@/generated/api/client';

const schema = z.object({
  seasonId: z.string(),
  userId: z.string(),
  role: z.string(),
});

export const AdminEnrollmentsCreateModal = ({
  table,
  createEnrollment,
  refetchEnrollments,
  seasons,
  users,
}: AdminEnrollmentsCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      seasonId: '',
      userId: '',
      role: '',
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: { seasonId: string; userId: string; role: string }) => {
    try {
      const requestData: AdminCreateEnrollmentRequest = {
        ...values,
        role: SeasonRole[values.role as keyof typeof SeasonRole],
      };
      await createEnrollment({ data: requestData });
      await refetchEnrollments();
      table.setCreatingRow(null);
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

  const roleOptions = Object.entries(SeasonRole).map(([key, _]) => ({
    value: key,
    label: key,
  }));

  const userOptions =
    users?.map((user) => ({
      value: user.userId,
      label: user.name,
    })) || [];

  return (
    <Stack>
      <Title order={3}>Create Enrollment</Title>
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

type AdminEnrollmentsCreateModalProps = {
  table: MRT_TableInstance<EnrollmentResponse>;
  createEnrollment: UseMutateAsyncFunction<
    AdminCreateEnrollmentResponseApiResult,
    AdminCreateEnrollmentResponseApiResult,
    {
      data: AdminCreateEnrollmentRequest;
    },
    unknown
  >;
  refetchEnrollments: (
    options?: RefetchOptions
  ) => Promise<
    QueryObserverResult<AdminListEnrollmentResponseApiResult, AdminListEnrollmentResponseApiResult>
  >;
  seasons: SeasonEntity[] | null | undefined;
  users: UserEntity[] | null | undefined;
};
