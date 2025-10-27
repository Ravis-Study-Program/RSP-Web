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
import { ActionIcon, Anchor, Badge, Button, Flex, Text, Title, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  AdminDeleteSeasonResponseApiResponse,
  SeasonEntity,
  useAdminCreateSeason,
  useAdminDeleteSeason,
  useAdminListSeason,
  useAdminUpdateSeason,
} from '@/generated/api/client';
import {
  getConfirmModalProps,
  getErrorNotification,
  getMantineTablePropsWithBanner,
  getSuccessNotification,
  NOTIFICATION_MESSAGES,
} from '@/shared/constants/mantineTableProps';
import { CONFIRMATION_MESSAGES } from '@/shared/constants/messages';
import classes from '@/shared/styles/tableStyles.module.css';
import { AdminSeasonsCreateModal } from './AdminSeasonsCreateModal';
import { AdminSeasonsUpdateModal } from './AdminSeasonsUpdateModal';

export const AdminSeasonsTable = () => {
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
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            {CONFIRMATION_MESSAGES.SEASON.DELETE_TITLE}
          </Title>
          <Text>{CONFIRMATION_MESSAGES.SEASON.DELETE_TEXT}</Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      ...getConfirmModalProps(),
      onConfirm: async () => {
        try {
          await deleteSeason({ data: { seasonId: row.original.seasonId! } });
          await refetchSeasons();
          modals.closeAll();
          notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.SEASON.DELETED));
        } catch (err) {
          const response = (err as any)?.response.data as AdminDeleteSeasonResponseApiResponse;
          notifications.show(getErrorNotification(response.error?.message));
        }
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
          return (
            <Anchor component={Link} to={`/seasons/${row.original.slug}`}>
              {row.original.name}
            </Anchor>
          );
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
        header: 'Image URL',
        Cell: ({ row }) => {
          const url = row.original.imageUrl;
          if (!url) {
            return (
              <Text size="sm" c="dimmed">
                -
              </Text>
            );
          }
          return (
            <Anchor href={url} target="_blank" size="sm">
              {url.length > 30 ? `${url.substring(0, 30)}...` : url}
            </Anchor>
          );
        },
      },
      {
        accessorKey: 'resourcesUrl',
        header: 'Resources URL',
        Cell: ({ row }) => {
          const url = row.original.resourcesUrl;
          if (!url) {
            return (
              <Text size="sm" c="dimmed">
                -
              </Text>
            );
          }
          return (
            <Anchor href={url} target="_blank" size="sm">
              {url.length > 30 ? `${url.substring(0, 30)}...` : url}
            </Anchor>
          );
        },
      },
      {
        accessorKey: 'isDataBackFilled',
        header: 'Backfilled',
        Cell: ({ row }) => {
          return (
            <Badge color={row.original.isDataBackFilled ? 'green' : 'gray'} size="sm">
              {row.original.isDataBackFilled ? 'Yes' : 'No'}
            </Badge>
          );
        },
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: seasonResponse?.responseBody?.seasons ?? [],
    ...getMantineTablePropsWithBanner(classes.table, isLoadingSeasonsError),
    createDisplayMode: 'modal',
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
          id: 'name',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.seasonId?.toString(),
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
      pagination,
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
