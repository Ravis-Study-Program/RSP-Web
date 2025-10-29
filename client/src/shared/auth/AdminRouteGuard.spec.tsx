import { render, screen } from '@test-utils';
import { describe, expect, it, vi } from 'vitest';
import AdminRouteGuard from './AdminRouteGuard';

const OutletMock = () => <div data-testid="outlet">Outlet</div>;
const NotFoundMock = () => <div data-testid="not-found">Not Found</div>;

const mocks = vi.hoisted(() => ({
  useGetCurrentUser: vi.fn(),
}));

vi.mock('@/generated/api/client', () => ({
  useGetCurrentUser: mocks.useGetCurrentUser,
}));

vi.mock('react-router-dom', () => ({
  Outlet: () => <OutletMock />,
}));

vi.mock('../../pages/NotFound/NotFound.page', () => ({
  __esModule: true,
  default: () => <NotFoundMock />,
}));

describe('AdminRouteGuard', () => {
  it('renders null when loading', () => {
    mocks.useGetCurrentUser.mockReturnValue({
      data: null,
      isLoading: true,
    });

    render(<AdminRouteGuard />);
    expect(screen.queryByTestId('outlet')).toBeNull();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders Outlet when user is admin', () => {
    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: {
            isAdmin: true,
          },
        },
      },
      isLoading: false,
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('outlet')).toBeInTheDocument();
    expect(screen.queryByTestId('not-found')).toBeNull();
  });

  it('renders NotFound when user is not admin', () => {
    mocks.useGetCurrentUser.mockReturnValue({
      data: {
        responseBody: {
          user: {
            isAdmin: false,
          },
        },
      },
      isLoading: false,
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });

  it('renders NotFound when no user data', () => {
    mocks.useGetCurrentUser.mockReturnValue({
      data: null,
      isLoading: false,
    });

    render(<AdminRouteGuard />);
    expect(screen.getByTestId('not-found')).toBeInTheDocument();
    expect(screen.queryByTestId('outlet')).toBeNull();
  });
});
