import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_EditActionButtons,
  useMantineReactTable,
  type MRT_ColumnDef,
  type MRT_Row,
  type MRT_TableOptions,
} from 'mantine-react-table';
import {
  ActionIcon,
  Autocomplete,
  Button,
  Flex,
  Pill,
  Stack,
  Text,
  Title,
  Tooltip,
} from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { modals } from '@mantine/modals';
import { LeetcodeDifficulty, mockLeetcodes, Problem } from '../Leetcode.data';
import {
  useCreateProblem,
  useDeleteProblem,
  useGetProblems,
  useUpdateProblem,
} from './LeetcodeTable.hooks';
import classes from './LeetcodeTable.module.css';

const LeetcodeTable = () => {
  const [validationErrors, setValidationErrors] = useState<Record<string, string | undefined>>({});

  const { mutateAsync: createProblem, status: isCreatingProblemStatus } = useCreateProblem();
  const {
    data: fetchedProblems = [],
    isError: isLoadingProblemsError,
    isFetching: isFetchingProblems,
    isLoading: isLoadingProblems,
  } = useGetProblems();
  const { mutateAsync: updateProblem, status: isUpdatingProblemStatus } = useUpdateProblem();
  const { mutateAsync: deleteProblem, status: isDeletingProblemStatus } = useDeleteProblem();

  // CREATE action
  const handleCreateProblem: MRT_TableOptions<Problem>['onCreatingRowSave'] = async ({
    values,
    exitCreatingMode,
  }) => {
    await createProblem(values);
    exitCreatingMode();
  };

  // UPDATE action
  const handleSaveProblem: MRT_TableOptions<Problem>['onEditingRowSave'] = async ({
    values,
    table,
  }) => {
    await updateProblem(values);
    table.setEditingRow(null); //Exit editing mode
  };

  // DELETE action
  const openDeleteConfirmModal = (row: MRT_Row<Problem>) => {
    modals.openConfirmModal({
      title: 'Delete Problem',
      children: (
        <Text>
          Are you sure you want to delete {mockLeetcodes[row.original.LeetcodeId].Title}? This
          action cannot be undone.
        </Text>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await deleteProblem(row.original.ProblemId?.toString());
        modals.closeAll();
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<Problem>[]>(
    () => [
      {
        accessorKey: 'StartDateTime',
        header: 'Attempt Date',
        Cell: ({ row }) => {
          const startFormatted = dayjs(row.original.StartDateTime).format('D MMM YYYY HH:mm:ss');
          return <Text size="sm">{startFormatted}</Text>;
        },
        Edit: ({ cell }) => {
          const date = cell.getValue<Date>() ?? null;
          const dateValue = date ? new Date(date) : null;

          return (
            <DateTimePicker
              withAsterisk
              clearable
              withSeconds
              label="Attempt Date"
              defaultValue={dateValue}
            />
          );
        },
      },
      {
        accessorKey: 'TimeTakenInMinutes',
        header: 'Time Taken (mins)',
        accessorFn: (row) => row.TimeTakenInMinutes,
        mantineEditTextInputProps: {
          type: 'number',
          required: true,
          error: validationErrors?.email,
          onFocus: () =>
            setValidationErrors({
              ...validationErrors,
              email: undefined,
            }),
        },
      },
      {
        accessorKey: 'LeetcodeId',
        id: 'Title',
        header: 'Title',
        Cell: ({ cell }) => {
          const index = cell.getValue<number>();
          const leetcode = mockLeetcodes[index];

          return <Text size="sm">{leetcode.Title}</Text>;
        },
        Edit: ({ cell }) => {
          const index = cell.getValue<number>();
          const defaultValue = index != null ? mockLeetcodes[index].Title : '';

          return (
            <Autocomplete
              withAsterisk
              label="Leetcode"
              data={mockLeetcodes.map((l) => l.Title)}
              defaultValue={defaultValue}
            />
          );
        },
      },
      {
        accessorKey: 'LeetcodeId',
        id: 'Difficulty',
        header: 'Difficulty',
        Cell: ({ cell }) => {
          const index = cell.getValue<number>();
          const leetcode = mockLeetcodes[index];

          let textClass = '';
          switch (leetcode.LeetcodeDifficulty) {
            case LeetcodeDifficulty.Easy:
              textClass = classes.textGreen;
              break;
            case LeetcodeDifficulty.Medium:
              textClass = classes.textYellow;
              break;
            case LeetcodeDifficulty.Hard:
              textClass = classes.textRed;
              break;
            default:
          }

          return (
            <Text size="sm" className={textClass}>
              {LeetcodeDifficulty[index]}
            </Text>
          );
        },
        Edit: () => null,
      },
      {
        accessorKey: 'LeetcodeId',
        id: 'Category',
        header: 'Category',
        Cell: ({ cell }) => {
          const index = cell.getValue<number>();
          const leetcode = mockLeetcodes[index];

          return (
            <Flex className={classes.categoryContainer}>
              {leetcode.LeetcodeCategories.map((category, index) => (
                <Pill size="sm" key={index} className={classes.category}>
                  {category.Name}
                </Pill>
              ))}
            </Flex>
          );
        },
        Edit: () => null,
      },
    ],
    [validationErrors]
  );

  const table = useMantineReactTable({
    columns,
    data: fetchedProblems,
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
    editDisplayMode: 'modal',
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'StartDateTime',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.ProblemId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingProblemsError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    onCreatingRowCancel: () => setValidationErrors({}),
    onCreatingRowSave: handleCreateProblem,
    onEditingRowCancel: () => setValidationErrors({}),
    onEditingRowSave: handleSaveProblem,
    renderCreateRowModalContent: ({ table, row, internalEditComponents }) => (
      <Stack>
        <Title order={3}>Create Problem</Title>
        {internalEditComponents}
        <Flex justify="flex-end" mt="xl">
          <MRT_EditActionButtons variant="text" table={table} row={row} />
        </Flex>
      </Stack>
    ),
    renderEditRowModalContent: ({ table, row, internalEditComponents }) => (
      <Stack>
        <Title order={3}>Edit Problem</Title>
        {internalEditComponents}
        <Flex justify="flex-end" mt="xl">
          <MRT_EditActionButtons variant="text" table={table} row={row} />
        </Flex>
      </Stack>
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
        Create New Problem
      </Button>
    ),
    state: {
      isLoading: isLoadingProblems,
      isSaving:
        isCreatingProblemStatus === 'pending' ||
        isUpdatingProblemStatus === 'pending' ||
        isDeletingProblemStatus === 'pending',
      showAlertBanner: isLoadingProblemsError,
      showProgressBars: isFetchingProblems,
    },
  });

  return <MantineReactTable table={table} />;
};

export default LeetcodeTable;
