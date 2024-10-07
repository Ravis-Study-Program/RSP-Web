import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Checkbox, Flex, Stack, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import {
  AdminListUserResponseApiResult,
  AdminUpdateUserRequest,
  AdminUpdateUserResponseApiResult,
  User,
} from '@/generated/api/client';

const schema = z.object({
  name: z.string().min(1),
  email: z.string().email().min(1),
  isAdmin: z.boolean(),
  discordId: z.string().min(1),
  profileImage: z.string().url(),
});

export const AdminUsersUpdateModal = ({
  table,
  row: { original: user },
  updateUser,
  refetchUsers,
}: AdminUsersUpdateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: user.name,
      email: user.email,
      isAdmin: user.isAdmin,
      discordId: user.discordId,
      profileImage: user.profileImage,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    name: string;
    email: string;
    isAdmin: boolean;
    discordId: string;
    profileImage: string;
  }) => {
    try {
      const requestData: AdminUpdateUserRequest = values;
      await updateUser({ data: requestData });
      await refetchUsers();
      table.setEditingRow(null);
    } catch (err: unknown) {
      // eslint-disable-next-line no-console
      console.log(err);
    }
  };

  return (
    <Stack>
      <Title order={3}>Update User</Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput
          {...form.getInputProps('name')}
          label="Name"
          placeholder="Enter user name"
          withAsterisk
        />
        <TextInput
          {...form.getInputProps('email')}
          mt="sm"
          label="Email"
          placeholder="Enter user email"
          withAsterisk
        />
        <TextInput
          {...form.getInputProps('discordId')}
          mt="sm"
          label="Discord ID"
          placeholder="Enter user Discord ID"
          withAsterisk
        />
        <TextInput
          {...form.getInputProps('profileImage')}
          mt="sm"
          label="Profile Image"
          placeholder="Enter user Profile Image"
          withAsterisk
        />
        <Checkbox
          {...form.getInputProps('isAdmin', { type: 'checkbox' })}
          mt="sm"
          label="I like this user to be an admin"
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

type AdminUsersUpdateModalProps = {
  table: MRT_TableInstance<User>;
  updateUser: UseMutateAsyncFunction<
    AdminUpdateUserResponseApiResult,
    AdminUpdateUserResponseApiResult,
    {
      data: AdminUpdateUserRequest;
    },
    unknown
  >;
  row: MRT_Row<User>;
  refetchUsers: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListUserResponseApiResult, AdminListUserResponseApiResult>>;
};
