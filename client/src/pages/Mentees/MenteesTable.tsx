import { useMemo } from 'react';
import { IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Flex, Text, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import { Mentorship, useGetCurrentUserMenteesList } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
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

  // TODO: Kick mentee
  const openKickMenteeConfirmModal = (_: MRT_Row<Mentorship>) => {
    modals.openConfirmModal({
      title: 'Delete Mentee',
      children: (
        <Text>
          Are you sure you want to kick this mentee out of RSP? This action cannot be undone.
        </Text>
      ),
      labels: { confirm: 'Kick Mentee', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await refetchMentees();
        modals.closeAll();
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<Mentorship>[]>(
    () => [
      {
        accessorKey: 'menteeEnrollment.user.name',
        header: 'Name',
      },
      {
        accessorKey: 'menteeEnrollment.user.email',
        header: 'Name',
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: menteeResponse?.responseBody?.mentees ?? [],
    mantinePaperProps: {
      className: classes.table,
    },
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'name',
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
    renderRowActions: ({ row }) => (
      <Flex gap="md">
        <Tooltip label="Delete">
          <ActionIcon variant="subtle" color="red" onClick={() => openKickMenteeConfirmModal(row)}>
            <IconTrash />
          </ActionIcon>
        </Tooltip>
      </Flex>
    ),
    state: {
      isLoading: isLoadingMentees,
      showAlertBanner: isLoadingMenteesError,
      showProgressBars: isFetchingMentees,
    },
  });

  return <MantineReactTable table={table} />;
};
