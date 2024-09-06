import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { SeasonRole } from '../Season';
import ViewRouter from './ViewRouter';

const CoordinatorViewMock = () => <div>Coordinator View</div>;
const MentorViewMock = () => <div>Mentor View</div>;
const StudentViewMock = () => <div>Student View</div>;

const mocks = vi.hoisted(() => ({
  useRouteLoaderData: vi.fn(),
}));

vi.mock('react-router-dom', () => ({
  useRouteLoaderData: mocks.useRouteLoaderData,
}));

describe('ViewRouter', () => {
  it('should render CoordinatorView for Coordinator role', () => {
    mocks.useRouteLoaderData.mockReturnValue(SeasonRole.Coordinator);
    render(
      <ViewRouter
        views={{
          coordinator: CoordinatorViewMock,
          mentor: MentorViewMock,
          student: StudentViewMock,
        }}
      />
    );
    expect(screen.getByText('Coordinator View')).toBeDefined();
  });

  it('should render MentorView for Mentor role', () => {
    mocks.useRouteLoaderData.mockReturnValue(SeasonRole.Mentor);
    render(
      <ViewRouter
        views={{
          coordinator: CoordinatorViewMock,
          mentor: MentorViewMock,
          student: StudentViewMock,
        }}
      />
    );
    expect(screen.getByText('Mentor View')).toBeDefined();
  });

  it('should render StudentView for Student role', () => {
    mocks.useRouteLoaderData.mockReturnValue(SeasonRole.Student);
    render(
      <ViewRouter
        views={{
          coordinator: CoordinatorViewMock,
          mentor: MentorViewMock,
          student: StudentViewMock,
        }}
      />
    );
    expect(screen.getByText('Student View')).toBeDefined();
  });
});
