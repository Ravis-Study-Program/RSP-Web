import { useMemo, useState } from 'react';
import { Flex, Group, MultiSelect, SegmentedControl, Text } from '@mantine/core';
import {
  useGetMentorAssignments,
  useGetSeasonWeeksBySeasonSlug,
  useListMockInterview,
  useListProblemAttempt,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { createOptionsFilter, getSeasonWeeks } from '@/shared/table/globalFilters';
import { LeetcodeTable } from '../Leetcode/LeetcodeTable/LeetcodeTable';
import { MockInterviewTable } from '../MockInterviews/MockInterviewTable/MockInterviewTable';
import { MentorsTable } from './MentorsTable';
import classes from '../Mentees/Mentees.module.css';

export default function MentorsPage() {
  const { seasonSlug } = useSeasonSlug();
  const { enrollmentId, seasonId } = useUserAndEnrollment(seasonSlug);
  const [section, setSection] = useState<'Portfolio' | 'Performance'>('Portfolio');
  const [selectedMentors, setSelectedMentors] = useState<string[]>([]);
  const [selectedStudents, setSelectedStudents] = useState<string[]>([]);
  const [selectedSeasonWeeks, setSelectedSeasonWeeks] = useState<string[]>([]);

  const { data: mentorAssignmentsResponse, refetch: refetchMentorAssignments } =
    useGetMentorAssignments(seasonSlug, {}, { query: { enabled: seasonSlug !== '' } });

  const { data: seasonWeeksResponse } = useGetSeasonWeeksBySeasonSlug(
    { SeasonSlug: seasonSlug },
    { query: { enabled: seasonSlug !== '' } }
  );

  // Get mentors and students from mentor assignments data
  const mentors = useMemo(() => {
    const assignments = mentorAssignmentsResponse?.responseBody?.assignments || [];
    const mentorNames = new Set<string>();
    assignments.forEach((assignment) => {
      if (assignment.mentorName) {
        mentorNames.add(assignment.mentorName);
      }
    });
    return Array.from(mentorNames).map((name) => ({
      label: name,
      value: name,
    }));
  }, [mentorAssignmentsResponse]);

  const students = useMemo(() => {
    const assignments = mentorAssignmentsResponse?.responseBody?.assignments || [];
    return assignments
      .filter((assignment) => assignment.studentName)
      .map((assignment) => ({
        label: assignment.studentName!,
        value: assignment.studentName!,
      }));
  }, [mentorAssignmentsResponse]);

  // Get mentor assignments filtered by selected mentors and students
  const filteredMentorAssignments = useMemo(() => {
    const assignments = mentorAssignmentsResponse?.responseBody?.assignments || [];
    return assignments.filter((assignment) => {
      // If no mentors and no students are selected, show all
      if (selectedMentors.length === 0 && selectedStudents.length === 0) {
        return true;
      }

      // Check if student is in selected students (or no students selected)
      const studentFilter =
        selectedStudents.length === 0 ||
        (assignment.studentName && selectedStudents.includes(assignment.studentName));

      // Check if mentor is in selected mentors (or no mentors selected)
      const mentorFilter =
        selectedMentors.length === 0 ||
        (assignment.mentorName && selectedMentors.includes(assignment.mentorName));

      return studentFilter && mentorFilter;
    });
  }, [mentorAssignmentsResponse, selectedMentors, selectedStudents]);

  const seasonWeeksOptions = getSeasonWeeks(seasonWeeksResponse?.responseBody?.seasonWeeks || []);

  const getUserIds = useMemo(() => {
    return filteredMentorAssignments.map((assignment) => assignment.studentId!);
  }, [filteredMentorAssignments]);

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useListMockInterview(
    {
      SeasonId: seasonId || undefined,
      IncludeCustom: true,
      IncludeLeetcode: true,
      IncludeBehavioural: true,
      UserIds: getUserIds,
    },
    { query: { enabled: getUserIds.length > 0 && seasonId !== null } }
  );

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useListProblemAttempt(
    {
      SeasonId: seasonId || undefined,
      IncludeCustom: false,
      IncludeLeetcode: true,
      UserIds: getUserIds,
    },
    { query: { enabled: getUserIds.length > 0 && seasonId !== null } }
  );

  const PortfolioComponent = useMemo(() => {
    return (
      <MentorsTable
        mentorAssignments={filteredMentorAssignments}
        refetchMentorAssignments={refetchMentorAssignments}
      />
    );
  }, [filteredMentorAssignments, refetchMentorAssignments]);

  const LeetcodeComponent = useMemo(() => {
    const selectedUserNames = new Set([...selectedMentors, ...selectedStudents]);
    const problemAttempts = (problemAttemptsResponse?.responseBody?.problemAttempts || []).filter(
      (problemAttempt) => {
        // User filter - show if user is in selected users (or no users selected)
        const userFilter =
          selectedUserNames.size === 0 ||
          (problemAttempt.user?.name && selectedUserNames.has(problemAttempt.user.name));

        // Season weeks filter
        const seasonWeekFilter =
          selectedSeasonWeeks.length === 0 ||
          (problemAttempt.seasonWeekId != null &&
            selectedSeasonWeeks.includes(problemAttempt.seasonWeekId.toString()));

        return userFilter && seasonWeekFilter;
      }
    );

    return (
      <LeetcodeTable
        refetchProblemAttempts={refetchProblemAttempts}
        problemAttempts={problemAttempts}
        enrollmentId={enrollmentId || ''}
        enableEditing={false}
        showAuthor
        showCategory
      />
    );
  }, [
    refetchProblemAttempts,
    problemAttemptsResponse,
    selectedMentors,
    selectedStudents,
    selectedSeasonWeeks,
    enrollmentId,
  ]);

  const MockInterviewComponent = useMemo(() => {
    const selectedUserNames = new Set([...selectedMentors, ...selectedStudents]);
    const mocks = (mockInterviewsResponse?.responseBody?.mockInterviews || []).filter((mock) => {
      // User filter - show if interviewer or interviewee is in selected users (or no users selected)
      const userFilter =
        selectedUserNames.size === 0 ||
        (mock.interviewer?.name && selectedUserNames.has(mock.interviewer.name)) ||
        (mock.interviewee?.name && selectedUserNames.has(mock.interviewee.name));

      // Season weeks filter
      const seasonWeekFilter =
        selectedSeasonWeeks.length === 0 ||
        (mock.seasonWeekId != null && selectedSeasonWeeks.includes(mock.seasonWeekId.toString()));

      return userFilter && seasonWeekFilter;
    });

    return (
      <MockInterviewTable
        refetchMockInterviews={refetchMockInterviews}
        mockInterviews={mocks}
        seasonId={seasonId || ''}
        enableEditing={false}
      />
    );
  }, [
    refetchMockInterviews,
    mockInterviewsResponse,
    selectedMentors,
    selectedStudents,
    selectedSeasonWeeks,
    seasonId,
  ]);

  const PerformanceComponent = (
    <>
      <Text fw={500} size="md" my={18} mt={0}>
        Leetcode
      </Text>
      {LeetcodeComponent}
      <Text fw={500} size="md" my={18} mt={24}>
        Mock Interviews
      </Text>
      {MockInterviewComponent}
    </>
  );

  return (
    <>
      <Flex justify="space-between" align="flex-start">
        <Group mb="lg" justify="flex-start">
          <SegmentedControl
            onChange={(value: any) =>
              setTimeout(() => {
                setSection(value);
              }, 150)
            }
            mb={10}
            className={classes.control}
            size="sm"
            color="blue"
            data={['Portfolio', 'Performance']}
          />
        </Group>
        <Group mb="lg" justify="flex-end">
          <MultiSelect
            classNames={{ inputField: classes.inputField }}
            label="Mentors"
            placeholder="Pick value(s)"
            data={mentors}
            filter={createOptionsFilter()}
            miw={150}
            searchable
            nothingFoundMessage="Nothing found..."
            value={selectedMentors}
            onChange={(values) => {
              setSelectedMentors(values as string[]);
            }}
          />
          <MultiSelect
            classNames={{ inputField: classes.inputField }}
            label="Students"
            placeholder="Pick value(s)"
            data={students}
            filter={createOptionsFilter()}
            miw={150}
            searchable
            nothingFoundMessage="Nothing found..."
            value={selectedStudents}
            onChange={(values) => {
              setSelectedStudents(values as string[]);
            }}
          />
          {section === 'Performance' && (
            <MultiSelect
              classNames={{ inputField: classes.inputField }}
              label="Season Week"
              placeholder="Pick value(s)"
              data={seasonWeeksOptions}
              filter={createOptionsFilter()}
              miw={150}
              searchable
              nothingFoundMessage="Nothing found..."
              value={selectedSeasonWeeks}
              onChange={(values) => {
                setSelectedSeasonWeeks(values as string[]);
              }}
            />
          )}
        </Group>
      </Flex>
      {section === 'Portfolio' ? PortfolioComponent : null}

      {section === 'Performance' ? PerformanceComponent : null}
    </>
  );
}
