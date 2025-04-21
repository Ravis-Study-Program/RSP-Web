import { render, screen } from '@test-utils';
import { describe, expect, it, vi } from 'vitest';
import AdminRouteGuard from './AdminRouteGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NotFoundMock = () => <div data-testid="not-found">Not Found</div>;

const mocks = vi.hoisted(() => ({
  useAuth0: vi.fn(),
  useGetCurrentUser: vi.fn(),
}));

vi.mock('@auth0/auth0-react', () => ({
  useAuth0: mocks.useAuth0,
}));

vi.mock('react-router-dom', () => ({
  Outlet: () => <OutletMock />,
}));

vi.mock('@/generated/api/client', () => ({
  useGetCurrentUser: mocks.useGetCurrentUser,
}));

vi.mock('../../pages/NotFound/NotFound.page', () => ({
  __esModule: true,
  default: () => <NotFoundMock />,
}));

describe('AdminRouteGuard', () => {
  it('renders null when either auth0 or user data is loading', () => {
    mocks.useAuth0.mockReturnValue({ isAuthenticated: true, isLoading: true });
    mocks.useGetCurrentUser.mockReturnValue({ data: undefined, isLoading: true });

    render(<AdminRouteGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders Outlet when authenticated and user is admin', () => {
    mocks.useAuth0.mockReturnValue({ isAuthenticated: true, isLoading: false });
    mocks.useGetCurrentUser.mockReturnValue({
      isLoading: false,
      data: {
        responseBody: {
          user: {
            isAdmin: true,
          },
        },
      },
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders NotFound when authenticated but user is not admin', () => {
    mocks.useAuth0.mockReturnValue({ isAuthenticated: true, isLoading: false });
    mocks.useGetCurrentUser.mockReturnValue({
      isLoading: false,
      data: {
        responseBody: {
          user: {
            isAdmin: false,
          },
        },
      },
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });

  it('renders NotFound when not authenticated', () => {
    mocks.useAuth0.mockReturnValue({ isAuthenticated: false, isLoading: false });
    mocks.useGetCurrentUser.mockReturnValue({
      isLoading: false,
      data: {
        responseBody: {
          user: {
            isAdmin: false,
          },
        },
      },
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });
});
