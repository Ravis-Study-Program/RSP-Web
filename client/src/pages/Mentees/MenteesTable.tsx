import { useMemo } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Flex, Text, Title, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  KickStudentResponseApiResult,
  MenteeResponseDto,
  SeasonStudentRolePromotion,
  useGetCurrentUserMenteesList,
  useKickStudent,
  useUpdateStudentRolePromotion,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { StudentRolePromotionUpdateModal } from './StudentRolePromotionUpdateModal';
import classes from './MenteesTable.module.css';

export const MenteesTable = () => {
  const { seasonSlug } = useSeasonSlug();
  const {
    data: menteeResponse,
    isError: isLoadingMenteesError,
    isFetching: isFetchingMentees,
    isLoading: isLoadingMentees,
    refetch: refetchMentees,
  } = useGetCurrentUserMenteesList(seasonSlug);
  const { mutateAsync: kickStudent, status: isKickingStudentStatus } = useKickStudent();
  const { mutateAsync: updateStudentRolePromotion, status: isUpdatingStudentRolePromotionStatus } =
    useUpdateStudentRolePromotion();

  const openKickMenteeConfirmModal = (row: MRT_Row<MenteeResponseDto>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            Delete Mentee
          </Title>
          <Text>
            Are you sure you want to kick this mentee out of RSP? This action cannot be undone.
          </Text>
        </>
      ),
      labels: { confirm: 'Kick Mentee', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await kickStudent({
            data: {
              seasonSlug,
              menteeEnrollmentId: row.original.menteeEnrollmentId,
            },
          });
          await refetchMentees();
          modals.closeAll();
          notifications.show({
            color: 'green',
            title: 'Success',
            message: 'Mentee deleted successfully.',
          });
        } catch (err) {
          const response = (err as any)?.response.data as KickStudentResponseApiResult;
          notifications.show({
            color: 'red',
            title: 'Error',
            autoClose: false,
            message: response.error?.message,
          });
        }
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<MenteeResponseDto>[]>(
    () => [
      {
        accessorKey: 'menteeName',
        header: 'Name',
      },
      {
        header: 'Student Role Promotion',
        accessorFn: (row) => {
          const roleKey = Object.keys(SeasonStudentRolePromotion).find(
            (key) =>
              SeasonStudentRolePromotion[key as keyof typeof SeasonStudentRolePromotion] ===
              row.studentRolePromotion
          ) as keyof typeof SeasonStudentRolePromotion;

          return roleKey ? roleKey : 'Unknown Role';
        },
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: menteeResponse?.responseBody?.mentees ?? [],
    createDisplayMode: 'modal',
    mantineEditRowModalProps: {
      closeOnClickOutside: false,
      withCloseButton: true,
      closeButtonProps: {
        className: classes.modalCloseButton,
      },
    },
    mantinePaperProps: {
      className: classes.table,
    },
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'menteeName',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.menteeEnrollmentId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingMenteesError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    renderEditRowModalContent: ({ table, row }) => (
      <StudentRolePromotionUpdateModal
        table={table}
        row={row}
        updateStudentRolePromotion={updateStudentRolePromotion}
        refetchMentees={refetchMentees}
        seasonSlug={seasonSlug}
      />
    ),
    renderRowActions: ({ row }) => (
      <Flex gap="md">
        <Tooltip label="Edit">
          <ActionIcon variant="subtle" onClick={() => table.setEditingRow(row)}>
            <IconEdit />
          </ActionIcon>
        </Tooltip>
        <Tooltip label="Delete">
          <ActionIcon variant="subtle" color="red" onClick={() => openKickMenteeConfirmModal(row)}>
            <IconTrash />
          </ActionIcon>
        </Tooltip>
      </Flex>
    ),
    state: {
      isLoading: isLoadingMentees,
      isSaving:
        isKickingStudentStatus === 'pending' || isUpdatingStudentRolePromotionStatus === 'pending',
      showAlertBanner: isLoadingMenteesError,
      showProgressBars: isFetchingMentees,
    },
  });

  return <MantineReactTable table={table} />;
};
