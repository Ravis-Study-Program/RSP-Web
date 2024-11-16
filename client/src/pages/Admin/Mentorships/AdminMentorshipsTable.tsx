import { useMemo } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Button, Flex, Text, Title, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  AdminDeleteMentorshipResponseApiResult,
  MentorshipResponse,
  useAdminCreateMentorship,
  useAdminDeleteMentorship,
  useAdminListEnrollment,
  useAdminListMentorship,
  useAdminListSeason,
  useAdminUpdateMentorship,
} from '@/generated/api/client';
import { AdminMentorshipsCreateModal } from './AdminMentorshipsCreateModal';
import { AdminMentorshipsUpdateModal } from './AdminMentorshipsUpdateModal';
import classes from './AdminMentorshipsTable.module.css';

export const AdminMentorshipsTable = () => {
  const {
    data: mentorshipResponse,
    isError: isLoadingMentorshipsError,
    isFetching: isFetchingMentorships,
    isLoading: isLoadingMentorships,
    refetch: refetchMentorships,
  } = useAdminListMentorship();
  const { mutateAsync: createMentorship, status: isCreatingMentorshipStatus } =
    useAdminCreateMentorship();
  const { mutateAsync: updateMentorship, status: isUpdatingMentorshipStatus } =
    useAdminUpdateMentorship();
  const { mutateAsync: deleteMentorship, status: isDeletingMentorshipStatus } =
    useAdminDeleteMentorship();
  const {
    data: seasonResponse,
    isError: isLoadingSeasonsError,
    isFetching: isFetchingSeasons,
    isLoading: isLoadingSeasons,
  } = useAdminListSeason();
  const {
    data: enrollmentResponse,
    isError: isLoadingEnrollmentsError,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useAdminListEnrollment();

  const openDeleteConfirmModal = (row: MRT_Row<MentorshipResponse>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            Delete Mentorship
          </Title>
          <Text>
            Are you sure you want to delete this mentorship? This action cannot be undone.
          </Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deleteMentorship({ params: { mentorshipId: row.original.mentorshipId! } });
          await refetchMentorships();
          modals.closeAll();
          notifications.show({
            color: 'green',
            title: 'Success',
            message: 'Mentorship deleted successfully.',
          });
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteMentorshipResponseApiResult;
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

  const columns = useMemo<MRT_ColumnDef<MentorshipResponse>[]>(
    () => [
      {
        header: 'Season',
        accessorFn: (row) => row.seasonName,
      },
      {
        header: 'Mentor',
        accessorFn: (row) => row.mentorName,
      },
      {
        header: 'Mentee',
        accessorFn: (row) => row.menteeName,
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: mentorshipResponse?.responseBody?.mentorships ?? [],
    mantinePaperProps: {
      className: classes.table,
    },
    createDisplayMode: 'modal',
    mantineCreateRowModalProps: {
      closeOnClickOutside: false,
      withCloseButton: true,
      closeButtonProps: {
        className: classes.modalCloseButton,
      },
    },
    mantineEditRowModalProps: {
      closeOnClickOutside: false,
      withCloseButton: true,
      closeButtonProps: {
        className: classes.modalCloseButton,
      },
    },
    editDisplayMode: 'modal',
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'Season',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.mentorshipId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingMentorshipsError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <AdminMentorshipsCreateModal
        table={table}
        createMentorship={createMentorship}
        refetchMentorships={refetchMentorships}
        seasons={seasonResponse?.responseBody?.seasons}
        enrollments={enrollmentResponse?.responseBody?.enrollments}
      />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <AdminMentorshipsUpdateModal
        table={table}
        row={row}
        updateMentorship={updateMentorship}
        refetchMentorships={refetchMentorships}
        seasons={seasonResponse?.responseBody?.seasons}
        enrollments={enrollmentResponse?.responseBody?.enrollments}
      />
    ),
    renderRowActions: ({ row, table }) => (
      <Flex gap="md">
        <Tooltip label="Edit">
          <ActionIcon variant="subtle" onClick={() => table.setEditingRow(row)}>
            <IconEdit />
          </ActionIcon>
        </Tooltip>
        <Tooltip label="Delete">
          <ActionIcon variant="subtle" color="red" onClick={() => openDeleteConfirmModal(row)}>
            <IconTrash />
          </ActionIcon>
        </Tooltip>
      </Flex>
    ),
    renderTopToolbarCustomActions: ({ table }) => (
      <Button
        onClick={() => {
          table.setCreatingRow(true);
        }}
      >
        Create New Mentorship
      </Button>
    ),
    state: {
      isLoading: isLoadingMentorships || isLoadingEnrollments || isLoadingSeasons,
      isSaving:
        isCreatingMentorshipStatus === 'pending' ||
        isUpdatingMentorshipStatus === 'pending' ||
        isDeletingMentorshipStatus === 'pending',
      showAlertBanner:
        isLoadingMentorshipsError || isLoadingEnrollmentsError || isLoadingSeasonsError,
      showProgressBars: isFetchingMentorships || isFetchingEnrollments || isFetchingSeasons,
    },
  });

  return <MantineReactTable table={table} />;
};
