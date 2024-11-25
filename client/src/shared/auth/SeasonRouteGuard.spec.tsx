import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import SeasonRouteGuard from './SeasonRouteGuard';

const mocks = vi.hoisted(() => ({
  useParams: vi.fn(),
  useUserAndEnrollment: vi.fn(),
}));

vi.mock('react-router-dom', () => ({
  useParams: mocks.useParams,
  Outlet: () => <div data-testid="outlet">Outlet</div>,
}));

vi.mock('@/pages/NotFound/NotFound.page', () => ({
  default: () => <div data-testid="not-found">Not Found</div>,
}));

vi.mock('../hooks/useUserAndEnrollment', () => ({
  useUserAndEnrollment: mocks.useUserAndEnrollment,
}));

describe('SeasonRouteGuard', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders NotFoundPage if seasonSlug is not present', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: undefined });
    mocks.useUserAndEnrollment.mockReturnValue({
      isLoading: false,
    });

    render(<SeasonRouteGuard />);

    expect(screen.queryByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });

  it('renders null while loading', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useUserAndEnrollment.mockReturnValue({
      isLoading: true,
    });

    const { container } = render(<SeasonRouteGuard />);
    expect(container.firstChild).toBeNull();
  });

  it('renders Outlet when the user is an admin', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useUserAndEnrollment.mockReturnValue({
      user: null,
      isAdmin: true,
      role: null,
      isLoading: false,
    });

    render(<SeasonRouteGuard />);

    expect(screen.queryByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders NotFoundPage if the user or role is null for non-admin', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useUserAndEnrollment.mockReturnValue({
      user: null,
      isAdmin: false,
      role: null,
      isLoading: false,
    });

    render(<SeasonRouteGuard />);

    expect(screen.queryByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });

  it('renders Outlet when the user is found and has a role', () => {
    mocks.useParams.mockReturnValue({ seasonSlug: 'test-season-slug' });
    mocks.useUserAndEnrollment.mockReturnValue({
      user: { id: 'test-user-id' },
      isAdmin: false,
      role: { id: 'student' },
      isLoading: false,
    });

    render(<SeasonRouteGuard />);

    expect(screen.queryByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });
});
