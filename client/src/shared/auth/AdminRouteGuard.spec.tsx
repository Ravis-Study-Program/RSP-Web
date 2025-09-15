import { render, screen } from '@test-utils';
import { describe, expect, it, vi } from 'vitest';
import AdminRouteGuard from './AdminRouteGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NotFoundMock = () => <div data-testid="not-found">Not Found</div>;

const mocks = vi.hoisted(() => ({
  useAuth0User: vi.fn(),
}));

vi.mock('@/shared/hooks/useAuth0User', () => ({
  useAuth0User: mocks.useAuth0User,
}));

vi.mock('react-router-dom', () => ({
  Outlet: () => <OutletMock />,
}));

vi.mock('../../pages/NotFound/NotFound.page', () => ({
  __esModule: true,
  default: () => <NotFoundMock />,
}));

describe('AdminRouteGuard', () => {
  it('renders null when auth0 is loading', () => {
    mocks.useAuth0User.mockReturnValue({
      isAuthenticated: true,
      isLoading: true,
      isAdmin: false,
    });

    render(<AdminRouteGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders Outlet when authenticated and user is admin', () => {
    mocks.useAuth0User.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      isAdmin: true,
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders NotFound when authenticated but user is not admin', () => {
    mocks.useAuth0User.mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      isAdmin: false,
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });

  it('renders NotFound when not authenticated', () => {
    mocks.useAuth0User.mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      isAdmin: false,
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });
});
