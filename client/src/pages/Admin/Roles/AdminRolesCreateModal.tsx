import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Stack, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import {
  AdminCreateRoleRequest,
  AdminCreateRoleResponseApiResult,
  AdminListRoleResponseApiResult,
  Role,
} from '@/generated/api/client';

const schema = z.object({
  name: z.string().min(1),
});

export const AdminRolesCreateModal = ({
  table,
  createRole,
  refetchRoles,
}: AdminRolesCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: '',
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: { name: string }) => {
    try {
      const requestData: AdminCreateRoleRequest = values;
      await createRole({ data: requestData });
      await refetchRoles();
      table.setCreatingRow(null);
    } catch (err: unknown) {
      // eslint-disable-next-line no-console
      console.log(err);
    }
  };

  return (
    <Stack>
      <Title order={3}>Create Role</Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput
          {...form.getInputProps('name')}
          label="Name"
          placeholder="Enter role name"
          withAsterisk
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

type AdminRolesCreateModalProps = {
  table: MRT_TableInstance<Role>;
  createRole: UseMutateAsyncFunction<
    AdminCreateRoleResponseApiResult,
    AdminCreateRoleResponseApiResult,
    {
      data: AdminCreateRoleRequest;
    },
    unknown
  >;
  refetchRoles: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListRoleResponseApiResult, AdminListRoleResponseApiResult>>;
};
