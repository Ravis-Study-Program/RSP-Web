import { useEffect, useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { Button, Container, Flex, Group, Text, Title } from '@mantine/core';
import { CreateUserIfNotExistsRequest, useCreateUserIfNotExists } from '@/generated/api/client';
import classes from './EmailVerificiation.module.css';

export function EmailVerificiationPage() {
  const { logout, user, loginWithRedirect } = useAuth0();
  const { mutateAsync: createUser } = useCreateUserIfNotExists();

  const WAIT_TIME = 10;
  const [countdown, setCountdown] = useState(WAIT_TIME);
  const [canResend, setCanResend] = useState(false);

  useEffect(() => {
    if (countdown <= 0) {
      setCanResend(true);
      return;
    }

    const timer = setInterval(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    return () => clearInterval(timer);
  }, [countdown]);

  const handleResend = async () => {
    setCanResend(false);
    setCountdown(WAIT_TIME);
    try {
      const request: CreateUserIfNotExistsRequest = {
        name: user?.nickname || 'New User',
      };
      await createUser({ data: request });
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error('User creation error:', err);
    }
  };

  return (
    <Flex justify="center" direction="column" align="center" className={classes.root}>
      <Container>
        <Title className={classes.title}>You're almost there!</Title>
        <Text size="lg" ta="center" mt="xl" className={classes.description}>
          We've sent a verification email to activate your account.
        </Text>
        <Text size="lg" ta="center" my="lg" mb="xl" className={classes.description}>
          Please check your inbox and click the link to verify your email. Once you've done that,
          come back and click the button below.
        </Text>
        <Group justify="center">
          <Button
            mt="xl"
            onClick={() =>
              loginWithRedirect({
                appState: { returnTo: '/' },
                authorizationParams: {
                  prompt: 'login',
                },
              })
            }
            autoContrast
            color="green.8"
            size="md"
          >
            I Have Verified My Email
          </Button>
          <Button
            onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
            mt="xl"
            autoContrast
            color="gray.4"
            size="md"
          >
            Back To Home
          </Button>
        </Group>

        <Group justify="center" mt="xl">
          {!canResend ? (
            <Text c="gray.1" size="sm">
              You can resend a verification email in {countdown} second{countdown !== 1 ? 's' : ''}.
            </Text>
          ) : (
            <Button onClick={handleResend} color="dark.8" size="md">
              Resend Verification Email
            </Button>
          )}
        </Group>
      </Container>
      <Text c="gray.3" size="sm" pos="absolute" bottom={20}>
        Ravi Study Program
      </Text>
    </Flex>
  );
}
