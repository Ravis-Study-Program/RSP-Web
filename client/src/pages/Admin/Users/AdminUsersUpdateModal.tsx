import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Checkbox, Flex, Stack, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  AdminListUserResponseApiResponse,
  AdminUpdateUserRequest,
  AdminUpdateUserResponseApiResponse,
  AdminUserDto,
} from '@/generated/api/client';

const schema = z.object({
  name: z.string().min(1),
  email: z.string().email().min(1),
  isAdmin: z.boolean(),
  isTestUser: z.boolean(),
  discordId: z.string().nullable().optional(),
  profileImage: z.string().url().or(z.literal('')).nullable().optional(),
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
      userId: user.userId,
      email: user.email,
      isAdmin: user.isAdmin,
      isTestUser: user.isTestUser,
      discordId: user.discordId,
      profileImage: user.profileImage,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    name: string;
    userId: string;
    email: string;
    isAdmin: boolean;
    isTestUser: boolean;
    discordId?: string | null;
    profileImage?: string | null;
  }) => {
    try {
      const requestData: AdminUpdateUserRequest = {
        ...values,
        userId: user.userId,
      };
      await updateUser({ data: requestData });
      await refetchUsers();
      table.setEditingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'User updated successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as AdminUpdateUserResponseApiResponse;
      notifications.show({
        color: 'red',
        title: 'Error',
        autoClose: false,
        message: response.error?.message,
      });
    }
  };

  return (
    <Stack>
      <Title order={3} mt={15}>
        Update User
      </Title>
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
        />
        <TextInput
          {...form.getInputProps('profileImage')}
          mt="sm"
          label="Profile Image"
          placeholder="Enter user Profile Image"
        />
        <Checkbox
          {...form.getInputProps('isAdmin', { type: 'checkbox' })}
          mt="sm"
          label="I like this user to be an admin"
        />
        <Checkbox
          {...form.getInputProps('isTestUser', { type: 'checkbox' })}
          mt="sm"
          label="Mark this user as a test user"
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
  table: MRT_TableInstance<AdminUserDto>;
  updateUser: UseMutateAsyncFunction<
    AdminUpdateUserResponseApiResponse,
    unknown,
    {
      data: AdminUpdateUserRequest;
    },
    unknown
  >;
  row: MRT_Row<AdminUserDto>;
  refetchUsers: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListUserResponseApiResponse, unknown>>;
};
