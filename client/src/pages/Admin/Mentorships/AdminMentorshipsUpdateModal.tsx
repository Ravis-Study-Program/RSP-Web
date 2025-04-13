import { useEffect, useState } from 'react';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  AdminListMentorshipResponseApiResponse,
  AdminUpdateMentorshipRequest,
  AdminUpdateMentorshipResponseApiResponse,
  EnrollmentResponseDto,
  MentorshipResponse,
  SeasonEntity,
  SeasonRole,
} from '@/generated/api/client';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const schema = z.object({
  seasonId: z.string(),
  mentorEnrollmentId: z.string(),
  menteeEnrollmentId: z.string(),
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
      seasonId: mentorship.seasonId,
      mentorEnrollmentId: mentorship.mentorEnrollmentId,
      menteeEnrollmentId: mentorship.menteeEnrollmentId,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    mentorEnrollmentId: string;
    menteeEnrollmentId: string;
  }) => {
    try {
      const requestData: AdminUpdateMentorshipRequest = {
        mentorshipId: mentorship.mentorshipId || '',
        mentorEnrollmentId: values.mentorEnrollmentId,
        menteeEnrollmentId: values.menteeEnrollmentId,
      };
      await updateMentorship({ data: requestData });
      await refetchMentorships();
      table.setEditingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Mentorship updated successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as AdminUpdateMentorshipResponseApiResponse;
      notifications.show({
        color: 'red',
        title: 'Error',
        autoClose: false,
        message: response.error?.message,
      });
    }
  };

  const [mentorOptions, setMentorOptions] = useState<{ value: string; label: string }[]>(
    enrollments
      ?.filter((e) => e.seasonId === mentorship.seasonId && e.role === SeasonRole.Mentor)
      .map((enrollment) => ({
        value: enrollment.enrollmentId,
        label: enrollment.userName,
      })) || []
  );
  const [menteeOptions, setMenteeOptions] = useState<{ value: string; label: string }[]>(
    enrollments
      ?.filter((e) => e.seasonId === mentorship.seasonId && e.role === SeasonRole.Student)
      .map((enrollment) => ({
        value: enrollment.enrollmentId,
        label: enrollment.userName,
      })) || []
  );

  form.watch('seasonId', ({ value }) => {
    // Reset form
    form.setValues({ mentorEnrollmentId: '', menteeEnrollmentId: '' });

    const mentors =
      enrollments
        ?.filter((e) => e.seasonId === value && e.role === SeasonRole.Mentor)
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.userName,
        })) || [];

    const mentees =
      enrollments
        ?.filter((e) => e.seasonId === value && e.role === SeasonRole.Student)
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.userName,
        })) || [];

    setMentorOptions(mentors);
    setMenteeOptions(mentees);
  });

  useEffect(() => {
    const mentors =
      enrollments
        ?.filter((e) => e.seasonId === mentorship.seasonId && e.role === SeasonRole.Mentor)
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.userName,
        })) || [];

    const mentees =
      enrollments
        ?.filter((e) => e.seasonId === mentorship.seasonId && e.role === SeasonRole.Student)
        .map((enrollment) => ({
          value: enrollment.enrollmentId,
          label: enrollment.userName,
        })) || [];

    setMentorOptions(mentors);
    setMenteeOptions(mentees);
  }, []);

  const seasonOptions =
    seasons?.map((season) => ({
      value: season.seasonId,
      label: season.name,
    })) || [];

  return (
    <Stack>
      <Title order={3} mt={15}>
        Update Mentorship
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Select
          {...form.getInputProps('seasonId')}
          label="Select Season"
          placeholder="Pick a season"
          data={seasonOptions}
          filter={createOptionsFilter()}
          withAsterisk
          searchable
        />
        <Select
          {...form.getInputProps('mentorEnrollmentId')}
          label="Select Mentor"
          placeholder={mentorOptions.length === 0 ? 'No mentors found' : 'Pick a mentor'}
          data={mentorOptions}
          filter={createOptionsFilter()}
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
          filter={createOptionsFilter()}
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
    AdminUpdateMentorshipResponseApiResponse,
    unknown,
    {
      data: AdminUpdateMentorshipRequest;
    },
    unknown
  >;
  row: MRT_Row<MentorshipResponse>;
  refetchMentorships: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListMentorshipResponseApiResponse, unknown>>;
  seasons: SeasonEntity[] | null | undefined;
  enrollments: EnrollmentResponseDto[] | null | undefined;
};
