import { useEffect, useState } from 'react';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import {
  AdminListMentorshipResponseApiResult,
  AdminUpdateMentorshipRequest,
  AdminUpdateMentorshipResponseApiResult,
  EnrollmentResponse,
  MentorshipResponse,
  Season,
} from '@/generated/api/client';

const schema = z.object({
  seasonId: z.string().uuid().min(1),
  mentorEnrollmentId: z.string().uuid().min(1),
  menteeEnrollmentId: z.string().uuid().min(1),
});

export const AdminMentorshipsUpdateModal = ({
  table,
  row: { original: mentorship },
  updateMentorship,
  refetchMentorships,
  seasons,
  enrollments,
}: AdminMentorshipsUpdateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      seasonId: mentorship.mentor.seasonId,
      mentorEnrollmentId: mentorship.mentor.enrollmentId,
      menteeEnrollmentId: mentorship.mentee.enrollmentId,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    seasonId: string;
    mentorEnrollmentId: string;
    menteeEnrollmentId: string;
  }) => {
    try {
      const requestData: AdminUpdateMentorshipRequest = {
        mentorshipId: mentorship.mentorshipId,
        mentorEnrollmentId: values.mentorEnrollmentId,
        menteeEnrollmentId: values.menteeEnrollmentId,
      };
      await updateMentorship({ data: requestData });
      await refetchMentorships();
      table.setEditingRow(null);
    } catch (err: unknown) {
      // eslint-disable-next-line no-console
      console.log(err);
    }
  };

  const [mentorOptions, setMentorOptions] = useState<{ value: string; label: string }[]>(
    enrollments
      ?.filter((e) => e.seasonId === mentorship.mentor.seasonId && e.role === 'Mentor')
      .map((enrollment) => ({
        value: enrollment.enrollmentId,
        label: enrollment.user,
      })) || []
  );
  const [menteeOptions, setMenteeOptions] = useState<{ value: string; label: string }[]>(
    enrollments
      ?.filter((e) => e.seasonId === mentorship.mentor.seasonId && e.role === 'Student')
      .map((enrollment) => ({
        value: enrollment.enrollmentId,
        label: enrollment.user,
      })) || []
  );

  form.watch('seasonId', ({ value }) => {
    // Reset form
    form.setValues({ mentorEnrollmentId: '', menteeEnrollmentId: '' });

    const mentors =
      enrollments
        ?.filter((e) => e.seasonId === value && e.role === 'Mentor')
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.user,
        })) || [];

    const mentees =
      enrollments
        ?.filter((e) => e.seasonId === value && e.role === 'Student')
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.user,
        })) || [];

    setMentorOptions(mentors);
    setMenteeOptions(mentees);
  });

  useEffect(() => {
    const mentors =
      enrollments
        ?.filter((e) => e.seasonId === mentorship.mentor.seasonId && e.role === 'Mentor')
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.user,
        })) || [];

    const mentees =
      enrollments
        ?.filter((e) => e.seasonId === mentorship.mentor.seasonId && e.role === 'Student')
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.user,
        })) || [];

    setMentorOptions(mentors);
    setMenteeOptions(mentees);
  }, []);

  const seasonOptions =
    seasons?.map((season) => ({
      value: season.seasonId || '',
      label: season.name,
    })) || [];

  return (
    <Stack>
      <Title order={3}>Update Mentorship</Title>
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
          {...form.getInputProps('mentorEnrollmentId')}
          label="Select Mentor"
          placeholder={mentorOptions.length === 0 ? 'No mentors found' : 'Pick a mentor'}
          data={mentorOptions}
          withAsterisk
          mt="sm"
          searchable
          disabled={mentorOptions.length === 0}
        />
        <Select
          {...form.getInputProps('menteeEnrollmentId')}
          label="Select Mentee"
          placeholder={menteeOptions.length === 0 ? 'No mentees found' : 'Pick a mentee'}
          data={menteeOptions}
          withAsterisk
          mt="sm"
          searchable
          disabled={menteeOptions.length === 0}
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

type AdminMentorshipsUpdateModalProps = {
  table: MRT_TableInstance<MentorshipResponse>;
  updateMentorship: UseMutateAsyncFunction<
    AdminUpdateMentorshipResponseApiResult,
    AdminUpdateMentorshipResponseApiResult,
    {
      data: AdminUpdateMentorshipRequest;
    },
    unknown
  >;
  row: MRT_Row<MentorshipResponse>;
  refetchMentorships: (
    options?: RefetchOptions
  ) => Promise<
    QueryObserverResult<AdminListMentorshipResponseApiResult, AdminListMentorshipResponseApiResult>
  >;
  seasons: Season[] | null | undefined;
  enrollments: EnrollmentResponse[] | null | undefined;
};
