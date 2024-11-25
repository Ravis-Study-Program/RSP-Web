import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import SeasonRoleViewRouter from './SeasonRoleViewRouter';

const mocks = vi.hoisted(() => ({
  useParams: vi.fn(),
  useGetIsUserEnrolled: vi.fn(),
}));

vi.mock('react-router-dom', () => ({
  useParams: mocks.useParams,
}));

vi.mock('@/generated/api/client', () => ({
  useGetIsUserEnrolled: mocks.useGetIsUserEnrolled,
  SeasonRole: {
    Student: 'Student',
    Mentor: 'Mentor',
    Coordinator: 'Coordinator',
  },
}));

vi.mock('@/pages/NotFound/NotFound.page', () => ({
  default: () => <div data-testid="not-found">Not Found</div>,
}));

describe('SeasonRoleViewRouter', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders NotFoundPage if seasonSlug is not present', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: undefined });
    mocks.useGetIsUserEnrolled.mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: false,
      data: null,
    });

    render(<SeasonRoleViewRouter />);

    expect(screen.queryByTestId('not-found')).toBeInTheDocument();
  });

  it('renders null while loading or fetching', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useGetIsUserEnrolled.mockReturnValue({
      isLoading: true,
      isFetching: true,
      isError: false,
      data: null,
    });

    const { container } = render(<SeasonRoleViewRouter />);
    expect(container.firstChild).toBeNull();
  });

  it('renders NotFoundPage if the user is not enrolled', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useGetIsUserEnrolled.mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: false,
      data: {
        responseBody: { isEnrolled: false },
      },
    });

    render(<SeasonRoleViewRouter />);

    expect(screen.queryByTestId('not-found')).toBeInTheDocument();
  });

  it('renders studentView for Student role', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useGetIsUserEnrolled.mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: false,
      data: {
        responseBody: { isEnrolled: true, role: 'Student' },
      },
    });

    render(
      <SeasonRoleViewRouter studentView={<div data-testid="student-view">Student View</div>} />
    );

    expect(screen.queryByTestId('student-view')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders mentorView for Mentor role', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useGetIsUserEnrolled.mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: false,
      data: {
        responseBody: { isEnrolled: true, role: 'Mentor' },
      },
    });

    render(<SeasonRoleViewRouter mentorView={<div data-testid="mentor-view">Mentor View</div>} />);

    expect(screen.queryByTestId('mentor-view')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders coordinatorView for Coordinator role', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useGetIsUserEnrolled.mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: false,
      data: {
        responseBody: { isEnrolled: true, role: 'Coordinator' },
      },
    });

    render(
      <SeasonRoleViewRouter
        coordinatorView={<div data-testid="coordinator-view">Coordinator View</div>}
      />
    );

    expect(screen.queryByTestId('coordinator-view')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders NotFoundPage if the role is invalid or no view is provided', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useGetIsUserEnrolled.mockReturnValue({
      isLoading: false,
      isFetching: false,
      isError: false,
      data: {
        responseBody: { isEnrolled: true, role: 'InvalidRole' },
      },
    });

    render(<SeasonRoleViewRouter />);

    expect(screen.queryByTestId('not-found')).toBeInTheDocument();
  });
});
