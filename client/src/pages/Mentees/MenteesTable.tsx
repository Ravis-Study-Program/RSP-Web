import { useMemo, useState } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import { QueryObserverResult, RefetchOptions } from '@tanstack/react-query';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Flex, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { ProfileLink } from '@/components/ProfileLink/ProfileLink';
import {
  GetCurrentUserMenteesListResponseApiResponse,
  MentorshipResponse,
  SeasonStudentRolePromotion,
  useKickStudent,
  useUpdateStudentRolePromotion,
} from '@/generated/api/client';
import { getMantineTablePropsWithBanner } from '@/shared/constants/mantineTableProps';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import classes from '@/shared/styles/tableStyles.module.css';
import { KickStudentModal } from './KickStudentModal';
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
  const { mutateAsync: kickStudent, status: isKickingStudentStatus } = useKickStudent();
  const { mutateAsync: updateStudentRolePromotion, status: isUpdatingStudentRolePromotionStatus } =
    useUpdateStudentRolePromotion();

  const openKickMenteeModal = (row: MRT_Row<MentorshipResponse>) => {
    modals.open({
      children: (
        <KickStudentModal
          row={row}
          kickStudent={kickStudent}
          refetchMentorships={refetchMentorships}
          seasonSlug={seasonSlug}
          onClose={() => modals.closeAll()}
        />
      ),
      size: 'lg',
      centered: true,
    });
  };

  const columns = useMemo<MRT_ColumnDef<MentorshipResponse>[]>(
    () => [
      {
        accessorKey: 'menteeName',
        header: 'Name',
        Cell: ({ row }) => {
          return (
            <ProfileLink userName={row.original.menteeName} userSlug={row.original.menteeSlug} />
          );
        },
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
          <ActionIcon variant="subtle" color="red" onClick={() => openKickMenteeModal(row)}>
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
