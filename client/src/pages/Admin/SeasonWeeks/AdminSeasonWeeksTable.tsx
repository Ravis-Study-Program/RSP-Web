import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { Link } from 'react-router-dom';
import { ActionIcon, Anchor, Button, Flex, Text, Title, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  AdminDeleteSeasonWeekResponseApiResponse,
  SeasonWeekEntity,
  useAdminCreateSeasonWeek,
  useAdminDeleteSeasonWeek,
  useAdminListSeason,
  useAdminListSeasonWeek,
  useAdminUpdateSeasonWeek,
} from '@/generated/api/client';
import { AdminSeasonWeeksCreateModal } from './AdminSeasonWeeksCreateModal';
import { AdminSeasonWeeksUpdateModal } from './AdminSeasonWeeksUpdateModal';
import classes from './AdminSeasonWeeksTable.module.css';

export const AdminSeasonWeeksTable = () => {
  const [pageSize, setPageSize] = useLocalStorage({
    key: 'page-size',
    defaultValue: 10,
    getInitialValueInEffect: false,
  });

  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize,
  });

  const {
    data: seasonWeekResponse,
    isError: isLoadingSeasonWeeksError,
    isFetching: isFetchingSeasonWeeks,
    isLoading: isLoadingSeasonWeeks,
    refetch: refetchSeasonWeeks,
  } = useAdminListSeasonWeek();
  const { mutateAsync: createSeasonWeek, status: isCreatingSeasonWeekStatus } =
    useAdminCreateSeasonWeek();
  const { mutateAsync: updateSeasonWeek, status: isUpdatingSeasonWeekStatus } =
    useAdminUpdateSeasonWeek();
  const { mutateAsync: deleteSeasonWeek, status: isDeletingSeasonWeekStatus } =
    useAdminDeleteSeasonWeek();

  const {
    data: seasonResponse,
    isError: isLoadingSeasonsError,
    isFetching: isFetchingSeasons,
    isLoading: isLoadingSeasons,
  } = useAdminListSeason();

  const openDeleteConfirmModal = (row: MRT_Row<SeasonWeekEntity>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            Delete Season Week
          </Title>
          <Text>
            Are you sure you want to delete this Season Week? This action cannot be undone.
          </Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deleteSeasonWeek({ data: { seasonWeekId: row.original.seasonWeekId! } });
          await refetchSeasonWeeks();
          modals.closeAll();
          notifications.show({
            color: 'green',
            title: 'Success',
            message: 'SeasonWeek deleted successfully.',
          });
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteSeasonWeekResponseApiResponse;
          notifications.show({
            color: 'red',
            title: 'Error',
            autoClose: true,
            message: response.error?.message,
          });
        }
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<SeasonWeekEntity>[]>(
    () => [
      {
        accessorKey: 'seasonId',
        accessorFn: (row) => row.season?.slug,
        header: 'Season',
        Cell: ({ row }) => {
          return (
            <Anchor component={Link} to={`/seasons/${row.original.season?.slug}`}>
              {row.original.season?.name}
            </Anchor>
          );
        },
      },
      {
        accessorKey: 'weekNumber',
        header: 'Week Number',
      },
      {
        accessorKey: 'startDate',
        accessorFn: (row) => dayjs(row.startDate).format('D MMM YYYY'),
        header: 'Start Date',
        Cell: ({ row }) => {
          const startFormatted = dayjs(row.original.startDate).format('D MMM YYYY');
          return startFormatted;
        },
      },
      {
        accessorKey: 'endDate',
        accessorFn: (row) => dayjs(row.endDate).format('D MMM YYYY'),
        header: 'End Date',
        Cell: ({ row }) => {
          const endFormatted = dayjs(row.original.endDate).format('D MMM YYYY');
          return <Text size="sm">{endFormatted}</Text>;
        },
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: seasonWeekResponse?.responseBody?.seasonWeeks ?? [],
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
    onPaginationChange: (updater) => {
      const next = typeof updater === 'function' ? updater(pagination) : updater;
      setPageSize(next.pageSize);
      setPagination(next);
    },
    editDisplayMode: 'modal',
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'seasonId',
          desc: true,
        },
        {
          id: 'weekNumber',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.seasonWeekId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingSeasonWeeksError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <AdminSeasonWeeksCreateModal
        table={table}
        seasons={seasonResponse?.responseBody?.seasons || []}
        createSeasonWeek={createSeasonWeek}
        refetchSeasonWeeks={refetchSeasonWeeks}
      />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <AdminSeasonWeeksUpdateModal
        table={table}
        row={row}
        seasons={seasonResponse?.responseBody?.seasons || []}
        updateSeasonWeek={updateSeasonWeek}
        refetchSeasonWeeks={refetchSeasonWeeks}
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
        Create New Season Week
      </Button>
    ),
    state: {
      pagination,
      isLoading: isLoadingSeasonWeeks || isLoadingSeasons,
      isSaving:
        isCreatingSeasonWeekStatus === 'pending' ||
        isUpdatingSeasonWeekStatus === 'pending' ||
        isDeletingSeasonWeekStatus === 'pending',
      showAlertBanner: isLoadingSeasonWeeksError || isLoadingSeasonsError,
      showProgressBars: isFetchingSeasonWeeks || isFetchingSeasons,
    },
  });

  return <MantineReactTable table={table} />;
};
