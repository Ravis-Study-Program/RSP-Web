import { Button } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import { createUser } from '@/generated/actions/client';
import { CreateUserRequest } from '@/generated/models';

export function TestPage() {
  const discordId = 'Discord ID';
  const email = 'email';
  const name = 'Name';
  const profileImage = 'Profile Image';

  const handleCreateUser = async () => {
    const newUser: CreateUserRequest = {
      discordId,
      email,
      name,
      profileImage,
    };

    try {
      await createUser(newUser);
    } catch (error) {
      console.log('Error creating user:', error);
    }
  };

  return (
    <Layout>
      <Button onClick={handleCreateUser}>Click Me</Button>
    </Layout>
  );
}
