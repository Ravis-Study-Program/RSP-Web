import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  AdminListEnrollmentResponseApiResponse,
  AdminUpdateEnrollmentRequest,
  AdminUpdateEnrollmentResponseApiResponse,
  EnrollmentResponseDto,
  SeasonEntity,
  SeasonRole,
  SeasonStudentRolePromotion,
  UserEntity,
} from '@/generated/api/client';
import {
  SeasonRoleReverseIndex,
  SeasonStudentRolePromotionReverseIndex,
} from '@/shared/entities/reverseIndex';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const schema = z.object({
  seasonId: z.string(),
  userId: z.string(),
  role: z.string(),
  studentRolePromotion: z.string(),
});

export const AdminEnrollmentsUpdateModal = ({
  table,
  row: { original: enrollment },
  updateEnrollment,
  refetchEnrollments,
  seasons,
  users,
}: AdminEnrollmentsUpdateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      seasonId: enrollment.seasonId,
      userId: enrollment.userId,
      role: SeasonRoleReverseIndex[enrollment.role],
      studentRolePromotion: SeasonStudentRolePromotionReverseIndex[enrollment.studentRolePromotion],
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    seasonId: string;
    userId: string;
    role: string;
    studentRolePromotion: string;
  }) => {
    try {
      const requestData: AdminUpdateEnrollmentRequest = {
        ...values,
        enrollmentId: enrollment.enrollmentId,
        role: SeasonRole[values.role as keyof typeof SeasonRole],
        studentRolePromotion:
          SeasonStudentRolePromotion[
            values.studentRolePromotion as keyof typeof SeasonStudentRolePromotion
          ],
      };
      await updateEnrollment({ data: requestData });
      await refetchEnrollments();
      table.setEditingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Enrollment updated successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as AdminUpdateEnrollmentResponseApiResponse;
      notifications.show({
        color: 'red',
        title: 'Error',
        autoClose: false,
        message: response.error?.message,
      });
    }
  };

  const roleOptions = Object.entries(SeasonRole).map(([key, _]) => ({
    value: key,
    label: key,
  }));

  const studentRolePromotionOptions = Object.entries(SeasonStudentRolePromotion).map(
    ([key, _]) => ({
      value: key,
      label: key,
    })
  );

  const seasonOptions =
    seasons?.map((season) => ({
      value: season.seasonId,
      label: season.name,
    })) || [];

  const userOptions =
    users?.map((user) => ({
      value: user.userId,
      label: user.name,
    })) || [];

  return (
    <Stack>
      <Title order={3} mt={15}>
        Update Enrollment
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
          {...form.getInputProps('userId')}
          label="Select User"
          placeholder="Pick a user"
          data={userOptions}
          filter={createOptionsFilter()}
          withAsterisk
          mt="sm"
          searchable
        />
        <Select
          {...form.getInputProps('role')}
          label="Select Role"
          placeholder="Pick a role"
          data={roleOptions}
          filter={createOptionsFilter()}
          withAsterisk
          mt="sm"
          searchable
        />
        <Select
          {...form.getInputProps('studentRolePromotion')}
          label="Select Student Role Promotion"
          placeholder="Pick a student role promotion"
          data={studentRolePromotionOptions}
          filter={createOptionsFilter()}
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

type AdminEnrollmentsUpdateModalProps = {
  table: MRT_TableInstance<EnrollmentResponseDto>;
  updateEnrollment: UseMutateAsyncFunction<
    AdminUpdateEnrollmentResponseApiResponse,
    unknown,
    {
      data: AdminUpdateEnrollmentRequest;
    },
    unknown
  >;
  row: MRT_Row<EnrollmentResponseDto>;
  refetchEnrollments: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListEnrollmentResponseApiResponse, unknown>>;
  seasons: SeasonEntity[] | null | undefined;
  users: UserEntity[] | null | undefined;
};
