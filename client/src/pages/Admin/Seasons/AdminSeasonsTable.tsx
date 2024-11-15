import dayjs from 'dayjs';
import { useMemo } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Anchor, Button, Flex, Text, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import {
  SeasonEntity,
  useAdminCreateSeason,
  useAdminDeleteSeason,
  useAdminListSeason,
  useAdminUpdateSeason,
} from '@/generated/api/client';
import { AdminSeasonsCreateModal } from './AdminSeasonsCreateModal';
import { AdminSeasonsUpdateModal } from './AdminSeasonsUpdateModal';
import classes from './AdminSeasonsTable.module.css';

export const AdminSeasonsTable = () => {
  const {
    data: seasonResponse,
    isError: isLoadingSeasonsError,
    isFetching: isFetchingSeasons,
    isLoading: isLoadingSeasons,
    refetch: refetchSeasons,
  } = useAdminListSeason();
  const { mutateAsync: createSeason, status: isCreatingSeasonStatus } = useAdminCreateSeason();
  const { mutateAsync: updateSeason, status: isUpdatingSeasonStatus } = useAdminUpdateSeason();
  const { mutateAsync: deleteSeason, status: isDeletingSeasonStatus } = useAdminDeleteSeason();

  const openDeleteConfirmModal = (row: MRT_Row<SeasonEntity>) => {
    modals.openConfirmModal({
      title: 'Delete Season',
      children: (
        <Text>Are you sure you want to delete this season? This action cannot be undone.</Text>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await deleteSeason({ params: { seasonId: row.original.seasonId! } });
        await refetchSeasons();
        modals.closeAll();
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<SeasonEntity>[]>(
    () => [
      {
        accessorKey: 'name',
        accessorFn: (row) => row.name,
        header: 'Name',
        Cell: ({ row }) => {
          return <Anchor href={`/seasons/${row.original.slug}`}>{row.original.name}</Anchor>;
        },
      },
      {
        accessorKey: 'slug',
        header: 'Slug',
      },
      {
        accessorKey: 'startDateInclusiveUtc',
        accessorFn: (row) => dayjs(row.endDateInclusiveUtc).format('D MMM YYYY'),
        header: 'Start Date',
        Cell: ({ row }) => {
          const startFormatted = dayjs(row.original.startDateInclusiveUtc).format('D MMM YYYY');
          return startFormatted;
        },
      },
      {
        accessorKey: 'endDateInclusiveUtc',
        accessorFn: (row) => dayjs(row.endDateInclusiveUtc).format('D MMM YYYY'),
        header: 'End Date',
        Cell: ({ row }) => {
          const endFormatted = dayjs(row.original.endDateInclusiveUtc).format('D MMM YYYY');
          return <Text size="sm">{endFormatted}</Text>;
        },
      },
      {
        accessorKey: 'location',
        header: 'Location',
      },
      {
        accessorKey: 'imageUrl',
        header: 'Image Url',
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: seasonResponse?.responseBody?.seasons ?? [],
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
          id: 'startDateInclusiveUtc',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.seasonId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingSeasonsError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <AdminSeasonsCreateModal
        table={table}
        createSeason={createSeason}
        refetchSeasons={refetchSeasons}
      />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <AdminSeasonsUpdateModal
        table={table}
        row={row}
        updateSeason={updateSeason}
        refetchSeasons={refetchSeasons}
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
        Create New Season
      </Button>
    ),
    state: {
      isLoading: isLoadingSeasons,
      isSaving:
        isCreatingSeasonStatus === 'pending' ||
        isUpdatingSeasonStatus === 'pending' ||
        isDeletingSeasonStatus === 'pending',
      showAlertBanner: isLoadingSeasonsError,
      showProgressBars: isFetchingSeasons,
    },
  });

  return <MantineReactTable table={table} />;
};
