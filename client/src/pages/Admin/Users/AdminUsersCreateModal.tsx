import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Checkbox, Flex, Stack, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  AdminCreateUserRequest,
  AdminCreateUserResponseApiResponse,
  AdminListUserResponseApiResponse,
  UserEntity,
} from '@/generated/api/client';

const schema = z.object({
  name: z.string().min(1),
  email: z.string().email().min(1),
  isAdmin: z.boolean(),
  discordId: z.string(),
  profileImage: z.string().url().or(z.literal('')).optional(),
});

export const AdminUsersCreateModal = ({
  table,
  createUser,
  refetchUsers,
}: AdminUsersCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: '',
      email: '',
      isAdmin: false,
      discordId: '',
      profileImage: '',
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
      const requestData: AdminCreateUserRequest = values;
      await createUser({ data: requestData });
      await refetchUsers();
      table.setCreatingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'User created successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as AdminCreateUserResponseApiResponse;
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
        Create User
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
        <Flex justify="flex-end">
          <Button type="submit" mt="xl" mb="md">
            Submit
          </Button>
        </Flex>
      </form>
    </Stack>
  );
};

type AdminUsersCreateModalProps = {
  table: MRT_TableInstance<UserEntity>;
  createUser: UseMutateAsyncFunction<
    AdminCreateUserResponseApiResponse,
    unknown,
    {
      data: AdminCreateUserRequest;
    },
    unknown
  >;
  refetchUsers: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListUserResponseApiResponse, unknown>>;
};
