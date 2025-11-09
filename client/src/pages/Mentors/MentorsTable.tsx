import { useMemo, useState } from 'react';
import { IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Flex, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import {
  MentorAssignmentDto,
  SeasonStudentRolePromotion,
  useKickStudent,
} from '@/generated/api/client';
import { getMantineTablePropsWithBanner } from '@/shared/constants/mantineTableProps';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import classes from '@/shared/styles/tableStyles.module.css';
import { KickStudentModal } from '../Mentees/KickStudentModal';

export const MentorsTable = ({
  mentorAssignments,
  refetchMentorAssignments,
}: MentorsTableProps) => {
  const [pageSize, setPageSize] = useLocalStorage({
    key: 'mentors-page-size',
    defaultValue: 10,
    getInitialValueInEffect: false,
  });

  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize,
  });

  const { seasonSlug } = useSeasonSlug();
  const { mutateAsync: kickStudent, status: isKickingStudentStatus } = useKickStudent();

  // All assignments are already student assignments, so we can use them directly
  const students = useMemo(() => {
    return mentorAssignments;
  }, [mentorAssignments]);

  const openKickStudentModal = (row: MRT_Row<MentorAssignmentDto>) => {
    // Create a mock mentorship object for compatibility with existing KickStudentModal
    const mockMentorship = {
      menteeName: row.original.studentName!,
      menteeEnrollmentId: row.original.enrollmentId!, // Now we have the correct enrollment ID
      studentRolePromotion: row.original.studentRolePromotion,
    };

    modals.open({
      children: (
        <KickStudentModal
          row={{ original: mockMentorship } as any}
          kickStudent={kickStudent}
          refetchMentorships={async () => {
            refetchMentorAssignments();
            return Promise.resolve({} as any);
          }}
          seasonSlug={seasonSlug}
          onClose={() => modals.closeAll()}
        />
      ),
      size: 'lg',
      centered: true,
    });
  };

  const columns = useMemo<MRT_ColumnDef<MentorAssignmentDto>[]>(
    () => [
      {
        accessorKey: 'studentName',
        header: 'Student Name',
      },
      {
        accessorKey: 'mentorName',
        header: 'Mentor',
        accessorFn: (row) => {
          return row.mentorName || 'Unassigned';
        },
      },
      {
        accessorKey: 'studentRolePromotion',
        header: 'Student Role Promotion',
        accessorFn: (row) => {
          if (!row.studentRolePromotion) {
            return 'N/A';
          }
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
    data: students || [],
    ...getMantineTablePropsWithBanner(classes.table, false),
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
          id: 'studentName',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.studentId!,
    isMultiSortEvent: () => true,
    renderRowActions: ({ row }) => (
      <Flex gap="md">
        <Tooltip label="Kick Student">
          <ActionIcon variant="subtle" color="red" onClick={() => openKickStudentModal(row)}>
            <IconTrash />
          </ActionIcon>
        </Tooltip>
      </Flex>
    ),
    state: {
      pagination,
      isSaving: isKickingStudentStatus === 'pending',
    },
  });

  return <MantineReactTable table={table} />;
};

type MentorsTableProps = {
  mentorAssignments: MentorAssignmentDto[];
  refetchMentorAssignments: () => void;
};
