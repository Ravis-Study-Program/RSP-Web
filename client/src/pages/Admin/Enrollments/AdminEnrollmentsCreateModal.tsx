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
  Role,
  Season,
  User,
} from '@/generated/api/client';

const schema = z.object({
  seasonId: z.string().uuid().min(1),
  userId: z.string().uuid().min(1),
  roleId: z.string().uuid().min(1),
});

export const AdminEnrollmentsCreateModal = ({
  table,
  createEnrollment,
  refetchEnrollments,
  seasons,
  roles,
  users,
}: AdminEnrollmentsCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      seasonId: '',
      userId: '',
      roleId: '',
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: { seasonId: string; userId: string; roleId: string }) => {
    try {
      const requestData: AdminCreateEnrollmentRequest = values;
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
      value: season.seasonId || '',
      label: season.name,
    })) || [];

  const roleOptions =
    roles?.map((role) => ({
      value: role.roleId || '',
      label: role.name,
    })) || [];

  const userOptions =
    users?.map((user) => ({
      value: user.userId || '',
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
          {...form.getInputProps('roleId')}
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
  seasons: Season[] | null | undefined;
  roles: Role[] | null | undefined;
  users: User[] | null | undefined;
};
