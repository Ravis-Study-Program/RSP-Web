import { useMemo, useState } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import { QueryObserverResult, RefetchOptions } from '@tanstack/react-query';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Flex, Text, Title, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  GetCurrentUserMenteesListResponseApiResponse,
  KickStudentResponseApiResponse,
  MentorshipResponse,
  SeasonStudentRolePromotion,
  useGetCurrentUser,
  useKickStudent,
  useUpdateStudentRolePromotion,
} from '@/generated/api/client';
import {
  getConfirmModalProps,
  getErrorNotification,
  getMantineTablePropsWithBanner,
  getSuccessNotification,
  NOTIFICATION_MESSAGES,
} from '@/shared/constants/mantineTableProps';
import { CONFIRMATION_MESSAGES } from '@/shared/constants/messages';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import classes from '@/shared/styles/tableStyles.module.css';
import { StudentRolePromotionUpdateModal } from './StudentRolePromotionUpdateModal';

export const MenteesTable = ({ refetchMentorships, mentorships }: MenteesTableProps) => {
  const [pageSize, setPageSize] = useLocalStorage({
    key: 'page-size',
    defaultValue: 10,
    getInitialValueInEffect: false,
  });

  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize,
  });

  const { seasonSlug } = useSeasonSlug();
  const { data: userResponse } = useGetCurrentUser();
  const userId = userResponse?.responseBody?.user.userId ?? '';
  const { mutateAsync: kickStudent, status: isKickingStudentStatus } = useKickStudent();
  const { mutateAsync: updateStudentRolePromotion, status: isUpdatingStudentRolePromotionStatus } =
    useUpdateStudentRolePromotion();

  const openKickMenteeConfirmModal = (row: MRT_Row<MentorshipResponse>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            {CONFIRMATION_MESSAGES.MENTEE.DELETE_TITLE}
          </Title>
          <Text>{CONFIRMATION_MESSAGES.MENTEE.DELETE_TEXT}</Text>
        </>
      ),
      labels: { confirm: 'Kick Mentee', cancel: 'Cancel' },
      ...getConfirmModalProps(),
      onConfirm: async () => {
        try {
          await kickStudent({
            data: {
              seasonSlug,
              menteeEnrollmentId: row.original.menteeEnrollmentId,
              userId,
            },
          });
          await refetchMentorships();
          modals.closeAll();
          notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.MENTEE.DELETED));
        } catch (err) {
          const response = (err as any)?.response.data as KickStudentResponseApiResponse;
          notifications.show(getErrorNotification(response.error?.message));
        }
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<MentorshipResponse>[]>(
    () => [
      {
        accessorKey: 'menteeName',
        header: 'Name',
      },
      {
        accessorKey: 'studentRolePromotion',
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
    data: mentorships || [],
    ...getMantineTablePropsWithBanner(classes.table, false),
    createDisplayMode: 'modal',
    onPaginationChange: (updater) => {
      const next = typeof updater === 'function' ? updater(pagination) : updater;
      setPageSize(next.pageSize);
      setPagination(next);
    },
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'studentRolePromotion',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.menteeEnrollmentId?.toString(),
    isMultiSortEvent: () => true,
    renderEditRowModalContent: ({ table, row }) => (
      <StudentRolePromotionUpdateModal
        table={table}
        row={row}
        updateStudentRolePromotion={updateStudentRolePromotion}
        refetchMentorships={refetchMentorships}
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
      pagination,
      isSaving:
        isKickingStudentStatus === 'pending' || isUpdatingStudentRolePromotionStatus === 'pending',
    },
  });

  return <MantineReactTable table={table} />;
};

type MenteesTableProps = {
  refetchMentorships: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<GetCurrentUserMenteesListResponseApiResponse, unknown>>;
  mentorships: MentorshipResponse[] | null | undefined;
};
