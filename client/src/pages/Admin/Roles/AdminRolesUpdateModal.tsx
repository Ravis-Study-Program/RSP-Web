import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Stack, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import {
  AdminListRoleResponseApiResult,
  AdminUpdateRoleRequest,
  AdminUpdateRoleResponseApiResult,
  Role,
} from '@/generated/api/client';

const schema = z.object({
  name: z.string().min(1),
});

export const AdminRolesUpdateModal = ({
  table,
  row: { original: role },
  updateRole,
  refetchRoles,
}: AdminRolesUpdateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: role.name,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: { name: string }) => {
    try {
      const requestData: AdminUpdateRoleRequest = {
        ...values,
        roleId: role.roleId,
      };
      await updateRole({ data: requestData });
      await refetchRoles();
      table.setEditingRow(null);
    } catch (err: unknown) {
      // eslint-disable-next-line no-console
      console.log(err);
    }
  };

  return (
    <Stack>
      <Title order={3}>Update Role</Title>
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

type AdminRolesUpdateModalProps = {
  table: MRT_TableInstance<Role>;
  updateRole: UseMutateAsyncFunction<
    AdminUpdateRoleResponseApiResult,
    AdminUpdateRoleResponseApiResult,
    {
      data: AdminUpdateRoleRequest;
    },
    unknown
  >;
  row: MRT_Row<Role>;
  refetchRoles: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListRoleResponseApiResult, AdminListRoleResponseApiResult>>;
};
